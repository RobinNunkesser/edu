#!/usr/bin/env python3

from __future__ import annotations

import json
import re
from dataclasses import dataclass
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
LECTURES_WIKI = ROOT.parent.parent.parent.parent / "wikis" / "lectures"
OUTPUT_DIR = ROOT / "src" / "WebSite" / "wwwroot" / "content" / "site"
WIKI_BASE_URL = "https://github.com/isd-nunkesser/lectures.wiki/wiki"
BLOB_BASE_URL = "https://github.com/isd-nunkesser/lectures.wiki/blob/master"


@dataclass(frozen=True)
class LectureDefinition:
    source_name: str
    output_key: str
    title: str


LECTURES = [
    LectureDefinition("AppDevelopment", "app-development", "App Development"),
    LectureDefinition("AI", "ai", "AI"),
    LectureDefinition("TechnischeInformatik", "technische-informatik", "Technische Informatik"),
]

LINK_RE = re.compile(r"\[([^\]]+)\]\(([^)]+)\)")
HEADING_RE = re.compile(r"^(#{1,6})\s+(.*)$")


def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    for lecture in LECTURES:
        source_path = LECTURES_WIKI / f"{lecture.source_name}.md"
        output_path = OUTPUT_DIR / f"lecture-template.{lecture.output_key}.de.json"
        content = build_template_content(lecture, source_path.read_text(encoding="utf-8"))
        output_path.write_text(json.dumps(content, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(f"Wrote {output_path.relative_to(ROOT)}")


def build_template_content(lecture: LectureDefinition, markdown: str) -> dict:
    blocks = parse_blocks(markdown)
    sections = []

    organizational_links = parse_organizational_links(blocks)
    if organizational_links:
        sections.append(make_section("Organisatorisches", organizational_links))

    assessment_section = parse_assessment_section(blocks)
    if assessment_section is not None:
        sections.append(assessment_section)

    demo_links = parse_demonstrator_links(blocks)
    if demo_links:
        sections.append(make_section("Interaktive Übungen / Demonstratoren", demo_links))

    exercise_links, mock_exam_links = parse_exercise_links(blocks.get(("Übungen",), []))
    if exercise_links:
        sections.append(make_section("Übungsblätter", exercise_links))
    if mock_exam_links:
        sections.append(make_section("Probeklausuren", mock_exam_links))

    slide_links = parse_simple_bullet_section(blocks.get(("Folien",), []))
    if slide_links:
        sections.append(make_section("Folien", slide_links))

    recording_links = parse_recording_links(blocks)
    if recording_links:
        sections.append(make_section("Aufzeichnungen", recording_links, ""))

    return {
        "slug": lecture.output_key,
        "title": lecture.title,
        "semester": parse_semester(blocks) or "Vorlesung aus lectures.wiki",
        "summary": "",
        "sourceUrl": f"{WIKI_BASE_URL}/{lecture.source_name}",
        "highlightTags": [],
        "sections": sections,
    }


def parse_blocks(markdown: str) -> dict[tuple[str, ...], list[str]]:
    blocks: dict[tuple[str, ...], list[str]] = {}
    current_path: list[str] = []

    for raw_line in markdown.splitlines():
        match = HEADING_RE.match(raw_line)
        if match:
            level = len(match.group(1))
            title = match.group(2).strip()
            current_path = current_path[: level - 1] + [title]
            blocks.setdefault(tuple(current_path), [])
            continue

        if current_path:
            blocks.setdefault(tuple(current_path), []).append(raw_line)

    return blocks


def parse_semester(blocks: dict[tuple[str, ...], list[str]]) -> str | None:
    for path in blocks:
        if len(path) == 2 and path[0] == "Organisatorisches" and path[1].startswith("Termin im "):
            return path[1].replace("Termin im ", "", 1).strip()
    return None


def parse_organizational_links(blocks: dict[tuple[str, ...], list[str]]) -> list[dict]:
    schedule_block = next(
        (lines for path, lines in blocks.items() if len(path) == 2 and path[0] == "Organisatorisches" and path[1].startswith("Termin im ")),
        [],
    )
    rows = parse_markdown_table(schedule_block)
    links = []

    for row in rows:
        day = row[0].strip() if len(row) > 0 else ""
        kind = row[1].strip() if len(row) > 1 else ""
        time = row[2].strip() if len(row) > 2 else ""
        room = row[3].strip() if len(row) > 3 else ""
        if not (day and kind and time and room):
            continue

        label = {
            "VL": "Vorlesung",
            "ÜB": "Übung",
        }.get(kind, kind)
        links.append(
            {
                "title": label,
                "url": "",
                "description": f"{day} {time}, Raum {room}",
            }
        )

    return links


def parse_assessment_section(blocks: dict[tuple[str, ...], list[str]]) -> dict | None:
    lines = blocks.get(("Organisatorisches", "Prüfungsform"), [])
    bullet_lines = [line.strip() for line in lines if line.strip().startswith("-")]
    if not bullet_lines:
        return None

    bullets = [line[1:].strip() for line in bullet_lines]
    is_project = any("GitHub Assignment" in bullet or "Projekt" in bullet for bullet in bullets)
    if is_project:
        links = []
        team_description = next((bullet for bullet in bullets if bullet.lower().startswith("teams bis ")), None)
        if team_description:
            links.append({"title": "Teamgröße", "url": "", "description": team_description})

        for title, url in parse_links_from_lines(lines):
            links.append({"title": title, "url": resolve_url(url), "description": ""})

        return make_section("Prüfungsvariante Projekt", links)

    duration = next((extract_duration(bullet) for bullet in bullets if extract_duration(bullet)), None)
    if duration is None:
        duration = bullets[-1] if bullets else ""

    return make_section(
        "Prüfungsvariante Klausur",
        [{"title": "Dauer", "url": "", "description": duration}],
    )


def parse_demonstrator_links(blocks: dict[tuple[str, ...], list[str]]) -> list[dict]:
    lines = blocks.get(("Organisatorisches", "ISD Companion App"), [])
    return [
        {"title": title, "url": resolve_url(url), "description": ""}
        for title, url in parse_links_from_lines(lines)
        if "<img" not in title.lower() and not url.startswith("./images/")
    ]


def parse_exercise_links(lines: list[str]) -> tuple[list[dict], list[dict]]:
    exercise_links = []
    mock_exam_links = []

    for title, url in parse_links_from_lines(lines):
        target = mock_exam_links if "Probeklausur" in title else exercise_links
        target.append({"title": title, "url": resolve_url(url), "description": ""})

    return exercise_links, mock_exam_links


def parse_simple_bullet_section(lines: list[str]) -> list[dict]:
    return [
        {"title": title, "url": resolve_url(url), "description": ""}
        for title, url in parse_links_from_lines(lines)
    ]


def parse_recording_links(blocks: dict[tuple[str, ...], list[str]]) -> list[dict]:
    links = []
    for path, lines in blocks.items():
        if len(path) != 2 or path[0] != "Vorlesungsvideos":
            continue

        for row in parse_markdown_table(lines):
            row_links = parse_links_from_line(row[0]) if row else []
            if not row_links:
                continue

            title, url = row_links[0]
            description = row[1].strip() if len(row) > 1 else ""
            links.append({"title": title, "url": resolve_url(url), "description": description})

    return links


def parse_markdown_table(lines: list[str]) -> list[list[str]]:
    rows = []
    separator_consumed = False

    for raw_line in lines:
        line = raw_line.strip()
        if "|" not in line:
            continue

        parts = [part.strip() for part in line.strip("|").split("|")]
        if not separator_consumed:
            if all(part and set(part.replace(":", "")) <= {"-"} for part in parts):
                separator_consumed = True
            continue

        rows.append(parts)

    return rows


def parse_links_from_lines(lines: list[str]) -> list[tuple[str, str]]:
    links: list[tuple[str, str]] = []
    for line in lines:
        links.extend(parse_links_from_line(line))
    return links


def parse_links_from_line(line: str) -> list[tuple[str, str]]:
    return [(match.group(1).strip(), match.group(2).strip()) for match in LINK_RE.finditer(line)]


def resolve_url(url: str) -> str:
    if url.startswith("http://") or url.startswith("https://") or url.startswith("/"):
        return url

    if url.startswith("./artifacts/"):
        return f"{BLOB_BASE_URL}/{url[2:]}"

    if url.startswith("./"):
        return f"{BLOB_BASE_URL}/{url[2:]}"

    return f"{WIKI_BASE_URL}/{url.replace(' ', '-') }"


def extract_duration(text: str) -> str | None:
    match = re.search(r"(\d+\s*Minuten)", text)
    return match.group(1) if match else None


def make_section(title: str, links: list[dict], intro: str = "") -> dict:
    return {
        "title": title,
        "intro": intro,
        "links": links,
    }


if __name__ == "__main__":
    main()
#!/usr/bin/env python3

from __future__ import annotations

import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
LECTURES_WIKI = ROOT.parent.parent.parent.parent / "wikis" / "lectures"
OUTPUT_DIR = ROOT / "src" / "WebSite" / "wwwroot" / "content" / "site"
WIKI_BASE_URL = "https://github.com/isd-nunkesser/lectures.wiki/wiki"
BLOB_BASE_URL = "https://github.com/isd-nunkesser/lectures.wiki/blob/master"
INDEX_FILE_NAME = "teaching-lectures.de.json"


LINK_RE = re.compile(r"\[([^\]]+)\]\(([^)]+)\)")
HEADING_RE = re.compile(r"^(#{1,6})\s+(.*)$")
CAMEL_CASE_RE = re.compile(r"(?<=[a-z0-9])(?=[A-Z])")

INTERNAL_DEMONSTRATORS = {
    "TechnischeInformatik": [("Minimaler Spannbaum (MST)", "/study/minimaler-spannbaum")],
}

TITLE_OVERRIDES = {
    "iOSDevelopment": "iOS Development",
    "ITConsulting": "IT Consulting",
    "ITProjektmanagement": "IT Projektmanagement",
}

SLUG_OVERRIDES = {
    "iOSDevelopment": "ios-development",
    "ITConsulting": "it-consulting",
    "ITProjektmanagement": "it-projektmanagement",
}

def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    lectures = discover_lectures()
    de_index_items = []

    for source_name in lectures:
        source_path = LECTURES_WIKI / f"{source_name}.md"
        de_content = build_template_content(source_name, source_path.read_text(encoding="utf-8"))
        output_path = OUTPUT_DIR / f"lecture-template.{de_content['slug']}.de.json"
        output_path.write_text(json.dumps(de_content, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        de_index_items.append(
            {
                "slug": de_content["slug"],
                "title": de_content["title"],
                "semester": de_content["semester"],
            }
        )
        print(f"Wrote {output_path.relative_to(ROOT)}")

    index_path = OUTPUT_DIR / INDEX_FILE_NAME
    de_index_items.sort(key=lambda item: item["title"].lower())
    index_path.write_text(json.dumps(de_index_items, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Wrote {index_path.relative_to(ROOT)}")


def discover_lectures() -> list[str]:
    lectures = []
    for path in sorted(LECTURES_WIKI.glob("*.md")):
        if path.name.startswith("_"):
            continue

        content = path.read_text(encoding="utf-8")
        if content.startswith("# Organisatorisches"):
            lectures.append(path.stem)

    return lectures


def build_template_content(source_name: str, markdown: str) -> dict:
    blocks = parse_blocks(markdown)
    sections = []
    slug = slugify_source_name(source_name)
    title = humanize_source_name(source_name)

    organizational_links = parse_organizational_links(blocks)
    if organizational_links:
        sections.append(make_section("Organisatorisches", organizational_links))

    assessment_section = parse_assessment_section(blocks)
    if assessment_section is not None:
        sections.append(assessment_section)

    demo_links = parse_demonstrator_links(source_name, blocks)
    if demo_links:
        sections.append(make_section("Interaktive Übungen / Demonstratoren", demo_links))

    exercise_links, mock_exam_links = parse_exercise_links(blocks)
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

    summary = build_summary(sections, "de")
    highlight_tags = build_highlight_tags(sections, "de")

    return {
        "slug": slug,
        "title": title,
        "semester": parse_semester(blocks) or "Vorlesung aus lectures.wiki",
        "summary": summary,
        "sourceUrl": f"{WIKI_BASE_URL}/{source_name}",
        "highlightTags": highlight_tags,
        "sections": sections,
    }
def humanize_source_name(source_name: str) -> str:
    if source_name in TITLE_OVERRIDES:
        return TITLE_OVERRIDES[source_name]

    if source_name.isupper():
        return source_name

    return CAMEL_CASE_RE.sub(" ", source_name).replace("  ", " ").strip()


def slugify_source_name(source_name: str) -> str:
    if source_name in SLUG_OVERRIDES:
        return SLUG_OVERRIDES[source_name]

    return humanize_source_name(source_name).lower().replace(" ", "-")


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
        if len(row) >= 4:
            day = row[0].strip()
            kind = row[1].strip()
            time = row[2].strip()
            room = row[3].strip()
            if not (day and kind and time and room):
                continue

            label = {
                "VL": "Vorlesung",
                "ÜB": "Übung",
            }.get(kind, kind)
        elif len(row) >= 3:
            day = row[0].strip()
            time = row[1].strip()
            room = row[2].strip()
            if not (day and time and room):
                continue

            label = "Termin"
        else:
            continue

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


def parse_demonstrator_links(source_name: str, blocks: dict[tuple[str, ...], list[str]]) -> list[dict]:
    lines = blocks.get(("Organisatorisches", "ISD Companion App"), [])
    links = [
        {"title": title, "url": resolve_url(url), "description": ""}
        for title, url in parse_links_from_lines(lines)
        if "<img" not in title.lower() and not url.startswith("./images/")
    ]

    for title, url in INTERNAL_DEMONSTRATORS.get(source_name, []):
        links.insert(0, {"title": title, "url": url, "description": ""})

    return links


def parse_exercise_links(blocks: dict[tuple[str, ...], list[str]]) -> tuple[list[dict], list[dict]]:
    exercise_links = []
    mock_exam_links = []

    exercise_lines: list[str] = []
    for path, lines in blocks.items():
        if len(path) == 1 and is_exercise_section(path[0]):
            exercise_lines.extend(lines)

    for title, url in parse_links_from_lines(exercise_lines):
        target = mock_exam_links if "Probeklausur" in title else exercise_links
        target.append({"title": title, "url": resolve_url(url), "description": ""})

    return exercise_links, mock_exam_links


def is_exercise_section(title: str) -> bool:
    normalized = title.lower()
    return "übung" in normalized or "uebung" in normalized or "exercise" in normalized


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


def build_highlight_tags(sections: list[dict], language: str) -> list[str]:
    titles = {section["title"] for section in sections}
    tags = []

    if "Prüfungsvariante Projekt" in titles or "Assessment Variant Project" in titles:
        tags.append("Project" if language == "en" else "Projekt")
    elif "Prüfungsvariante Klausur" in titles or "Assessment Variant Exam" in titles:
        tags.append("Exam" if language == "en" else "Klausur")

    if "Übungsblätter" in titles or "Probeklausuren" in titles or "Exercise Sheets" in titles or "Mock Exams" in titles:
        tags.append("Exercises" if language == "en" else "Uebungen")
    if "Interaktive Übungen / Demonstratoren" in titles or "Interactive Exercises / Demonstrators" in titles:
        tags.append("Demonstrators" if language == "en" else "Demonstratoren")
    if "Folien" in titles or "Slides" in titles:
        tags.append("Slides" if language == "en" else "Folien")
    if "Aufzeichnungen" in titles or "Recordings" in titles:
        tags.append("Videos")

    return tags


def build_summary(sections: list[dict], language: str) -> str:
    titles = {section["title"] for section in sections}
    is_project = "Prüfungsvariante Projekt" in titles or "Assessment Variant Project" in titles
    prefix = "Project-based" if language == "en" and is_project else "Exam-focused" if language == "en" else "Projektorientierte" if is_project else "Klausurorientierte"

    parts = []
    if "Übungsblätter" in titles or "Probeklausuren" in titles or "Exercise Sheets" in titles or "Mock Exams" in titles:
        parts.append("exercise material" if language == "en" else "Uebungsmaterial")
    if "Interaktive Übungen / Demonstratoren" in titles or "Interactive Exercises / Demonstrators" in titles:
        parts.append("demonstrators" if language == "en" else "Demonstratoren")
    if "Folien" in titles or "Slides" in titles:
        parts.append("slides" if language == "en" else "Folien")
    if "Aufzeichnungen" in titles or "Recordings" in titles:
        parts.append("recordings" if language == "en" else "Aufzeichnungen")

    if not parts:
        return f"{prefix} course page from lectures.wiki." if language == "en" else f"{prefix} Veranstaltungsseite aus lectures.wiki."

    if len(parts) == 1:
        details = parts[0]
    else:
        details = ", ".join(parts[:-1]) + (f" and {parts[-1]}" if language == "en" else f" und {parts[-1]}")

    return f"{prefix} course page with {details}." if language == "en" else f"{prefix} Veranstaltungsseite mit {details}."


def make_section(title: str, links: list[dict], intro: str = "") -> dict:
    return {
        "title": title,
        "intro": intro,
        "links": links,
    }


if __name__ == "__main__":
    main()
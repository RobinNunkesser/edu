using Shared.Domain;

namespace Shared.Application;

public sealed class SiteContentService
{
    private const string LecturesWikiRepository = "https://github.com/isd-nunkesser/lectures.wiki";

    public IReadOnlyList<HomeAreaViewModel> GetAreas() =>
    [
        new(
            "Lehre",
            "Lehrveranstaltungen mit Unterseiten, Übungen, Folien, Videos und weiterführenden Materialien.",
            SiteRoutes.Teaching,
            ["Vorlesungen", "Übungen", "Lehrmaterialien"]),
        new(
            "Forschung",
            "Schwerpunkte, Themenfelder und Anknüpfungspunkte für Projekt- und Abschlussarbeiten.",
            SiteRoutes.Research,
            ["Schwerpunkte", "Projekte", "Arbeiten"]),
        new(
            "Software",
            "Lehrsoftware, Open-Source-Artefakte und wiederverwendbare Pakete aus Projekten und Lehrveranstaltungen.",
            SiteRoutes.Software,
            ["Apps", "Pakete", "Repos"])
    ];

    public IReadOnlyList<LectureOverviewViewModel> GetTeachingLectures() => GetLectureDetails()
        .Select(lecture => new LectureOverviewViewModel(
            lecture.Slug,
            lecture.Title,
            lecture.Semester,
            lecture.Summary,
            SiteRoutes.Lecture(lecture.Slug),
            lecture.HighlightTags))
        .ToArray();

    public LectureDetailViewModel? GetLectureBySlug(string slug) => GetLectureDetails()
        .FirstOrDefault(lecture => string.Equals(lecture.Slug, slug, StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyList<LectureDetailViewModel> GetLectureDetails() =>
    [
        new(
            "app-development",
            "App Development",
            "6. Semester",
            "Die Struktur orientiert sich zunächst am vorhandenen lectures-Wiki und bündelt organisatorische Informationen, Projektformat, Folien und Vorlesungsvideos auf einer Seite.",
            $"{LecturesWikiRepository}/wiki/AppDevelopment",
            ["Projektformat", "GitHub Assignment", "Folien", "Videos"],
            [
                new LectureSectionViewModel(
                    "Organisatorisches",
                    "Termin, Modulzuordnung und Prüfungsform für das Sommersemester 2026.",
                    [
                        new ResourceLinkViewModel("Vorlesung", string.Empty, "Di 09:30 - 11:00, H4.3-E00-100"),
                        new ResourceLinkViewModel("Übung", string.Empty, "Di 11:00 - 12:30, H3.3-E01-220"),
                        new ResourceLinkViewModel("Prüfungsform", string.Empty, "Präsentation plus Projektabgabe, Teams bis 3 Personen")
                    ]),
                new LectureSectionViewModel(
                    "Übungen und Projektarbeit",
                    "Die Lehrveranstaltung ist projektorientiert. Die prominenten Arbeitsmaterialien sind daher Assignment, Abgabekriterien und die in der Vorlesung referenzierten Inhalte.",
                    [
                        new ResourceLinkViewModel("GitHub Assignment", "https://classroom.github.com/a/nFtBV3dj", "Projektgrundlage für die Lehrveranstaltung"),
                        new ResourceLinkViewModel("Abgabekriterien", $"{LecturesWikiRepository}/wiki/Abgabekriterien", "Allgemeine Kriterien für Softwareprojekte"),
                        new ResourceLinkViewModel("Lehrbuch App Engineering", "https://link.springer.com/book/10.1007/978-3-662-67476-5", "Begleitendes Lehrbuch")
                    ]),
                new LectureSectionViewModel(
                    "Lehrmaterialien",
                    "Folien und Vorlesungsvideos aus dem lectures-Wiki.",
                    [
                        new ResourceLinkViewModel("Folie KI-gestützte Entwicklung", "https://mymoodle.hshl.de/pluginfile.php/2056914/mod_folder/content/0/KIgestuetzteEntwicklung.pdf", "Einführung in die Veranstaltung"),
                        new ResourceLinkViewModel("Folie GitHub Copilot", "https://mymoodle.hshl.de/pluginfile.php/2056914/mod_folder/content/0/GitHubCopilot.pdf", "Werkzeugschwerpunkt"),
                        new ResourceLinkViewModel("Vorlesung 1 SS2026", "https://hshl.cloud.panopto.eu/Panopto/Pages/Viewer.aspx?id=8158f9be-cbdf-4024-8647-b42c0096caa3", "Panopto-Aufzeichnung vom 14.04.2026")
                    ])
            ]),
        new(
            "technische-informatik",
            "Technische Informatik",
            "3. Semester",
            "Diese Unterseite übernimmt den Schwerpunkt aus dem lectures-Wiki: Übungen und Probeklausuren stehen prominent im Vordergrund, ergänzt um Companion- und Vorlesungsmaterialien.",
            $"{LecturesWikiRepository}/wiki/TechnischeInformatik",
            ["Übungen", "Probeklausuren", "Companion", "Folien"],
            [
                new LectureSectionViewModel(
                    "Organisatorisches",
                    "Rahmendaten aus dem Wintersemester 2024/2025.",
                    [
                        new ResourceLinkViewModel("Vorlesung", string.Empty, "Do 09:30 - 11:00, H4.3-E00-110"),
                        new ResourceLinkViewModel("Übung", string.Empty, "Do 11:00 - 12:30, H4.2-E00-140"),
                        new ResourceLinkViewModel("Prüfungsform", string.Empty, "Klausur im Prüfungszeitraum, zusammen mit Grundlagen der Programmierung")
                    ]),
                new LectureSectionViewModel(
                    "Prominente Übungen",
                    "Die PDF-Übungen und Probeklausuren stammen direkt aus der lectures-Wiki-Struktur.",
                    [
                        new ResourceLinkViewModel("Übung Informationsdarstellung", $"{LecturesWikiRepository}/blob/master/artifacts/ti/TIUebung01Informationsdarstellung.pdf", "Aufgabenblatt"),
                        new ResourceLinkViewModel("Übung Boolesche Algebra", $"{LecturesWikiRepository}/blob/master/artifacts/ti/TIUebung02BoolescheAlgebra.pdf", "Aufgabenblatt"),
                        new ResourceLinkViewModel("Übung Boolesche Minimierung", $"{LecturesWikiRepository}/blob/master/artifacts/ti/TIUebung03BoolescheMinimierung.pdf", "Aufgabenblatt"),
                        new ResourceLinkViewModel("Probeklausur Teil 1", $"{LecturesWikiRepository}/blob/master/artifacts/ti/TIProbeklausurTeil01.pdf", "Prüfungsvorbereitung"),
                        new ResourceLinkViewModel("Probeklausur Teil 2", $"{LecturesWikiRepository}/blob/master/artifacts/ti/TIProbeklausurTeil02.pdf", "Prüfungsvorbereitung")
                    ]),
                new LectureSectionViewModel(
                    "Weitere Inhalte",
                    "Begleitende digitale und interaktive Materialien.",
                    [
                        new ResourceLinkViewModel("ISD Companion", $"{LecturesWikiRepository}/wiki/ISDCompanion", "Companion-App mit Übungen zur Vorlesung"),
                        new ResourceLinkViewModel("Externe DNF/CNF-Seite", "https://www.mathematik.uni-marburg.de/~thormae/lectures/ti1/code/normalform/index.html", "Zusätzliche Erklärung"),
                        new ResourceLinkViewModel("Externe Karnaugh-Veitch-Map", "https://www.mathematik.uni-marburg.de/~thormae/lectures/ti1/code/karnaughmap/index.html", "Zusätzliche Erklärung"),
                        new ResourceLinkViewModel("Externer Quine-McCluskey-Algorithmus", "https://www.mathematik.uni-marburg.de/~thormae/lectures/ti1/code/qmc/index.html", "Zusätzliche Erklärung")
                    ]),
                new LectureSectionViewModel(
                    "Folien und Videos",
                    "Auswahl aus den im Wiki verlinkten Materialien.",
                    [
                        new ResourceLinkViewModel("Folie Einführung", "https://mymoodle.hshl.de/pluginfile.php/1999141/mod_folder/content/0/TI01Einfuehrung.pdf", "Einführungsfolien"),
                        new ResourceLinkViewModel("Folie Boolesche Algebra", "https://mymoodle.hshl.de/pluginfile.php/1999141/mod_folder/content/0/TI04BoolescheAlgebra.pdf", "Themenblock"),
                        new ResourceLinkViewModel("Vorlesung 1 WS2024/2025", "https://hshl.cloud.panopto.eu/Panopto/Pages/Viewer.aspx?id=9978ed30-f8b1-4767-9b1b-b1f7008fea49", "Panopto-Aufzeichnung")
                    ])
            ]),
        new(
            "kuenstliche-intelligenz",
            "Künstliche Intelligenz",
            "6. Semester",
            "Auch diese Seite ist zunächst aus dem lectures-Wiki abgeleitet und stellt Projektideen, Assignment-nahe Arbeitsweise, Folien und Code-Einstieg in den Vordergrund.",
            $"{LecturesWikiRepository}/wiki/AI",
            ["Projektideen", "Folien", "Code", "Videos"],
            [
                new LectureSectionViewModel(
                    "Organisatorisches",
                    "Rahmendaten aus dem Sommersemester 2026.",
                    [
                        new ResourceLinkViewModel("Vorlesung", string.Empty, "Mi 12:00 - 13:30, H4.1-E00-110"),
                        new ResourceLinkViewModel("Übung", string.Empty, "Mo 11:00 - 12:30, ungerade Kalenderwochen, H4.1-E00-140"),
                        new ResourceLinkViewModel("Prüfungsform", string.Empty, "Präsentation plus Projektabgabe, Teams bis 3 Personen")
                    ]),
                new LectureSectionViewModel(
                    "Projektorientierte Übungen",
                    "Die Veranstaltung arbeitet projektbasiert statt über klassische Übungsblätter.",
                    [
                        new ResourceLinkViewModel("GitHub Assignment", "https://classroom.github.com/a/QZgdkZsE", "Projektbasis im Kurs"),
                        new ResourceLinkViewModel("AIMA Lehrbuch", "http://aima.cs.berkeley.edu", "Artificial Intelligence: A Modern Approach"),
                        new ResourceLinkViewModel("Projektideen im Wiki", $"{LecturesWikiRepository}/wiki/AI", "Demonstratoren, Benchmarks und eigene Ideen")
                    ]),
                new LectureSectionViewModel(
                    "Code und Materialien",
                    "Codequellen und ausgewählte Lehrmaterialien aus dem Wiki.",
                    [
                        new ResourceLinkViewModel("Ports", "https://github.com/Italbytz/nuget-ports-algorithms", "C#-Portierungen"),
                        new ResourceLinkViewModel("Adapters", "https://github.com/Italbytz/nuget-adapters-algorithms", "Adapter-Schicht"),
                        new ResourceLinkViewModel("Folie Intelligente Agenten", "https://mymoodle.hshl.de/pluginfile.php/2056583/mod_folder/content/0/IntelligenteAgenten.pdf", "Themeneinstieg"),
                        new ResourceLinkViewModel("Folie Lernen aus Beispielen", "https://mymoodle.hshl.de/pluginfile.php/2017628/mod_folder/content/0/AI09LernenAusBeispielen.pdf", "Lernkapitel"),
                        new ResourceLinkViewModel("Vorlesung 1 SS2026", "https://hshl.cloud.panopto.eu/Panopto/Pages/Viewer.aspx?id=72907e05-cad9-445d-816a-b42600ddc4e5", "Panopto-Aufzeichnung")
                    ])
            ])
    ];
}

public sealed record HomeAreaViewModel(
    string Title,
    string Summary,
    string Route,
    IReadOnlyList<string> Highlights);

public sealed record LectureOverviewViewModel(
    string Slug,
    string Title,
    string Semester,
    string Summary,
    string Route,
    IReadOnlyList<string> HighlightTags);

public sealed record LectureDetailViewModel(
    string Slug,
    string Title,
    string Semester,
    string Summary,
    string SourceUrl,
    IReadOnlyList<string> HighlightTags,
    IReadOnlyList<LectureSectionViewModel> Sections);

public sealed record LectureSectionViewModel(
    string Title,
    string Intro,
    IReadOnlyList<ResourceLinkViewModel> Links);

public sealed record ResourceLinkViewModel(
    string Title,
    string Url,
    string Description);
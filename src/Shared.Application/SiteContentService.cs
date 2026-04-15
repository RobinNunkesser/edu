using Shared.Domain;

namespace Shared.Application;

public sealed class SiteContentService
{
    private const string LecturesWikiRepository = "https://github.com/isd-nunkesser/lectures.wiki";

    public IReadOnlyList<HomeAreaViewModel> GetAreas(SiteLanguage language) =>
    [
        new(
            Translate(language, "Lehre", "Teaching"),
            Translate(language, "Lehrveranstaltungen mit Unterseiten, Übungen, Folien, Videos und weiterführenden Materialien.", "Courses with dedicated subpages, exercises, slides, videos, and supporting materials."),
            SiteRoutes.Teaching(language),
            TranslateList(language, ["Vorlesungen", "Übungen", "Lehrmaterialien"], ["Courses", "Exercises", "Materials"])),
        new(
            Translate(language, "Forschung", "Research"),
            Translate(language, "Schwerpunkte, Themenfelder und Anknüpfungspunkte für Projekt- und Abschlussarbeiten.", "Focus areas, research themes, and entry points for projects and theses."),
            SiteRoutes.Research(language),
            TranslateList(language, ["Schwerpunkte", "Projekte", "Arbeiten"], ["Topics", "Projects", "Theses"])),
        new(
            Translate(language, "Software", "Software"),
            Translate(language, "Lehrsoftware, Open-Source-Artefakte und wiederverwendbare Pakete aus Projekten und Lehrveranstaltungen.", "Teaching software, open-source artifacts, and reusable packages from projects and courses."),
            SiteRoutes.Software(language),
            TranslateList(language, ["Apps", "Pakete", "Repos"], ["Apps", "Packages", "Repos"]))
    ];

    public IReadOnlyList<LectureOverviewViewModel> GetTeachingLectures(SiteLanguage language) => GetLectureDetails(language)
        .Select(lecture => new LectureOverviewViewModel(
            lecture.Slug,
            lecture.Title,
            lecture.Semester,
            lecture.Summary,
            SiteRoutes.Lecture(language, lecture.Slug),
            lecture.HighlightTags))
        .ToArray();

    public LectureDetailViewModel? GetLectureBySlug(SiteLanguage language, string slug) => GetLectureDetails(language)
        .FirstOrDefault(lecture => string.Equals(lecture.Slug, slug, StringComparison.OrdinalIgnoreCase));

    private IReadOnlyList<LectureDetailViewModel> GetLectureDetails(SiteLanguage language) =>
    [
        new(
            "app-development",
            "App Development",
            Translate(language, "6. Semester", "6th semester"),
            Translate(language, "Die Struktur orientiert sich zunächst am vorhandenen lectures-Wiki und bündelt organisatorische Informationen, Projektformat, Folien und Vorlesungsvideos auf einer Seite.", "This first version follows the existing lectures wiki and bundles course logistics, project setup, slides, and lecture recordings on a single page."),
            $"{LecturesWikiRepository}/wiki/AppDevelopment",
            TranslateList(language, ["Projektformat", "GitHub Assignment", "Folien", "Videos"], ["Project format", "GitHub Assignment", "Slides", "Videos"]),
            [
                new LectureSectionViewModel(
                    Translate(language, "Organisatorisches", "Course logistics"),
                    Translate(language, "Termin, Modulzuordnung und Prüfungsform für das Sommersemester 2026.", "Schedule, module context, and assessment format for summer term 2026."),
                    [
                        new ResourceLinkViewModel(Translate(language, "Vorlesung", "Lecture"), string.Empty, Translate(language, "Di 09:30 - 11:00, H4.3-E00-100", "Tue 09:30 - 11:00, H4.3-E00-100")),
                        new ResourceLinkViewModel(Translate(language, "Übung", "Lab"), string.Empty, Translate(language, "Di 11:00 - 12:30, H3.3-E01-220", "Tue 11:00 - 12:30, H3.3-E01-220")),
                        new ResourceLinkViewModel(Translate(language, "Prüfungsform", "Assessment"), string.Empty, Translate(language, "Präsentation plus Projektabgabe, Teams bis 3 Personen", "Presentation plus project submission, teams of up to three"))
                    ]),
                new LectureSectionViewModel(
                    Translate(language, "Übungen und Projektarbeit", "Exercises and project work"),
                    Translate(language, "Die Lehrveranstaltung ist projektorientiert. Die prominenten Arbeitsmaterialien sind daher Assignment, Abgabekriterien und die in der Vorlesung referenzierten Inhalte.", "The course is project-driven. The most relevant working materials are therefore the assignment, submission criteria, and the materials referenced during the lectures."),
                    [
                        new ResourceLinkViewModel("GitHub Assignment", "https://classroom.github.com/a/nFtBV3dj", Translate(language, "Projektgrundlage für die Lehrveranstaltung", "Project baseline for the course")),
                        new ResourceLinkViewModel(Translate(language, "Abgabekriterien", "Submission criteria"), $"{LecturesWikiRepository}/wiki/Abgabekriterien", Translate(language, "Allgemeine Kriterien für Softwareprojekte", "General criteria for software projects")),
                        new ResourceLinkViewModel(Translate(language, "Lehrbuch App Engineering", "Textbook App Engineering"), "https://link.springer.com/book/10.1007/978-3-662-67476-5", Translate(language, "Begleitendes Lehrbuch", "Companion textbook"))
                    ]),
                new LectureSectionViewModel(
                    Translate(language, "Lehrmaterialien", "Course materials"),
                    Translate(language, "Folien und Vorlesungsvideos aus dem lectures-Wiki.", "Slides and lecture recordings from the lectures wiki."),
                    [
                        new ResourceLinkViewModel(Translate(language, "Folie KI-gestützte Entwicklung", "Slide deck on AI-assisted development"), "https://mymoodle.hshl.de/pluginfile.php/2056914/mod_folder/content/0/KIgestuetzteEntwicklung.pdf", Translate(language, "Einführung in die Veranstaltung", "Course introduction")),
                        new ResourceLinkViewModel("GitHub Copilot", "https://mymoodle.hshl.de/pluginfile.php/2056914/mod_folder/content/0/GitHubCopilot.pdf", Translate(language, "Werkzeugschwerpunkt", "Tooling focus")),
                        new ResourceLinkViewModel(Translate(language, "Vorlesung 1 SS2026", "Lecture 1 SS2026"), "https://hshl.cloud.panopto.eu/Panopto/Pages/Viewer.aspx?id=8158f9be-cbdf-4024-8647-b42c0096caa3", Translate(language, "Panopto-Aufzeichnung vom 14.04.2026", "Panopto recording from 2026-04-14"))
                    ])
            ]),
        new(
            "technische-informatik",
            Translate(language, "Technische Informatik", "Technical Computer Engineering"),
            Translate(language, "3. Semester", "3rd semester"),
            Translate(language, "Diese Unterseite übernimmt den Schwerpunkt aus dem lectures-Wiki: Übungen und Probeklausuren stehen prominent im Vordergrund, ergänzt um Companion- und Vorlesungsmaterialien.", "This page keeps the emphasis from the lectures wiki: exercises and mock exams are placed front and center, complemented by companion and lecture materials."),
            $"{LecturesWikiRepository}/wiki/TechnischeInformatik",
            TranslateList(language, ["Übungen", "Probeklausuren", "Companion", "Folien"], ["Exercises", "Mock exams", "Companion", "Slides"]),
            [
                new LectureSectionViewModel(
                    Translate(language, "Organisatorisches", "Course logistics"),
                    Translate(language, "Rahmendaten aus dem Wintersemester 2024/2025.", "Course setup from winter term 2024/2025."),
                    [
                        new ResourceLinkViewModel(Translate(language, "Vorlesung", "Lecture"), string.Empty, Translate(language, "Do 09:30 - 11:00, H4.3-E00-110", "Thu 09:30 - 11:00, H4.3-E00-110")),
                        new ResourceLinkViewModel(Translate(language, "Übung", "Lab"), string.Empty, Translate(language, "Do 11:00 - 12:30, H4.2-E00-140", "Thu 11:00 - 12:30, H4.2-E00-140")),
                        new ResourceLinkViewModel(Translate(language, "Prüfungsform", "Assessment"), string.Empty, Translate(language, "Klausur im Prüfungszeitraum, zusammen mit Grundlagen der Programmierung", "Written exam during the exam period, combined with Fundamentals of Programming"))
                    ]),
                new LectureSectionViewModel(
                    Translate(language, "Prominente Übungen", "Featured exercises"),
                    Translate(language, "Die PDF-Übungen und Probeklausuren stammen direkt aus der lectures-Wiki-Struktur.", "The exercise sheets and mock exams are linked directly from the lectures wiki structure."),
                    [
                        new ResourceLinkViewModel(Translate(language, "Übung Informationsdarstellung", "Exercise on information representation"), $"{LecturesWikiRepository}/blob/master/artifacts/ti/TIUebung01Informationsdarstellung.pdf", Translate(language, "Aufgabenblatt", "Exercise sheet")),
                        new ResourceLinkViewModel(Translate(language, "Übung Boolesche Algebra", "Exercise on Boolean algebra"), $"{LecturesWikiRepository}/blob/master/artifacts/ti/TIUebung02BoolescheAlgebra.pdf", Translate(language, "Aufgabenblatt", "Exercise sheet")),
                        new ResourceLinkViewModel(Translate(language, "Übung Boolesche Minimierung", "Exercise on Boolean minimization"), $"{LecturesWikiRepository}/blob/master/artifacts/ti/TIUebung03BoolescheMinimierung.pdf", Translate(language, "Aufgabenblatt", "Exercise sheet")),
                        new ResourceLinkViewModel(Translate(language, "Probeklausur Teil 1", "Mock exam part 1"), $"{LecturesWikiRepository}/blob/master/artifacts/ti/TIProbeklausurTeil01.pdf", Translate(language, "Prüfungsvorbereitung", "Exam preparation")),
                        new ResourceLinkViewModel(Translate(language, "Probeklausur Teil 2", "Mock exam part 2"), $"{LecturesWikiRepository}/blob/master/artifacts/ti/TIProbeklausurTeil02.pdf", Translate(language, "Prüfungsvorbereitung", "Exam preparation"))
                    ]),
                new LectureSectionViewModel(
                    Translate(language, "Weitere Inhalte", "Additional materials"),
                    Translate(language, "Begleitende digitale und interaktive Materialien.", "Supporting digital and interactive materials."),
                    [
                        new ResourceLinkViewModel("ISD Companion", $"{LecturesWikiRepository}/wiki/ISDCompanion", Translate(language, "Companion-App mit Übungen zur Vorlesung", "Companion app with lecture exercises")),
                        new ResourceLinkViewModel(Translate(language, "Externe DNF/CNF-Seite", "External DNF/CNF page"), "https://www.mathematik.uni-marburg.de/~thormae/lectures/ti1/code/normalform/index.html", Translate(language, "Zusätzliche Erklärung", "Additional explanation")),
                        new ResourceLinkViewModel(Translate(language, "Externe Karnaugh-Veitch-Map", "External Karnaugh-Veitch map"), "https://www.mathematik.uni-marburg.de/~thormae/lectures/ti1/code/karnaughmap/index.html", Translate(language, "Zusätzliche Erklärung", "Additional explanation")),
                        new ResourceLinkViewModel(Translate(language, "Externer Quine-McCluskey-Algorithmus", "External Quine-McCluskey algorithm"), "https://www.mathematik.uni-marburg.de/~thormae/lectures/ti1/code/qmc/index.html", Translate(language, "Zusätzliche Erklärung", "Additional explanation"))
                    ]),
                new LectureSectionViewModel(
                    Translate(language, "Folien und Videos", "Slides and videos"),
                    Translate(language, "Auswahl aus den im Wiki verlinkten Materialien.", "Selection of materials linked from the wiki."),
                    [
                        new ResourceLinkViewModel(Translate(language, "Folie Einführung", "Introduction slides"), "https://mymoodle.hshl.de/pluginfile.php/1999141/mod_folder/content/0/TI01Einfuehrung.pdf", Translate(language, "Einführungsfolien", "Introductory slides")),
                        new ResourceLinkViewModel(Translate(language, "Folie Boolesche Algebra", "Boolean algebra slides"), "https://mymoodle.hshl.de/pluginfile.php/1999141/mod_folder/content/0/TI04BoolescheAlgebra.pdf", Translate(language, "Themenblock", "Topic block")),
                        new ResourceLinkViewModel(Translate(language, "Vorlesung 1 WS2024/2025", "Lecture 1 WS2024/2025"), "https://hshl.cloud.panopto.eu/Panopto/Pages/Viewer.aspx?id=9978ed30-f8b1-4767-9b1b-b1f7008fea49", Translate(language, "Panopto-Aufzeichnung", "Panopto recording"))
                    ])
            ]),
        new(
            "kuenstliche-intelligenz",
            Translate(language, "Künstliche Intelligenz", "Artificial Intelligence"),
            Translate(language, "6. Semester", "6th semester"),
            Translate(language, "Auch diese Seite ist zunächst aus dem lectures-Wiki abgeleitet und stellt Projektideen, Assignment-nahe Arbeitsweise, Folien und Code-Einstieg in den Vordergrund.", "This page is also derived from the lectures wiki and focuses on project ideas, assignment-driven workflows, slides, and code entry points."),
            $"{LecturesWikiRepository}/wiki/AI",
            TranslateList(language, ["Projektideen", "Folien", "Code", "Videos"], ["Project ideas", "Slides", "Code", "Videos"]),
            [
                new LectureSectionViewModel(
                    Translate(language, "Organisatorisches", "Course logistics"),
                    Translate(language, "Rahmendaten aus dem Sommersemester 2026.", "Course setup from summer term 2026."),
                    [
                        new ResourceLinkViewModel(Translate(language, "Vorlesung", "Lecture"), string.Empty, Translate(language, "Mi 12:00 - 13:30, H4.1-E00-110", "Wed 12:00 - 13:30, H4.1-E00-110")),
                        new ResourceLinkViewModel(Translate(language, "Übung", "Lab"), string.Empty, Translate(language, "Mo 11:00 - 12:30, ungerade Kalenderwochen, H4.1-E00-140", "Mon 11:00 - 12:30, odd calendar weeks, H4.1-E00-140")),
                        new ResourceLinkViewModel(Translate(language, "Prüfungsform", "Assessment"), string.Empty, Translate(language, "Präsentation plus Projektabgabe, Teams bis 3 Personen", "Presentation plus project submission, teams of up to three"))
                    ]),
                new LectureSectionViewModel(
                    Translate(language, "Projektorientierte Übungen", "Project-based exercises"),
                    Translate(language, "Die Veranstaltung arbeitet projektbasiert statt über klassische Übungsblätter.", "The course is project-based rather than centered around classic exercise sheets."),
                    [
                        new ResourceLinkViewModel("GitHub Assignment", "https://classroom.github.com/a/QZgdkZsE", Translate(language, "Projektbasis im Kurs", "Project baseline inside the course")),
                        new ResourceLinkViewModel("AIMA", "http://aima.cs.berkeley.edu", "Artificial Intelligence: A Modern Approach"),
                        new ResourceLinkViewModel(Translate(language, "Projektideen im Wiki", "Project ideas in the wiki"), $"{LecturesWikiRepository}/wiki/AI", Translate(language, "Demonstratoren, Benchmarks und eigene Ideen", "Demonstrators, benchmarks, and original project ideas"))
                    ]),
                new LectureSectionViewModel(
                    Translate(language, "Code und Materialien", "Code and materials"),
                    Translate(language, "Codequellen und ausgewählte Lehrmaterialien aus dem Wiki.", "Code sources and selected teaching materials from the wiki."),
                    [
                        new ResourceLinkViewModel("Ports", "https://github.com/Italbytz/nuget-ports-algorithms", Translate(language, "C#-Portierungen", "C# ports")),
                        new ResourceLinkViewModel("Adapters", "https://github.com/Italbytz/nuget-adapters-algorithms", Translate(language, "Adapter-Schicht", "Adapter layer")),
                        new ResourceLinkViewModel(Translate(language, "Folie Intelligente Agenten", "Slide deck on intelligent agents"), "https://mymoodle.hshl.de/pluginfile.php/2056583/mod_folder/content/0/IntelligenteAgenten.pdf", Translate(language, "Themeneinstieg", "Topic introduction")),
                        new ResourceLinkViewModel(Translate(language, "Folie Lernen aus Beispielen", "Slide deck on learning from examples"), "https://mymoodle.hshl.de/pluginfile.php/2017628/mod_folder/content/0/AI09LernenAusBeispielen.pdf", Translate(language, "Lernkapitel", "Learning chapter")),
                        new ResourceLinkViewModel(Translate(language, "Vorlesung 1 SS2026", "Lecture 1 SS2026"), "https://hshl.cloud.panopto.eu/Panopto/Pages/Viewer.aspx?id=72907e05-cad9-445d-816a-b42600ddc4e5", Translate(language, "Panopto-Aufzeichnung", "Panopto recording"))
                    ])
            ])
    ];

    private static string Translate(SiteLanguage language, string german, string english) => language == SiteLanguage.En ? english : german;

    private static IReadOnlyList<string> TranslateList(SiteLanguage language, IReadOnlyList<string> german, IReadOnlyList<string> english) => language == SiteLanguage.En ? english : german;
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
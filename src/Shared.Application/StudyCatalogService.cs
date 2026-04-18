using Content.Schema;
using Shared.Domain;

namespace Shared.Application;

public sealed class StudyCatalogService
{
    public CatalogLandingSnapshot GetLandingSnapshot()
    {
        var document = CreateSeedDocument();

        return new CatalogLandingSnapshot(
            Title: "Study Companion im Web",
            Intro: "Das Repository startet als GitHub-Pages-faehige Webanwendung mit klar getrennten Schichten fuer Fachlogik, Web-UI und Importpfade.",
            CatalogRoute: StudyRoutes.Study,
            Sections: document.Sections
                .Select(section => new CatalogSectionViewModel(
                    section.Title,
                    section.Description,
                    section.Groups
                        .SelectMany(group => group.Topics)
                        .Select(topic => new CatalogTopicViewModel(
                            topic.Title,
                            topic.Summary,
                            topic.SourceSystem,
                            topic.IsImported,
                            StudyRoutes.Topic(topic.Slug)))
                    .ToArray()))
                .ToArray());
    }

    public IReadOnlyList<CatalogSectionViewModel> GetSections() => GetLandingSnapshot().Sections;

    private static StudyCatalogDocument CreateSeedDocument()
    {
        return new StudyCatalogDocument(
            "seed",
            [
                new StudySectionDocument(
                    "grundlagen",
                    "Grundlagen",
                    "Erste webgerechte Reprasentation von Themen, die spaeter aus bestehenden Companion-Quellen importiert werden.",
                    [
                        new StudyGroupDocument(
                            "Digitale Logik",
                            [
                                new StudyTopicDocument("normal-form", "Normalform", "Katalogeintrag fuer digitale Logik und erste Detailseite.", "ISD Companion", true, "NormalFormPage"),
                                new StudyTopicDocument("quine-mccluskey", "Quine-McCluskey", "Vorbereitung einer weiteren importierten Detailseite.", "ISD Companion", false, "QMCAlgorithmPage")
                            ])
                    ]),
                new StudySectionDocument(
                    "aufgaben-und-pruefungen",
                    "Aufgaben und Pruefungen",
                    "Zielpfad fuer spaetere Exam-Generator-Importe und aufgabenorientierte Webansichten.",
                    [
                        new StudyGroupDocument(
                            "Exam Generator",
                            [
                                new StudyTopicDocument("exercise-catalog", "Exercise Catalog", "Platzhalter fuer generierte Aufgabenkataloge.", "Exam Generator", false, "ExerciseCatalogPage"),
                                new StudyTopicDocument("gdi-uebungen", "GDI Uebungsstudio", "Verdichtete produktive Uebungsseite fuer mehrere GDI-Aufgabentypen auf einer Route.", "Exam Generator / edu", true, "GdiExerciseStudioPage"),
                                new StudyTopicDocument("binaere-addition", "Binaere Addition", "POC fuer ein druckbares Einzelaufgabenblatt mit neutralem Aufgabendokument.", "Exam Generator / edu", true, "BinaryAdditionExerciseDocument"),
                                new StudyTopicDocument("binaer-zu-dezimal", "Binaer zu Dezimal", "Importierter Umwandlungstyp ueber dieselbe Dokumentstruktur.", "Exam Generator / edu", true, "BinaryToDecimalExerciseDocument"),
                                new StudyTopicDocument("dezimal-zu-binaer", "Dezimal zu Binaer", "Importierter Umwandlungstyp ueber dieselbe Dokumentstruktur.", "Exam Generator / edu", true, "DecimalToBinaryExerciseDocument"),
                                new StudyTopicDocument("seitenersetzung", "Seitenersetzung", "Importierte Tabellenaufgabe zur Seitenersetzung mit gemeinsamer Worksheet-Struktur.", "Exam Generator / edu", true, "PageReplacementExerciseDocument"),
                                new StudyTopicDocument("kuerzeste-wege", "Kuerzeste Wege", "Importierte Graph-und-Tabellen-Aufgabe ueber dieselbe Dokumentstruktur.", "Exam Generator / edu", true, "ShortestPathExerciseDocument"),
                                new StudyTopicDocument("spannbaum", "Minimaler Spannbaum", "Importierte Graphaufgabe mit freiem Antwortbereich ueber dieselbe Dokumentstruktur.", "Exam Generator / edu", true, "SpanningTreeExerciseDocument"),
                                new StudyTopicDocument("zweierkomplement", "Zweierkomplement", "Importierter Darstellungstyp ueber dieselbe Dokumentstruktur.", "Exam Generator / edu", true, "TwosComplementExerciseDocument")
                            ])
                    ]),
                new StudySectionDocument(
                    "betriebssysteme-quiz",
                    "Betriebssysteme",
                    "Interaktives Wahr/Falsch-Quiz zu Betriebssystemen aus dem ISD-Vorlesungsinhalt.",
                    [
                        new StudyGroupDocument(
                            "Quiz",
                            [
                                new StudyTopicDocument("quiz-betriebssysteme", "Betriebssysteme Quiz", "Wahr/Falsch-Fragen zu Betriebssystemen aus Italbytz.Exam.OperatingSystems.", "Italbytz.Exam.OperatingSystems", true, "QuizOperatingSystems")
                            ])
                    ]),
                new StudySectionDocument(
                    "netzwerke-quiz",
                    "Netzwerke",
                    "Interaktives Wahr/Falsch-Quiz zu Computernetzwerken aus dem ISD-Vorlesungsinhalt.",
                    [
                        new StudyGroupDocument(
                            "Quiz",
                            [
                                new StudyTopicDocument("quiz-netzwerke", "Netzwerke Quiz", "Wahr/Falsch-Fragen zu Netzwerken aus Italbytz.Exam.Networking.", "Italbytz.Exam.Networking", true, "QuizNetworking")
                            ])
                    ])
            ]);
    }
}

public sealed record CatalogLandingSnapshot(
    string Title,
    string Intro,
    string CatalogRoute,
    IReadOnlyList<CatalogSectionViewModel> Sections);

public sealed record CatalogSectionViewModel(
    string Title,
    string Description,
    IReadOnlyList<CatalogTopicViewModel> Topics);

public sealed record CatalogTopicViewModel(
    string Title,
    string Summary,
    string SourceSystem,
    bool IsImported,
    string Route);
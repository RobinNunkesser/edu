using System.Globalization;
using System.Text.Json;
using Content.Schema;
using Italbytz.Graph;
using Italbytz.Graph.Abstractions;
using Italbytz.Graph.Visualization;
using Shared.Domain;
using System.Net.Http.Json;

namespace Shared.Application;

public sealed class StudyTopicService
{
    private const string MinimalSpanningTreeSlug = "minimaler-spannbaum";
    private const string BinaryAdditionSlug = "binaere-addition";
    private const string BinaryAdditionSlugEn = "binary-addition";
    private const string GdiExercisesSlug = "gdi-uebungen";
    private const string GdiExercisesSlugEn = "gdi-exercises";
    private const string BinaryToDecimalSlug = "binaer-zu-dezimal";
    private const string BinaryToDecimalSlugEn = "binary-to-decimal";
    private const string DecimalToBinarySlug = "dezimal-zu-binaer";
    private const string DecimalToBinarySlugEn = "decimal-to-binary";
    private const string TwosComplementSlug = "zweierkomplement";
    private const string TwosComplementSlugEn = "twos-complement";
    private const string RomaniaSearchSlug = "rumaenien-suche";
    private const string RomaniaSearchSlugEn = "romania-search";
    private const string NQueensSlug = "n-damen";
    private const string NQueensSlugEn = "n-queens";
    private const string GraphVisualizationSlug = "graph-visualisierung";
    private const string GraphVisualizationSlugEn = "graph-visualization";

    private static readonly JsonSerializerOptions ExerciseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly bool _disableContentCache;
    private readonly Dictionary<string, ExerciseDocumentViewModel?> _exerciseDocumentCache = new();

    public StudyTopicService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _disableContentCache = IsLocalDevelopment(httpClient.BaseAddress);
    }

    public StudyTopicCardViewModel GetFeaturedTopic(SiteLanguage language)
    {
        var isEnglish = language == SiteLanguage.En;

        return new StudyTopicCardViewModel(
            Title: isEnglish ? "Minimum Spanning Tree" : "Minimaler Spannbaum",
            Summary: isEnglish
                ? "Graph rendering POC for edu: an interactive SVG view over the existing MST solver from Italbytz.Graph."
                : "Graph-Rendering-POC fuer edu: eine interaktive SVG-Ansicht ueber dem bestehenden MST-Solver aus Italbytz.Graph.",
            Kicker: "ISD Companion / Italbytz.Graph",
            Route: StudyRoutes.Topic(language, MinimalSpanningTreeSlug),
            Tags: isEnglish
                ? ["Graphs", "Prim", "SVG POC"]
                : ["Graphen", "Prim", "SVG-POC"]);
    }

    public async Task<StudyTopicDetailViewModel?> GetTopicDetailAsync(SiteLanguage language, string slug)
    {
        if (!IsSupportedSlug(slug))
        {
            return null;
        }

        return IsExerciseSlug(slug)
            ? await BuildExerciseTopicAsync(language, slug)
            : string.Equals(slug, RomaniaSearchSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, RomaniaSearchSlugEn, StringComparison.OrdinalIgnoreCase)
            ? BuildRomaniaSearchTopic(language, slug)
            : string.Equals(slug, NQueensSlug, StringComparison.OrdinalIgnoreCase)
                || string.Equals(slug, NQueensSlugEn, StringComparison.OrdinalIgnoreCase)
                ? BuildNQueensTopic(language, slug)
            : BuildMinimalSpanningTreeTopic(language, slug);
    }

    private static bool IsSupportedSlug(string slug)
        => string.Equals(slug, MinimalSpanningTreeSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, GdiExercisesSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, GdiExercisesSlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, BinaryAdditionSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, BinaryAdditionSlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, BinaryToDecimalSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, BinaryToDecimalSlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, DecimalToBinarySlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, DecimalToBinarySlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, TwosComplementSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, TwosComplementSlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, RomaniaSearchSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, RomaniaSearchSlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, NQueensSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, NQueensSlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, GraphVisualizationSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, GraphVisualizationSlugEn, StringComparison.OrdinalIgnoreCase);

    private static bool IsExerciseSlug(string slug)
        => string.Equals(slug, GdiExercisesSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, GdiExercisesSlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, BinaryAdditionSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, BinaryAdditionSlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, BinaryToDecimalSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, BinaryToDecimalSlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, DecimalToBinarySlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, DecimalToBinarySlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, TwosComplementSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, TwosComplementSlugEn, StringComparison.OrdinalIgnoreCase);

    public async Task<ExerciseDocumentViewModel> GetExerciseDocumentAsync(SiteLanguage language, string exerciseKey, bool forceRuntime = false)
        => forceRuntime
            ? ExerciseDocumentFactory.CreateByKey(language, exerciseKey)
            : await LoadExerciseDocumentAsync(language, exerciseKey) ?? ExerciseDocumentFactory.CreateByKey(language, exerciseKey);

    private async Task<StudyTopicDetailViewModel> BuildExerciseTopicAsync(SiteLanguage language, string slug)
    {
        var isEnglish = language == SiteLanguage.En;
        var isStudio = string.Equals(slug, GdiExercisesSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, GdiExercisesSlugEn, StringComparison.OrdinalIgnoreCase);
        var exerciseKey = ResolveExerciseKey(slug);
        var document = await GetExerciseDocumentAsync(language, exerciseKey);
        var exerciseOptions = GetExerciseOptions(language);

        var title = isStudio
            ? isEnglish ? "GDI exercise studio" : "GDI Uebungsstudio"
            : exerciseKey switch
        {
            "binary-to-decimal" => isEnglish ? "Binary to decimal" : "Binaer zu Dezimal",
            "decimal-to-binary" => isEnglish ? "Decimal to binary" : "Dezimal zu Binaer",
            "twos-complement" => isEnglish ? "Two's complement" : "Zweierkomplement",
            _ => isEnglish ? "Binary addition" : "Binaere Addition"
        };

        var intro = isStudio
            ? isEnglish
                ? "A single productive GDI practice page for imported worksheet content, interactive solving, fresh runtime variants and print output."
                : "Eine einzige produktive GDI-Uebungsseite fuer importierte Aufgabeninhalte, interaktives Loesen, frische Laufzeitvarianten und Druckausgabe."
            : exerciseKey switch
        {
            "binary-to-decimal" => isEnglish
                ? "JSON import is now the primary source for this worksheet. If the file is missing, edu falls back to runtime generation."
                : "JSON-Import ist jetzt die primaere Quelle fuer dieses Aufgabenblatt. Falls die Datei fehlt, faellt edu auf Laufzeit-Erzeugung zurueck.",
            "decimal-to-binary" => isEnglish
                ? "JSON import is now the primary source for this worksheet. If the file is missing, edu falls back to runtime generation."
                : "JSON-Import ist jetzt die primaere Quelle fuer dieses Aufgabenblatt. Falls die Datei fehlt, faellt edu auf Laufzeit-Erzeugung zurueck.",
            _ => isEnglish
                ? "JSON import is now the primary source for this worksheet. If the file is missing, edu falls back to runtime generation."
                : "JSON-Import ist jetzt die primaere Quelle fuer dieses Aufgabenblatt. Falls die Datei fehlt, faellt edu auf Laufzeit-Erzeugung zurueck."
        };

        var sourceValue = isEnglish ? "Imported JSON / runtime fallback" : "Importiertes JSON / Laufzeit-Fallback";

        return new StudyTopicDetailViewModel(
            Kind: StudyTopicKind.Exercise,
            Slug: slug,
            Title: title,
            Intro: intro,
            SectionLabel: isStudio
                ? (isEnglish ? "Grundlagen der Informatik" : "Grundlagen der Informatik")
                : (isEnglish ? "Exercise POC" : "Aufgaben-POC"),
            MetaLabel: isEnglish ? "Rendering" : "Rendering",
            MetaValue: isStudio
                ? (isEnglish ? "Imported JSON + runtime variants on one page" : "Importiertes JSON + Laufzeitvarianten auf einer Seite")
                : (isEnglish ? "Imported JSON -> HTML worksheet" : "Importiertes JSON -> HTML-Arbeitsblatt"),
            BackLabel: isEnglish ? "Back to teaching" : "Zurueck zur Lehre",
            BackRoute: SiteRoutes.Teaching(language),
            Facts: isEnglish
                ? [
                    new StudyFactViewModel("Document", "ExerciseDocument"),
                    new StudyFactViewModel("Primary source", isStudio ? "content/study/*.json + runtime refresh" : "content/study/*.json"),
                    new StudyFactViewModel("Renderer", isStudio ? "Interactive HTML / Print CSS" : "HTML / Print CSS"),
                    new StudyFactViewModel("Topics", isStudio ? exerciseOptions.Count.ToString() : "1"),
                    new StudyFactViewModel("Source", sourceValue)
                ]
                : [
                    new StudyFactViewModel("Dokument", "ExerciseDocument"),
                    new StudyFactViewModel("Primaerquelle", isStudio ? "content/study/*.json + Laufzeit-Aktualisierung" : "content/study/*.json"),
                    new StudyFactViewModel("Renderer", isStudio ? "Interaktives HTML / Print CSS" : "HTML / Print CSS"),
                    new StudyFactViewModel("Themen", isStudio ? exerciseOptions.Count.ToString() : "1"),
                    new StudyFactViewModel("Quelle", sourceValue)
                ],
            GraphSectionTitle: null,
            GraphSectionIntro: null,
            StepSectionTitle: isStudio
                ? (isEnglish ? "Practice workflow" : "Uebungsworkflow")
                : (isEnglish ? "Task structure" : "Aufgabenstruktur"),
            StepSectionIntro: isEnglish
                ? isStudio
                    ? "Switch between GDI task types, solve them directly in the browser, generate fresh variants and print the currently selected sheet."
                    : "The topic page renders an imported exercise document and only falls back to runtime generation when no import exists."
                : isStudio
                    ? "Wechsle zwischen GDI-Aufgabentypen, loese sie direkt im Browser, erzeuge frische Varianten und drucke das aktuell gewaehlte Blatt."
                    : "Die Themenseite rendert ein importiertes Aufgabendokument und faellt nur bei fehlendem Import auf Laufzeit-Erzeugung zurueck.",
            ResultSectionTitle: isEnglish ? "Reuse target" : "Wiederverwendungsziel",
            ResultSectionIntro: isEnglish
                ? "The same semantic payload should later feed edu, Companion previews and LaTeX-based exam sheets."
                : "Dieselbe semantische Nutzlast soll spaeter edu, Companion-Previews und LaTeX-basierte Klausur-/Uebungsblaetter bedienen.",
            InitialStepDescription: string.Empty,
            PreviousStepLabel: string.Empty,
            NextStepLabel: string.Empty,
            CompleteSolutionLabel: string.Empty,
            ResetLabel: string.Empty,
            ProgressLabel: string.Empty,
            NQueens: null,
            GraphVisualization: null,
            GraphStates: null,
            GraphEdges: null,
            Steps: null)
        {
            ExerciseDocument = document,
            ExerciseDocumentKey = exerciseKey,
            ExerciseOptions = isStudio ? exerciseOptions : null
        };
    }

    private async Task<ExerciseDocumentViewModel?> LoadExerciseDocumentAsync(SiteLanguage language, string exerciseKey)
    {
        var cacheKey = $"{language}:{exerciseKey}";
        if (!_disableContentCache && _exerciseDocumentCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var fileName = $"{exerciseKey}.{(language == SiteLanguage.En ? "en" : "de")}.json";
        var response = await _httpClient.GetAsync($"content/study/{fileName}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var document = await response.Content.ReadFromJsonAsync<ExerciseDocumentViewModel>(ExerciseJsonOptions);
        if (!_disableContentCache)
        {
            _exerciseDocumentCache[cacheKey] = document;
        }

        return document;
    }

    private static string ResolveExerciseKey(string slug)
        => slug.ToLowerInvariant() switch
        {
            GdiExercisesSlug or GdiExercisesSlugEn => "binary-addition",
            BinaryAdditionSlug or BinaryAdditionSlugEn => "binary-addition",
            BinaryToDecimalSlug or BinaryToDecimalSlugEn => "binary-to-decimal",
            DecimalToBinarySlug or DecimalToBinarySlugEn => "decimal-to-binary",
            TwosComplementSlug or TwosComplementSlugEn => "twos-complement",
            _ => "binary-addition"
        };

    private static IReadOnlyList<ExerciseTopicOptionViewModel> GetExerciseOptions(SiteLanguage language)
    {
        var isEnglish = language == SiteLanguage.En;

        return
        [
            new ExerciseTopicOptionViewModel(
                "binary-addition",
                isEnglish ? "Binary addition" : "Binaere Addition",
                isEnglish ? "Add binary numbers with carry handling." : "Addiere Binaerzahlen mit Uebertrag."),
            new ExerciseTopicOptionViewModel(
                "binary-to-decimal",
                isEnglish ? "Binary to decimal" : "Binaer zu Dezimal",
                isEnglish ? "Convert bit patterns into decimal values." : "Wandle Bitmuster in Dezimalwerte um."),
            new ExerciseTopicOptionViewModel(
                "decimal-to-binary",
                isEnglish ? "Decimal to binary" : "Dezimal zu Binaer",
                isEnglish ? "Convert decimal values into bit patterns." : "Wandle Dezimalwerte in Bitmuster um."),
            new ExerciseTopicOptionViewModel(
                "twos-complement",
                isEnglish ? "Two's complement" : "Zweierkomplement",
                isEnglish ? "Compute signed 8-bit two's complement representations." : "Berechne vorzeichenbehaftete 8-Bit-Zweierkomplementdarstellungen.")
        ];
    }

    private static bool IsLocalDevelopment(Uri? baseAddress)
    {
        if (baseAddress is null)
        {
            return false;
        }

        return string.Equals(baseAddress.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(baseAddress.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase);
    }

    private static StudyTopicDetailViewModel BuildNQueensTopic(SiteLanguage language, string slug)
    {
        var isEnglish = language == SiteLanguage.En;
        var algorithms = NQueensSimulation.BuildAll(8)
            .Select(simulation => BuildNQueensAlgorithm(language, simulation))
            .ToArray();

        return new StudyTopicDetailViewModel(
            Kind: StudyTopicKind.NQueens,
            Slug: slug,
            Title: isEnglish ? "N-Queens with Local and Evolutionary Search" : "N-Damen mit lokaler und evolutionaerer Suche",
            Intro: isEnglish
                ? "This demonstrator compares three approaches discussed in the lecture on the same 8-queens problem: Hill Climbing, Simulated Annealing and a Genetic Algorithm."
                : "Dieser Demonstrator vergleicht drei in der Vorlesung besprochene Verfahren am selben 8-Damen-Problem: Hill Climbing, Simulated Annealing und einen Genetic Algorithm.",
            SectionLabel: isEnglish ? "AI demonstrator" : "KI-Demonstrator",
            MetaLabel: isEnglish ? "Algorithms" : "Algorithmen",
            MetaValue: "Hill Climbing / Simulated Annealing / Genetic Algorithm",
            BackLabel: isEnglish ? "Back to teaching" : "Zurueck zur Lehre",
            BackRoute: SiteRoutes.Teaching(language),
            Facts: isEnglish
                ? [
                    new StudyFactViewModel("Board", "8x8"),
                    new StudyFactViewModel("Algorithms", "3"),
                    new StudyFactViewModel("Goal", "0 conflicts"),
                    new StudyFactViewModel("Source", "ISD Companion + lecture strategies")
                ]
                : [
                    new StudyFactViewModel("Brett", "8x8"),
                    new StudyFactViewModel("Algorithmen", "3"),
                    new StudyFactViewModel("Ziel", "0 Konflikte"),
                    new StudyFactViewModel("Quelle", "ISD Companion + Vorlesungsansaetze")
                ],
            GraphSectionTitle: null,
            GraphSectionIntro: null,
            StepSectionTitle: isEnglish ? "Stepwise comparison" : "Schrittweiser Vergleich",
            StepSectionIntro: isEnglish
                ? "Switch between algorithms and inspect how each method changes queen positions, conflicts and evaluation metrics over time."
                : "Wechsle zwischen den Algorithmen und verfolge, wie jedes Verfahren Damenpositionen, Konflikte und Bewertungsmetriken ueber die Zeit veraendert.",
            ResultSectionTitle: isEnglish ? "Current state" : "Aktueller Zustand",
            ResultSectionIntro: isEnglish
                ? "The cards summarize the currently selected algorithm state instead of a fixed final tree."
                : "Die Karten fassen den aktuell ausgewaehlten Algorithmuszustand zusammen statt einer festen finalen Baumloesung.",
            InitialStepDescription: isEnglish
                ? "Choose an algorithm and advance one step at a time."
                : "Waehle einen Algorithmus und gehe dann Schritt fuer Schritt vor.",
            PreviousStepLabel: isEnglish ? "Previous" : "Zurueck",
            NextStepLabel: isEnglish ? "Next" : "Naechster Schritt",
            CompleteSolutionLabel: isEnglish ? "Complete run" : "Kompletter Lauf",
            ResetLabel: isEnglish ? "Reset" : "Neu starten",
            ProgressLabel: isEnglish ? "Current progress" : "Aktueller Fortschritt",
            NQueens: new NQueensTopicViewModel(
                AlgorithmSelectorLabel: isEnglish ? "Algorithm" : "Algorithmus",
                BoardSectionTitle: isEnglish ? "Board" : "Brett",
                BoardSectionIntro: isEnglish
                    ? "Conflicting queens are highlighted in red. The currently modified queen is outlined separately."
                    : "Konfliktbehaftete Damen sind rot markiert. Die aktuell bearbeitete Dame wird zusaetzlich hervorgehoben.",
                BoardSizeLabel: isEnglish ? "Board size" : "Brettgroesse",
                ConflictCountLabel: isEnglish ? "Conflicts" : "Konflikte",
                CurrentAssignmentLabel: isEnglish ? "Assignment" : "Belegung",
                CurrentMoveLabel: isEnglish ? "Current move" : "Aktuelle Aktion",
                SolvedLabel: isEnglish ? "Solution found" : "Loesung gefunden",
                StoppedLabel: isEnglish ? "No further improvement" : "Keine weitere Verbesserung",
                Algorithms: algorithms),
            GraphVisualization: null,
            GraphStates: null,
            GraphEdges: null,
            Steps: null);
    }

    private static NQueensAlgorithmViewModel BuildNQueensAlgorithm(SiteLanguage language, NQueensAlgorithmSimulation simulation)
    {
        var isEnglish = language == SiteLanguage.En;
        var name = simulation.Algorithm switch
        {
            NQueensAlgorithmKind.HillClimbing => "Hill Climbing",
            NQueensAlgorithmKind.SimulatedAnnealing => "Simulated Annealing",
            _ => "Genetic Algorithm"
        };

        var summary = simulation.Algorithm switch
        {
            NQueensAlgorithmKind.HillClimbing => isEnglish
                ? "Greedy local search: always pick the best improving queen move."
                : "Gierige lokale Suche: Es wird immer der beste verbessernde Zug gewaehlt.",
            NQueensAlgorithmKind.SimulatedAnnealing => isEnglish
                ? "Occasionally accepts worse moves while the temperature is still high."
                : "Akzeptiert bei hoher Temperatur gelegentlich auch schlechtere Zuege.",
            _ => isEnglish
                ? "Evolves a population of boards through selection, crossover and mutation."
                : "Entwickelt eine Population von Brettern ueber Selektion, Crossover und Mutation weiter."
        };

        return new NQueensAlgorithmViewModel(
            Key: simulation.Algorithm.ToString(),
            Name: name,
            Summary: summary,
            BoardSize: simulation.BoardSize,
            InitialQueenRows: simulation.InitialQueenRows,
            Steps: simulation.Steps.Select(step => BuildNQueensStep(language, simulation.Algorithm, step)).ToArray());
    }

    private static NQueensStepViewModel BuildNQueensStep(
        SiteLanguage language,
        NQueensAlgorithmKind algorithm,
        NQueensStepModel step)
    {
        var isEnglish = language == SiteLanguage.En;
        var assignment = FormatAssignment(step.QueenRows);
        var metric = algorithm switch
        {
            NQueensAlgorithmKind.HillClimbing => isEnglish
                ? $"fitness={FormatWeight(step.Fitness ?? 0)}"
                : $"Fitness={FormatWeight(step.Fitness ?? 0)}",
            NQueensAlgorithmKind.SimulatedAnnealing => isEnglish
                ? $"T={FormatWeight(step.Temperature ?? 0)}, fitness={FormatWeight(step.Fitness ?? 0)}"
                : $"T={FormatWeight(step.Temperature ?? 0)}, Fitness={FormatWeight(step.Fitness ?? 0)}",
            _ => isEnglish
                ? $"generation={step.Generation ?? 0}, fitness={FormatWeight(step.Fitness ?? 0)}"
                : $"Generation={step.Generation ?? 0}, Fitness={FormatWeight(step.Fitness ?? 0)}"
        };

        var summary = step.Kind switch
        {
            NQueensStepKind.Initial => isEnglish ? "Initial board" : "Startbrett",
            NQueensStepKind.Move => step.CurrentColumn.HasValue && step.CurrentRow.HasValue
                ? (isEnglish ? $"Move Q{step.CurrentColumn.Value + 1} to row {step.CurrentRow.Value + 1}" : $"Q{step.CurrentColumn.Value + 1} nach Zeile {step.CurrentRow.Value + 1} verschieben")
                : (isEnglish ? "Apply move" : "Zug anwenden"),
            NQueensStepKind.RejectedMove => step.CurrentColumn.HasValue && step.CurrentRow.HasValue
                ? (isEnglish ? $"Reject Q{step.CurrentColumn.Value + 1} -> row {step.CurrentRow.Value + 1}" : $"Q{step.CurrentColumn.Value + 1} -> Zeile {step.CurrentRow.Value + 1} verwerfen")
                : (isEnglish ? "Reject move" : "Zug verwerfen"),
            NQueensStepKind.Generation => isEnglish ? $"Generation {step.Generation}" : $"Generation {step.Generation}",
            NQueensStepKind.Solved => isEnglish ? "Solution found" : "Loesung gefunden",
            _ => isEnglish ? "Stop condition reached" : "Abbruchbedingung erreicht"
        };

        var description = step.Kind switch
        {
            NQueensStepKind.Initial => isEnglish
                ? $"Start with assignment {assignment}."
                : $"Starte mit der Belegung {assignment}.",
            NQueensStepKind.Move => isEnglish
                ? $"The new state is {assignment} with {step.ConflictCount} conflicting queen pairs."
                : $"Der neue Zustand ist {assignment} mit {step.ConflictCount} konfliktbehafteten Damenpaaren.",
            NQueensStepKind.RejectedMove => isEnglish
                ? $"The candidate move is rejected; the board stays at {assignment}."
                : $"Der Kandidatenzug wird verworfen; das Brett bleibt bei {assignment}.",
            NQueensStepKind.Generation => isEnglish
                ? $"This generation keeps the currently best board {assignment} with {step.ConflictCount} conflicts."
                : $"Diese Generation behaelt das aktuell beste Brett {assignment} mit {step.ConflictCount} Konflikten.",
            NQueensStepKind.Solved => isEnglish
                ? $"All queens are placed conflict-free: {assignment}."
                : $"Alle Damen sind konfliktfrei platziert: {assignment}.",
            _ => isEnglish
                ? $"The run stops at {assignment} with {step.ConflictCount} remaining conflicts."
                : $"Der Lauf endet bei {assignment} mit noch {step.ConflictCount} Konflikten."
        };

        return new NQueensStepViewModel(
            Number: step.StepNumber,
            Summary: summary,
            Metric: metric,
            Description: description,
            QueenRows: step.QueenRows,
            CurrentColumn: step.CurrentColumn,
            CurrentRow: step.CurrentRow,
            ConflictedColumns: step.ConflictedColumns,
            ConflictCount: step.ConflictCount,
            Solved: step.Solved,
            Stopped: step.Kind == NQueensStepKind.Stopped);
    }

    private static StudyTopicDetailViewModel BuildRomaniaSearchTopic(SiteLanguage language, string slug)
    {
        var isEnglish = language == SiteLanguage.En;
        const string startCity = "Arad";
        const string goalCity = "Bukarest";

        var graph = Graphs.Instance.AIMARomania;
        var simulationSteps = RomaniaSearchSimulation.SimulateAStar(graph, startCity, goalCity);
        var finalStep = simulationSteps.LastOrDefault();
        var finalPathStates = finalStep?.PathStates ?? [startCity];
        var solutionEdgeKeys = BuildPathEdgeKeys(graph, finalPathStates);

        var graphEdges = graph.Edges
            .Select(edge => new StudyEdgeViewModel(
                Source: edge.Source,
                Target: edge.Target,
                Weight: FormatWeight(edge.Tag),
                EdgeKey: GraphViewKeys.CreateUndirectedEdgeKey(edge.Source, edge.Target, edge.Tag),
                IsPartOfSolution: solutionEdgeKeys.Contains(GraphViewKeys.CreateUndirectedEdgeKey(edge.Source, edge.Target, edge.Tag))))
            .OrderBy(edge => edge.Source, StringComparer.Ordinal)
            .ThenBy(edge => edge.Target, StringComparer.Ordinal)
            .ThenBy(edge => edge.Weight, StringComparer.Ordinal)
            .ToArray();

        var graphVisualization = GraphViewFactory.BuildSvgGraphView(graph, solutionEdgeKeys);
        var graphStates = BuildRomaniaSearchStates(graph, simulationSteps, startCity);

        var steps = simulationSteps
            .Select(step => new StudyStepViewModel(
                Number: step.StepNumber,
                EdgeKey: string.Empty,
                EdgeLabel: isEnglish ? $"Expand {step.ExpandedNode.State}" : $"{step.ExpandedNode.State} expandieren",
                Weight: $"g={FormatWeight(step.ExpandedNode.PathCost)}, f={FormatWeight(step.ExpandedNode.Priority)}",
                Description: CreateRomaniaStepDescription(isEnglish, step)))
            .ToArray();

        var totalCost = finalStep?.ExpandedNode.PathCost ?? 0.0;
        var finalRoute = string.Join(" -> ", finalPathStates);

        return new StudyTopicDetailViewModel(
            Kind: StudyTopicKind.Graph,
            Slug: slug,
            Title: isEnglish ? "Romania Search with A*" : "Rumänien-Suche mit A*",
            Intro: isEnglish
                ? "This demonstrator reuses the Romania road map from the ISD Companion and visualizes how A* incrementally expands cities on the way from Arad to Bucharest."
                : "Dieser Demonstrator nutzt die Rumänien-Karte aus dem ISD Companion weiter und visualisiert, wie A* Städte auf dem Weg von Arad nach Bukarest schrittweise expandiert.",
            SectionLabel: isEnglish ? "AI demonstrator" : "KI-Demonstrator",
            MetaLabel: isEnglish ? "Solver" : "Solver",
            MetaValue: "A* / Italbytz.Graph / SVG",
            BackLabel: isEnglish ? "Back to teaching" : "Zurueck zur Lehre",
            BackRoute: SiteRoutes.Teaching(language),
            Facts: isEnglish
                ? [
                    new StudyFactViewModel("Cities", graph.Edges.SelectMany(edge => new[] { edge.Source, edge.Target }).Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture)),
                    new StudyFactViewModel("Roads", graph.Edges.Count().ToString(CultureInfo.InvariantCulture)),
                    new StudyFactViewModel("Path cost", FormatWeight(totalCost)),
                    new StudyFactViewModel("Source", "ISD Companion + Italbytz.Graph")
                ]
                : [
                    new StudyFactViewModel("Staedte", graph.Edges.SelectMany(edge => new[] { edge.Source, edge.Target }).Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture)),
                    new StudyFactViewModel("Strassen", graph.Edges.Count().ToString(CultureInfo.InvariantCulture)),
                    new StudyFactViewModel("Pfadkosten", FormatWeight(totalCost)),
                    new StudyFactViewModel("Quelle", "ISD Companion + Italbytz.Graph")
                ],
            GraphSectionTitle: isEnglish ? "Search graph" : "Suchgraph",
            GraphSectionIntro: isEnglish
                ? "The graph shows the current expanded city, the active route from Arad, the frontier and the successors generated in the current A* step."
                : "Der Graph zeigt die aktuell expandierte Stadt, den aktiven Weg ab Arad, die Frontier sowie die im aktuellen A*-Schritt erzeugten Nachfolger.",
            StepSectionTitle: isEnglish ? "A* trace" : "A*-Ablauf",
            StepSectionIntro: isEnglish
                ? "Advance through the search one expansion at a time and inspect how path cost g and evaluation f = g + h evolve."
                : "Gehe die Suche Expansion fuer Expansion durch und verfolge, wie sich Pfadkosten g und Bewertung f = g + h entwickeln.",
            ResultSectionTitle: isEnglish ? "Result" : "Ergebnis",
            ResultSectionIntro: isEnglish
                ? $"A* reaches Bucharest via {finalRoute} with total path cost {FormatWeight(totalCost)}."
                : $"A* erreicht Bukarest ueber {finalRoute} mit Gesamtkosten {FormatWeight(totalCost)}.",
            InitialStepDescription: isEnglish
                ? "Initial state: the frontier contains only Arad."
                : "Startzustand: In der Frontier befindet sich nur Arad.",
            PreviousStepLabel: isEnglish ? "Previous" : "Zurueck",
            NextStepLabel: isEnglish ? "Next" : "Naechster Schritt",
            CompleteSolutionLabel: isEnglish ? "Complete solution" : "Komplette Loesung",
            ResetLabel: isEnglish ? "Reset" : "Neu starten",
            ProgressLabel: isEnglish ? "Current progress" : "Aktueller Fortschritt",
            NQueens: null,
            GraphVisualization: graphVisualization,
            GraphStates: graphStates,
            GraphEdges: graphEdges,
            Steps: steps);
    }

    private static StudyTopicDetailViewModel BuildMinimalSpanningTreeTopic(SiteLanguage language, string slug)
    {
        var isEnglish = language == SiteLanguage.En;
        var isVisualizationPoc = string.Equals(slug, GraphVisualizationSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, GraphVisualizationSlugEn, StringComparison.OrdinalIgnoreCase);
        var graph = Graphs.Instance.TanenbaumWetherall;
        var solver = new MinimumSpanningTreeSolver();
        var parameters = new MinimumSpanningTreeParameters(graph);
        var solution = solver.Solve(parameters);
        var orderedSolutionEdges = solution.Edges.ToArray();

        var selectedKeys = orderedSolutionEdges
            .Select(edge => GraphViewKeys.CreateUndirectedEdgeKey(edge.Source, edge.Target, edge.Tag))
            .ToArray();

        var graphEdges = graph.Edges
            .Select(edge => new StudyEdgeViewModel(
                Source: edge.Source,
                Target: edge.Target,
                Weight: FormatWeight(edge.Tag),
                EdgeKey: GraphViewKeys.CreateUndirectedEdgeKey(edge.Source, edge.Target, edge.Tag),
                IsPartOfSolution: selectedKeys.Contains(GraphViewKeys.CreateUndirectedEdgeKey(edge.Source, edge.Target, edge.Tag))))
            .OrderBy(edge => edge.Source, StringComparer.Ordinal)
            .ThenBy(edge => edge.Target, StringComparer.Ordinal)
            .ThenBy(edge => edge.Weight, StringComparer.Ordinal)
            .ToArray();

        var graphVisualization = GraphViewFactory.BuildSvgGraphView(graph, selectedKeys);
        var graphStates = GraphViewFactory.BuildMinimumSpanningTreeStates(graph, orderedSolutionEdges);

        var steps = orderedSolutionEdges
            .Select((edge, index) => new StudyStepViewModel(
                Number: index + 1,
                EdgeKey: GraphViewKeys.CreateUndirectedEdgeKey(edge.Source, edge.Target, edge.Tag),
                EdgeLabel: $"{edge.Source} - {edge.Target}",
                Weight: FormatWeight(edge.Tag),
                Description: isEnglish
                    ? $"Edge {edge.Source} - {edge.Target} with weight {FormatWeight(edge.Tag)} is added to connect a new vertex at minimal cost."
                    : $"Die Kante {edge.Source} - {edge.Target} mit Gewicht {FormatWeight(edge.Tag)} wird aufgenommen, weil sie einen neuen Knoten mit minimalen Kosten anbindet."))
            .ToArray();

        return new StudyTopicDetailViewModel(
            Kind: StudyTopicKind.Graph,
            Slug: slug,
            Title: isVisualizationPoc
                ? (isEnglish ? "Graph Visualization POC" : "Graph-Visualisierung POC")
                : (isEnglish ? "Minimum Spanning Tree" : "Minimaler Spannbaum"),
            Intro: isEnglish
                ? (isVisualizationPoc
                    ? "Proof of concept for graph rendering in edu: the MST example is laid out with MSAGL and rendered as interactive SVG in the browser."
                    : "This page reuses the existing graph package from the companion code base and turns the MST example into a web-native SVG graph view for edu.")
                : (isVisualizationPoc
                    ? "Proof of concept fuer Graph-Rendering in edu: Das MST-Beispiel wird mit MSAGL gelayoutet und im Browser als interaktive SVG gerendert."
                    : "Diese Seite nutzt das bestehende Graph-Paket aus dem Companion-Codebestand weiter und macht das MST-Beispiel zu einer web-nativen SVG-Graphansicht fuer edu."),
            SectionLabel: isVisualizationPoc
                ? (isEnglish ? "Proof of concept" : "Proof of concept")
                : (isEnglish ? "Technical topic" : "Technisches Thema"),
            MetaLabel: isEnglish ? "Solver" : "Solver",
            MetaValue: "Prim / Italbytz.Graph / SVG",
            BackLabel: isEnglish ? "Back to teaching" : "Zurueck zur Lehre",
            BackRoute: isEnglish ? SiteRoutes.Teaching(language) : SiteRoutes.Teaching(language),
            Facts: isEnglish
                ? [
                    new StudyFactViewModel("Vertices", graph.Edges.SelectMany(edge => new[] { edge.Source, edge.Target }).Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture)),
                    new StudyFactViewModel("Edges", graph.Edges.Count().ToString(CultureInfo.InvariantCulture)),
                    new StudyFactViewModel("Total weight", FormatWeight(solution.Edges.Sum(edge => edge.Tag))),
                    new StudyFactViewModel("Source", "ISD Companion + Italbytz.Graph")
                ]
                : [
                    new StudyFactViewModel("Knoten", graph.Edges.SelectMany(edge => new[] { edge.Source, edge.Target }).Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture)),
                    new StudyFactViewModel("Kanten", graph.Edges.Count().ToString(CultureInfo.InvariantCulture)),
                    new StudyFactViewModel("Gesamtgewicht", FormatWeight(solution.Edges.Sum(edge => edge.Tag))),
                    new StudyFactViewModel("Quelle", "ISD Companion + Italbytz.Graph")
                ],
            GraphSectionTitle: isEnglish ? "Graph" : "Graph",
            GraphSectionIntro: isEnglish
                ? "The layout is calculated in C# with MSAGL and rendered as SVG in the browser so the current tree can be inspected directly on the graph."
                : "Das Layout wird in C# mit MSAGL berechnet und im Browser als SVG gerendert, damit der aktuelle Baum direkt im Graphen nachvollzogen werden kann.",
            StepSectionTitle: isEnglish ? "Stepwise solution" : "Schrittweise Loesung",
            StepSectionIntro: isEnglish
                ? "Advance through the tree one edge at a time or jump to the complete solution."
                : "Gehe Kante fuer Kante durch den Spannbaum oder springe direkt zur kompletten Loesung.",
            ResultSectionTitle: isEnglish ? "Result" : "Ergebnis",
            ResultSectionIntro: isEnglish
                ? "The final tree contains seven edges and reaches all eight vertices with minimal total weight."
                : "Der finale Baum enthaelt sieben Kanten und erreicht alle acht Knoten mit minimalem Gesamtgewicht.",
            InitialStepDescription: isEnglish
                ? "Initial state: no edge is selected yet."
                : "Startzustand: Noch ist keine Kante ausgewaehlt.",
            PreviousStepLabel: isEnglish ? "Previous" : "Zurueck",
            NextStepLabel: isEnglish ? "Next" : "Naechster Schritt",
            CompleteSolutionLabel: isEnglish ? "Complete solution" : "Komplette Loesung",
            ResetLabel: isEnglish ? "Reset" : "Neu starten",
            ProgressLabel: isEnglish ? "Current progress" : "Aktueller Fortschritt",
            NQueens: null,
            GraphVisualization: graphVisualization,
            GraphStates: graphStates,
            GraphEdges: graphEdges,
            Steps: steps);
    }

    private static string FormatWeight(double weight) => weight.ToString("0.##", CultureInfo.InvariantCulture);

    private static IReadOnlyList<GraphStateViewModel> BuildRomaniaSearchStates(
        IUndirectedGraph<string, ITaggedEdge<string, double>> graph,
        IReadOnlyList<RomaniaSearchStepModel> steps,
        string startCity)
    {
        var states = new List<GraphStateViewModel>
        {
            new(
                startCity,
                [startCity],
                [startCity],
                [],
                [],
                [],
                [],
                [])
        };

        foreach (var step in steps)
        {
            var activeEdgeKeys = BuildPathEdgeKeys(graph, step.PathStates);
            var successorEdgeKeys = BuildSuccessorEdgeKeys(graph, step.ExpandedNode.State, step.Successors);

            states.Add(new GraphStateViewModel(
                step.ExpandedNode.State,
                step.PathStates.ToArray(),
                step.Frontier.Select(node => node.State).Distinct(StringComparer.Ordinal).OrderBy(node => node, StringComparer.Ordinal).ToArray(),
                step.ExploredStates.Where(state => !string.Equals(state, step.ExpandedNode.State, StringComparison.Ordinal)).ToArray(),
                activeEdgeKeys,
                successorEdgeKeys,
                BuildDirectedPathEdgeIds(step.PathStates),
                step.Successors.Select(successor => GraphViewKeys.CreateDirectedEdgeId(step.ExpandedNode.State, successor.State)).ToArray()));
        }

        return states;
    }

    private static string CreateRomaniaStepDescription(bool isEnglish, RomaniaSearchStepModel step)
    {
        var successorStates = step.Successors.Count == 0
            ? (isEnglish ? "no new successors" : "keine neuen Nachfolger")
            : string.Join(", ", step.Successors.Select(successor => $"{successor.State} (g={FormatWeight(successor.PathCost)}, f={FormatWeight(successor.Priority)})"));

        return step.GoalReached
            ? isEnglish
                ? $"{step.ExpandedNode.State} is selected as goal node. The resulting route is {string.Join(" -> ", step.PathStates)}."
                : $"{step.ExpandedNode.State} wird als Zielknoten ausgewaehlt. Die resultierende Route ist {string.Join(" -> ", step.PathStates)}."
            : isEnglish
                ? $"{step.ExpandedNode.State} is expanded next because it has the smallest f-value. Generated successors: {successorStates}."
                : $"{step.ExpandedNode.State} wird als naechstes expandiert, weil es den kleinsten f-Wert besitzt. Erzeugte Nachfolger: {successorStates}.";
    }

    private static string[] BuildPathEdgeKeys(
        IUndirectedGraph<string, ITaggedEdge<string, double>> graph,
        IReadOnlyList<string> pathStates)
    {
        return pathStates
            .Zip(pathStates.Skip(1), (from, to) => FindEdge(graph, from, to))
            .Where(edge => edge is not null)
            .Select(edge => GraphViewKeys.CreateUndirectedEdgeKey(edge!.Source, edge.Target, edge.Tag))
            .ToArray();
    }

    private static string[] BuildSuccessorEdgeKeys(
        IUndirectedGraph<string, ITaggedEdge<string, double>> graph,
        string currentState,
        IReadOnlyList<RomaniaSearchTraceNodeModel> successors)
    {
        return successors
            .Select(successor => FindEdge(graph, currentState, successor.State))
            .Where(edge => edge is not null)
            .Select(edge => GraphViewKeys.CreateUndirectedEdgeKey(edge!.Source, edge.Target, edge.Tag))
            .ToArray();
    }

    private static string[] BuildDirectedPathEdgeIds(IReadOnlyList<string> pathStates)
        => pathStates.Zip(pathStates.Skip(1), (from, to) => GraphViewKeys.CreateDirectedEdgeId(from, to)).ToArray();

    private static string FormatAssignment(IReadOnlyList<int> queenRows)
        => string.Join(", ", queenRows.Select((row, column) => $"Q{column + 1}->{row + 1}"));

    private static ITaggedEdge<string, double>? FindEdge(
        IUndirectedGraph<string, ITaggedEdge<string, double>> graph,
        string from,
        string to)
    {
        return graph.Edges.FirstOrDefault(edge =>
            (string.Equals(edge.Source, from, StringComparison.Ordinal) && string.Equals(edge.Target, to, StringComparison.Ordinal))
            || (string.Equals(edge.Source, to, StringComparison.Ordinal) && string.Equals(edge.Target, from, StringComparison.Ordinal)));
    }
}

public sealed record StudyTopicCardViewModel(
    string Title,
    string Summary,
    string Kicker,
    string Route,
    IReadOnlyList<string> Tags);

public enum StudyTopicKind
{
    Graph,
    NQueens,
    Exercise
}

public sealed record StudyTopicDetailViewModel(
    StudyTopicKind Kind,
    string Slug,
    string Title,
    string Intro,
    string SectionLabel,
    string MetaLabel,
    string MetaValue,
    string BackLabel,
    string BackRoute,
    IReadOnlyList<StudyFactViewModel> Facts,
    string? GraphSectionTitle,
    string? GraphSectionIntro,
    string StepSectionTitle,
    string StepSectionIntro,
    string ResultSectionTitle,
    string ResultSectionIntro,
    string InitialStepDescription,
    string PreviousStepLabel,
    string NextStepLabel,
    string CompleteSolutionLabel,
    string ResetLabel,
    string ProgressLabel,
    NQueensTopicViewModel? NQueens,
    GraphViewModel? GraphVisualization,
    IReadOnlyList<GraphStateViewModel>? GraphStates,
    IReadOnlyList<StudyEdgeViewModel>? GraphEdges,
    IReadOnlyList<StudyStepViewModel>? Steps)
{
    public ExerciseDocumentViewModel? ExerciseDocument { get; init; }
    public string? ExerciseDocumentKey { get; init; }
    public IReadOnlyList<ExerciseTopicOptionViewModel>? ExerciseOptions { get; init; }
}

public sealed record ExerciseTopicOptionViewModel(
    string Key,
    string Title,
    string Summary);

public sealed record StudyFactViewModel(string Label, string Value);

public sealed record StudyEdgeViewModel(
    string Source,
    string Target,
    string Weight,
    string EdgeKey,
    bool IsPartOfSolution);

public sealed record StudyStepViewModel(
    int Number,
    string EdgeKey,
    string EdgeLabel,
    string Weight,
    string Description);

public sealed record NQueensTopicViewModel(
    string AlgorithmSelectorLabel,
    string BoardSectionTitle,
    string BoardSectionIntro,
    string BoardSizeLabel,
    string ConflictCountLabel,
    string CurrentAssignmentLabel,
    string CurrentMoveLabel,
    string SolvedLabel,
    string StoppedLabel,
    IReadOnlyList<NQueensAlgorithmViewModel> Algorithms);

public sealed record NQueensAlgorithmViewModel(
    string Key,
    string Name,
    string Summary,
    int BoardSize,
    int[] InitialQueenRows,
    IReadOnlyList<NQueensStepViewModel> Steps);

public sealed record NQueensStepViewModel(
    int Number,
    string Summary,
    string Metric,
    string Description,
    int[] QueenRows,
    int? CurrentColumn,
    int? CurrentRow,
    IReadOnlyList<int> ConflictedColumns,
    int ConflictCount,
    bool Solved,
    bool Stopped);
using System.Globalization;
using Italbytz.Graph;
using Italbytz.Graph.Abstractions;
using Shared.Domain;

namespace Shared.Application;

public sealed class StudyTopicService
{
    private const string MinimalSpanningTreeSlug = "minimaler-spannbaum";

    public StudyTopicCardViewModel GetFeaturedTopic(SiteLanguage language)
    {
        var isEnglish = language == SiteLanguage.En;

        return new StudyTopicCardViewModel(
            Title: isEnglish ? "Minimum Spanning Tree" : "Minimaler Spannbaum",
            Summary: isEnglish
                ? "First technical slice for edu: a web view over the existing MST solver from Italbytz.Graph."
                : "Erster technischer Durchstich fuer edu: eine Web-Ansicht ueber dem bestehenden MST-Solver aus Italbytz.Graph.",
            Kicker: "ISD Companion / Italbytz.Graph",
            Route: isEnglish ? $"/en{StudyRoutes.Topic(MinimalSpanningTreeSlug)}" : StudyRoutes.Topic(MinimalSpanningTreeSlug),
            Tags: isEnglish
                ? ["Graphs", "Prim", "Companion import"]
                : ["Graphen", "Prim", "Companion-Import"]);
    }

    public StudyTopicDetailViewModel? GetTopicDetail(SiteLanguage language, string slug)
    {
        if (!string.Equals(slug, MinimalSpanningTreeSlug, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return BuildMinimalSpanningTreeTopic(language);
    }

    private static StudyTopicDetailViewModel BuildMinimalSpanningTreeTopic(SiteLanguage language)
    {
        var isEnglish = language == SiteLanguage.En;
        var graph = Graphs.Instance.TanenbaumWetherall;
        var solver = new MinimumSpanningTreeSolver();
        var parameters = new MinimumSpanningTreeParameters(graph);
        var solution = solver.Solve(parameters);

        var selectedKeys = solution.Edges
            .Select(edge => CreateEdgeKey(edge.Source, edge.Target, edge.Tag))
            .ToArray();

        var graphEdges = graph.Edges
            .Select(edge => new StudyEdgeViewModel(
                Source: edge.Source,
                Target: edge.Target,
                Weight: FormatWeight(edge.Tag),
                EdgeKey: CreateEdgeKey(edge.Source, edge.Target, edge.Tag),
                IsPartOfSolution: selectedKeys.Contains(CreateEdgeKey(edge.Source, edge.Target, edge.Tag))))
            .OrderBy(edge => edge.Source, StringComparer.Ordinal)
            .ThenBy(edge => edge.Target, StringComparer.Ordinal)
            .ThenBy(edge => edge.Weight, StringComparer.Ordinal)
            .ToArray();

        var steps = solution.Edges
            .Select((edge, index) => new StudyStepViewModel(
                Number: index + 1,
                EdgeKey: CreateEdgeKey(edge.Source, edge.Target, edge.Tag),
                EdgeLabel: $"{edge.Source} - {edge.Target}",
                Weight: FormatWeight(edge.Tag),
                Description: isEnglish
                    ? $"Edge {edge.Source} - {edge.Target} with weight {FormatWeight(edge.Tag)} is added to connect a new vertex at minimal cost."
                    : $"Die Kante {edge.Source} - {edge.Target} mit Gewicht {FormatWeight(edge.Tag)} wird aufgenommen, weil sie einen neuen Knoten mit minimalen Kosten anbindet."))
            .ToArray();

        return new StudyTopicDetailViewModel(
            Slug: MinimalSpanningTreeSlug,
            Title: isEnglish ? "Minimum Spanning Tree" : "Minimaler Spannbaum",
            Intro: isEnglish
                ? "This page reuses the existing graph package from the companion code base and turns the MST example into a first web-native teaching slice for edu."
                : "Diese Seite nutzt das bestehende Graph-Paket aus dem Companion-Codebestand weiter und macht das MST-Beispiel zu einem ersten web-nativen Lehrdurchstich fuer edu.",
            SectionLabel: isEnglish ? "Technical topic" : "Technisches Thema",
            MetaLabel: isEnglish ? "Solver" : "Solver",
            MetaValue: "Prim / Italbytz.Graph",
            BackLabel: isEnglish ? "Back to teaching" : "Zurueck zur Lehre",
            BackRoute: isEnglish ? SiteRoutes.Teaching(language) : SiteRoutes.Teaching(language),
            GraphSectionTitle: isEnglish ? "Graph" : "Graph",
            GraphSectionIntro: isEnglish
                ? "The sample graph comes directly from the shared package and is intentionally small enough to follow the selection step by step."
                : "Der Beispielgraph kommt direkt aus dem gemeinsamen Paket und ist bewusst klein genug, um die Auswahl Schritt fuer Schritt nachzuvollziehen.",
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
            TotalWeightLabel: isEnglish ? "Total weight" : "Gesamtgewicht",
            SolutionEdgeLabel: isEnglish ? "Selected edges" : "Ausgewaehlte Kanten",
            VerticesLabel: isEnglish ? "Vertices" : "Knoten",
            EdgesLabel: isEnglish ? "Edges" : "Kanten",
            SourceSystemLabel: isEnglish ? "Source" : "Quelle",
            SourceSystemValue: "ISD Companion + Italbytz.Graph",
            TotalWeight: FormatWeight(solution.Edges.Sum(edge => edge.Tag)),
            VertexCount: graph.Edges
                .SelectMany(edge => new[] { edge.Source, edge.Target })
                .Distinct(StringComparer.Ordinal)
                .Count(),
            EdgeCount: graph.Edges.Count(),
            GraphEdges: graphEdges,
            Steps: steps);
    }

    private static string CreateEdgeKey(string source, string target, double weight)
    {
        var ordered = new[] { source, target }.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        return string.Create(CultureInfo.InvariantCulture, $"{ordered[0]}|{ordered[1]}|{weight:0.##}");
    }

    private static string FormatWeight(double weight) => weight.ToString("0.##", CultureInfo.InvariantCulture);
}

public sealed record StudyTopicCardViewModel(
    string Title,
    string Summary,
    string Kicker,
    string Route,
    IReadOnlyList<string> Tags);

public sealed record StudyTopicDetailViewModel(
    string Slug,
    string Title,
    string Intro,
    string SectionLabel,
    string MetaLabel,
    string MetaValue,
    string BackLabel,
    string BackRoute,
    string GraphSectionTitle,
    string GraphSectionIntro,
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
    string TotalWeightLabel,
    string SolutionEdgeLabel,
    string VerticesLabel,
    string EdgesLabel,
    string SourceSystemLabel,
    string SourceSystemValue,
    string TotalWeight,
    int VertexCount,
    int EdgeCount,
    IReadOnlyList<StudyEdgeViewModel> GraphEdges,
    IReadOnlyList<StudyStepViewModel> Steps);

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
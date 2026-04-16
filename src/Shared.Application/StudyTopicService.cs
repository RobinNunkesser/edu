using System.Globalization;
using Italbytz.Graph;
using Italbytz.Graph.Abstractions;
using Italbytz.Graph.Visualization;
using Shared.Domain;

namespace Shared.Application;

public sealed class StudyTopicService
{
    private const string MinimalSpanningTreeSlug = "minimaler-spannbaum";
    private const string GraphVisualizationSlug = "graph-visualisierung";
    private const string GraphVisualizationSlugEn = "graph-visualization";

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

    public StudyTopicDetailViewModel? GetTopicDetail(SiteLanguage language, string slug)
    {
        if (!IsSupportedSlug(slug))
        {
            return null;
        }

        return BuildMinimalSpanningTreeTopic(language, slug);
    }

    private static bool IsSupportedSlug(string slug)
        => string.Equals(slug, MinimalSpanningTreeSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, GraphVisualizationSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, GraphVisualizationSlugEn, StringComparison.OrdinalIgnoreCase);

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
            GraphVisualization: graphVisualization,
            GraphStates: graphStates,
            GraphEdges: graphEdges,
            Steps: steps);
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
    GraphViewModel GraphVisualization,
    IReadOnlyList<GraphStateViewModel> GraphStates,
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
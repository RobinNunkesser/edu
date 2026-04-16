using System.Globalization;
using Italbytz.Graph;
using Italbytz.Graph.Abstractions;
using Italbytz.Graph.Visualization;
using Shared.Domain;

namespace Shared.Application;

public sealed class StudyTopicService
{
    private const string MinimalSpanningTreeSlug = "minimaler-spannbaum";
    private const string RomaniaSearchSlug = "rumaenien-suche";
    private const string RomaniaSearchSlugEn = "romania-search";
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

        return string.Equals(slug, RomaniaSearchSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, RomaniaSearchSlugEn, StringComparison.OrdinalIgnoreCase)
            ? BuildRomaniaSearchTopic(language, slug)
            : BuildMinimalSpanningTreeTopic(language, slug);
    }

    private static bool IsSupportedSlug(string slug)
        => string.Equals(slug, MinimalSpanningTreeSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, RomaniaSearchSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, RomaniaSearchSlugEn, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, GraphVisualizationSlug, StringComparison.OrdinalIgnoreCase)
            || string.Equals(slug, GraphVisualizationSlugEn, StringComparison.OrdinalIgnoreCase);

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
            TotalWeightLabel: isEnglish ? "Path cost" : "Pfadkosten",
            SolutionEdgeLabel: isEnglish ? "Route segment" : "Routenabschnitt",
            VerticesLabel: isEnglish ? "Cities" : "Staedte",
            EdgesLabel: isEnglish ? "Roads" : "Strassen",
            SourceSystemLabel: isEnglish ? "Source" : "Quelle",
            SourceSystemValue: "ISD Companion + Italbytz.Graph",
            TotalWeight: FormatWeight(totalCost),
            VertexCount: graph.Edges.SelectMany(edge => new[] { edge.Source, edge.Target }).Distinct(StringComparer.Ordinal).Count(),
            EdgeCount: graph.Edges.Count(),
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
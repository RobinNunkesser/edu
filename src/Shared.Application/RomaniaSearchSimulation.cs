using Italbytz.Graph;
using Italbytz.Graph.Abstractions;

namespace Shared.Application;

internal static class RomaniaSearchSimulation
{
    public static IReadOnlyList<RomaniaSearchStepModel> SimulateAStar(
        IUndirectedGraph<string, ITaggedEdge<string, double>> graph,
        string initialState,
        string goalState)
    {
        var frontier = new List<FrontierItem>
        {
            new(new SearchNode(initialState, null, 0.0, 0), Graphs.Instance.AIMARomaniaHeuristic(initialState), 0)
        };

        var explored = new HashSet<string>(StringComparer.Ordinal);
        var exploredOrder = new List<string>();
        var steps = new List<RomaniaSearchStepModel>();
        long nextSequence = 1;

        while (frontier.Count > 0)
        {
            frontier.Sort(FrontierItemComparer.Instance);

            var current = frontier[0];
            frontier.RemoveAt(0);

            if (!explored.Add(current.Node.State))
            {
                continue;
            }

            exploredOrder.Add(current.Node.State);

            if (string.Equals(current.Node.State, goalState, StringComparison.Ordinal))
            {
                steps.Add(CreateStep(steps.Count + 1, current, frontier, exploredOrder, [], goalReached: true));
                break;
            }

            var successors = new List<RomaniaSearchTraceNodeModel>();
            foreach (var edge in GetOutgoingEdges(graph, current.Node.State))
            {
                var successorState = GetOtherVertex(edge, current.Node.State);
                if (explored.Contains(successorState))
                {
                    continue;
                }

                var successorNode = new SearchNode(
                    successorState,
                    current.Node,
                    current.Node.PathCost + edge.Tag,
                    current.Node.Depth + 1);

                var successorPriority = successorNode.PathCost + Graphs.Instance.AIMARomaniaHeuristic(successorState);
                frontier.Add(new FrontierItem(successorNode, successorPriority, nextSequence++));
                successors.Add(new RomaniaSearchTraceNodeModel(successorState, successorNode.PathCost, successorPriority, successorNode.Depth));
            }

            steps.Add(CreateStep(steps.Count + 1, current, frontier, exploredOrder, successors, goalReached: false));
        }

        return steps;
    }

    private static RomaniaSearchStepModel CreateStep(
        int stepNumber,
        FrontierItem expandedNode,
        IEnumerable<FrontierItem> frontier,
        IReadOnlyList<string> exploredStates,
        IReadOnlyList<RomaniaSearchTraceNodeModel> successors,
        bool goalReached)
    {
        return new RomaniaSearchStepModel(
            stepNumber,
            new RomaniaSearchTraceNodeModel(
                expandedNode.Node.State,
                expandedNode.Node.PathCost,
                expandedNode.Priority,
                expandedNode.Node.Depth),
            ToPathStates(expandedNode.Node),
            frontier.OrderBy(item => item.Priority).ThenBy(item => item.Sequence)
                .Select(item => new RomaniaSearchTraceNodeModel(item.Node.State, item.Node.PathCost, item.Priority, item.Node.Depth))
                .ToArray(),
            exploredStates.ToArray(),
            successors.ToArray(),
            goalReached);
    }

    private static IReadOnlyList<string> ToPathStates(SearchNode node)
    {
        var stack = new Stack<string>();
        var current = node;

        while (current is not null)
        {
            stack.Push(current.State);
            current = current.Parent;
        }

        return stack.ToArray();
    }

    private static IEnumerable<ITaggedEdge<string, double>> GetOutgoingEdges(
        IUndirectedGraph<string, ITaggedEdge<string, double>> graph,
        string state)
    {
        return graph.Edges
            .Where(edge => string.Equals(edge.Source, state, StringComparison.Ordinal)
                || string.Equals(edge.Target, state, StringComparison.Ordinal))
            .OrderBy(edge => GetOtherVertex(edge, state), StringComparer.Ordinal);
    }

    private static string GetOtherVertex(ITaggedEdge<string, double> edge, string state)
        => string.Equals(edge.Source, state, StringComparison.Ordinal) ? edge.Target : edge.Source;

    private sealed record SearchNode(string State, SearchNode? Parent, double PathCost, int Depth);

    private sealed record FrontierItem(SearchNode Node, double Priority, long Sequence);

    private sealed class FrontierItemComparer : IComparer<FrontierItem>
    {
        public static FrontierItemComparer Instance { get; } = new();

        public int Compare(FrontierItem? left, FrontierItem? right)
        {
            if (left is null && right is null)
            {
                return 0;
            }

            if (left is null)
            {
                return -1;
            }

            if (right is null)
            {
                return 1;
            }

            var priorityComparison = left.Priority.CompareTo(right.Priority);
            return priorityComparison != 0 ? priorityComparison : left.Sequence.CompareTo(right.Sequence);
        }
    }
}

internal sealed record RomaniaSearchTraceNodeModel(string State, double PathCost, double Priority, int Depth);

internal sealed record RomaniaSearchStepModel(
    int StepNumber,
    RomaniaSearchTraceNodeModel ExpandedNode,
    IReadOnlyList<string> PathStates,
    IReadOnlyList<RomaniaSearchTraceNodeModel> Frontier,
    IReadOnlyList<string> ExploredStates,
    IReadOnlyList<RomaniaSearchTraceNodeModel> Successors,
    bool GoalReached);
namespace Shared.Application;

internal enum VacuumAgentKind
{
    ReflexAgent,
    ModelBasedReflexAgent
}

internal enum VacuumLocationState { Clean, Dirty }

internal enum VacuumAction { Left, Right, Suck, NoOp }

internal sealed record VacuumWorldStepModel(
    int StepNumber,
    string AgentLocation,
    VacuumLocationState LocationAState,
    VacuumLocationState LocationBState,
    VacuumAction Action,
    int PerformanceMeasure,
    string Description);

internal sealed record VacuumWorldAgentSimulation(
    VacuumAgentKind Agent,
    string InitialLocation,
    VacuumLocationState InitialA,
    VacuumLocationState InitialB,
    IReadOnlyList<VacuumWorldStepModel> Steps);

/// <summary>
/// Step-by-step trace of the AIMA Vacuum World for two agent types.
/// Reference: AIMA 3rd Edition, Figure 2.8 (ReflexVacuumAgent) and Section 2.4 (ModelBasedReflexAgent).
/// Corresponds to aima-java: ReflexVacuumAgent.java and ModelBasedReflexVacuumAgent.java.
/// </summary>
internal static class VacuumWorldSimulation
{
    private const string LocationA = "A";
    private const string LocationB = "B";
    private const int MaxSteps = 10;

    public static IReadOnlyList<VacuumWorldAgentSimulation> BuildAll()
    {
        return
        [
            Simulate(VacuumAgentKind.ReflexAgent, LocationA, VacuumLocationState.Dirty, VacuumLocationState.Dirty),
            Simulate(VacuumAgentKind.ModelBasedReflexAgent, LocationA, VacuumLocationState.Dirty, VacuumLocationState.Dirty)
        ];
    }

    private static VacuumWorldAgentSimulation Simulate(
        VacuumAgentKind kind,
        string initialLocation,
        VacuumLocationState initA,
        VacuumLocationState initB)
    {
        var locA = initA;
        var locB = initB;
        var agentLoc = initialLocation;
        var performance = 0;
        var steps = new List<VacuumWorldStepModel>();

        // Model-based agent internal state
        VacuumLocationState? modelA = null;
        VacuumLocationState? modelB = null;

        for (var step = 1; step <= MaxSteps; step++)
        {
            var currState = agentLoc == LocationA ? locA : locB;

            // Update model if applicable
            if (kind == VacuumAgentKind.ModelBasedReflexAgent)
            {
                if (agentLoc == LocationA) modelA = locA;
                else modelB = locB;
            }

            VacuumAction action;
            if (kind == VacuumAgentKind.ModelBasedReflexAgent
                && modelA == VacuumLocationState.Clean
                && modelB == VacuumLocationState.Clean)
            {
                // Rule: if both known clean -> null (done)
                action = VacuumAction.NoOp;
                steps.Add(BuildStep(step, agentLoc, locA, locB, action, performance,
                    "Model zeigt beide Raeume sauber – kein Zug noetig."));
                break;
            }
            else if (currState == VacuumLocationState.Dirty)
            {
                action = VacuumAction.Suck;
            }
            else if (agentLoc == LocationA)
            {
                action = VacuumAction.Right;
            }
            else
            {
                action = VacuumAction.Left;
            }

            // Execute
            string description;
            if (action == VacuumAction.Suck)
            {
                if (currState == VacuumLocationState.Dirty)
                {
                    if (agentLoc == LocationA) locA = VacuumLocationState.Clean;
                    else locB = VacuumLocationState.Clean;
                    performance += 10;
                    description = $"Raum {agentLoc} ist schmutzig – Saugen. Performance +10 = {performance}.";
                }
                else
                {
                    description = $"Raum {agentLoc} ist bereits sauber – kein Effekt.";
                }
            }
            else if (action == VacuumAction.Right)
            {
                agentLoc = LocationB;
                performance--;
                description = $"Nach rechts (B) bewegen. Performance -1 = {performance}.";
            }
            else
            {
                agentLoc = LocationA;
                performance--;
                description = $"Nach links (A) bewegen. Performance -1 = {performance}.";
            }

            steps.Add(BuildStep(step, agentLoc, locA, locB, action, performance, description));
        }

        return new VacuumWorldAgentSimulation(kind, initialLocation, initA, initB, steps);
    }

    private static VacuumWorldStepModel BuildStep(
        int step, string agentLoc,
        VacuumLocationState locA, VacuumLocationState locB,
        VacuumAction action, int performance, string description)
        => new(step, agentLoc, locA, locB, action, performance, description);
}

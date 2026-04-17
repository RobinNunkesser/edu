namespace Content.Schema;

public sealed record ShortestPathDemoTraceViewModel
{
    public string TraceType { get; init; } = string.Empty;
    public IReadOnlyList<ShortestPathTraceStepViewModel> Steps { get; init; } = [];
}

public sealed record ShortestPathTraceStepViewModel
{
    public int StepIndex { get; init; }
    public string CurrentNode { get; init; } = string.Empty;
    public IReadOnlyList<string> Frontier { get; init; } = [];
    public IReadOnlyList<ShortestPathDistanceSnapshotViewModel> Distances { get; init; } = [];
    public IReadOnlyList<ShortestPathRelaxationViewModel> Relaxations { get; init; } = [];
}

public sealed record ShortestPathDistanceSnapshotViewModel
{
    public string Node { get; init; } = string.Empty;
    public int? Distance { get; init; }
    public string? PreviousNode { get; init; }
}

public sealed record ShortestPathRelaxationViewModel
{
    public string FromNode { get; init; } = string.Empty;
    public string ToNode { get; init; } = string.Empty;
    public int CandidateDistance { get; init; }
    public bool Accepted { get; init; }
}

public sealed record ShortestPathFinalStateViewModel
{
    public string StartNode { get; init; } = string.Empty;
    public IReadOnlyList<ShortestPathFinalDistanceViewModel> Distances { get; init; } = [];
    public IReadOnlyList<ShortestPathRouteViewModel> Routes { get; init; } = [];
}

public sealed record ShortestPathFinalDistanceViewModel
{
    public string Node { get; init; } = string.Empty;
    public int Distance { get; init; }
    public string? PreviousNode { get; init; }
}

public sealed record ShortestPathRouteViewModel
{
    public string TargetNode { get; init; } = string.Empty;
    public IReadOnlyList<string> Path { get; init; } = [];
    public int TotalCost { get; init; }
}

public sealed record PageReplacementDemoTraceDocumentViewModel
{
    public IReadOnlyList<PageReplacementRunViewModel> Runs { get; init; } = [];
}

public sealed record PageReplacementRunViewModel
{
    public PageReplacementFinalStateViewModel? FinalState { get; init; }
    public PageReplacementTraceViewModel? Trace { get; init; }
}

public sealed record PageReplacementTraceViewModel
{
    public string TraceType { get; init; } = string.Empty;
    public IReadOnlyList<PageReplacementTraceStepViewModel> Steps { get; init; } = [];
}

public sealed record PageReplacementTraceStepViewModel
{
    public int StepIndex { get; init; }
    public int RequestedPage { get; init; }
    public bool IsPageFault { get; init; }
    public IReadOnlyList<PageReplacementFrameStateViewModel> Frames { get; init; } = [];
    public string Explanation { get; init; } = string.Empty;
}

public sealed record PageReplacementFinalStateViewModel
{
    public string Strategy { get; init; } = string.Empty;
    public IReadOnlyList<int> ReferenceRequests { get; init; } = [];
    public IReadOnlyList<PageReplacementFrameStateViewModel> FinalFrames { get; init; } = [];
    public int PageFaultCount { get; init; }
}

public sealed record PageReplacementFrameStateViewModel
{
    public int Slot { get; init; }
    public string Page { get; init; } = string.Empty;
    public string ReferenceBit { get; init; } = string.Empty;
    public string Distance { get; init; } = string.Empty;
    public bool IsPointerSlot { get; init; }
}
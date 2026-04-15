namespace Content.Schema;

public sealed record StudyCatalogDocument(
    string SourceSystem,
    IReadOnlyList<StudySectionDocument> Sections);

public sealed record StudySectionDocument(
    string Id,
    string Title,
    string Description,
    IReadOnlyList<StudyGroupDocument> Groups);

public sealed record StudyGroupDocument(
    string Title,
    IReadOnlyList<StudyTopicDocument> Topics);

public sealed record StudyTopicDocument(
    string Slug,
    string Title,
    string Summary,
    string SourceSystem,
    bool IsImported,
    string TargetPage);
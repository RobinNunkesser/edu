using System.Text.Json.Serialization;

namespace Content.Schema;

public sealed record ExerciseDocumentViewModel(
    string Title,
    string Intro,
    string SourceLabel,
    string SourceValue,
    string PrintHint,
    string PrintLabel,
    string ShowSolutionLabel,
    string HideSolutionLabel,
    IReadOnlyList<ExerciseSectionViewModel> Sections);

public sealed record ExerciseSectionViewModel(
    string Title,
    string Intro,
    IReadOnlyList<ExerciseBlockViewModel> Blocks);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ExerciseParagraphBlockViewModel), typeDiscriminator: "paragraph")]
[JsonDerivedType(typeof(ExerciseBinaryAdditionBlockViewModel), typeDiscriminator: "binary-addition")]
[JsonDerivedType(typeof(ExercisePromptAnswerBlockViewModel), typeDiscriminator: "prompt-answer")]
public abstract record ExerciseBlockViewModel;

public sealed record ExerciseParagraphBlockViewModel(string Text) : ExerciseBlockViewModel;

public sealed record ExerciseBinaryAdditionBlockViewModel(
    string Title,
    string Intro,
    string SolutionLabel,
    IReadOnlyList<BinaryAdditionVariantViewModel> Variants) : ExerciseBlockViewModel;

public sealed record ExercisePromptAnswerBlockViewModel(
    string Title,
    string Intro,
    string PromptLabel,
    string AnswerLabel,
    string SolutionLabel,
    IReadOnlyList<PromptAnswerVariantViewModel> Variants) : ExerciseBlockViewModel;

public sealed record BinaryAdditionVariantViewModel(
    string Label,
    string LeftAddend,
    string RightAddend,
    string Sum);

public sealed record PromptAnswerVariantViewModel(
    string Label,
    string Prompt,
    string Answer);
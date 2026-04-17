using System.Text.Json.Serialization;

namespace Content.Schema;

public enum ExercisePromptAnswerPresentation
{
    Default = 0,
    WideAnswerLine = 1,
    WorkingBox = 2,
    FigureWithAnswerArea = 3,
    TableWithAnswerArea = 4
}

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
[JsonDerivedType(typeof(ExerciseFigureResponseBlockViewModel), typeDiscriminator: "figure-response")]
[JsonDerivedType(typeof(ExerciseFigureTableResponseBlockViewModel), typeDiscriminator: "figure-table-response")]
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
    ExercisePromptAnswerPresentation Presentation,
    IReadOnlyList<PromptAnswerVariantViewModel> Variants) : ExerciseBlockViewModel;

public sealed record ExerciseFigureResponseBlockViewModel(
    string Title,
    string Intro,
    string FigureSource,
    string FigureAltText,
    string AnswerLabel,
    string SolutionLabel,
    string SolutionText,
    int AnswerLineCount) : ExerciseBlockViewModel;

public sealed record ExerciseFigureTableResponseBlockViewModel(
    string Title,
    string Intro,
    string FigureSource,
    string FigureAltText,
    string RowHeaderLabel,
    string TableAriaLabel,
    IReadOnlyList<ExerciseTableColumnViewModel> Columns,
    IReadOnlyList<ExerciseTableRowViewModel> Rows,
    string AnswerLabel,
    string SolutionLabel,
    string SolutionText,
    int AnswerLineCount) : ExerciseBlockViewModel;

public sealed record ExerciseTableColumnViewModel(string Header);

public sealed record ExerciseTableRowViewModel(
    string Header,
    IReadOnlyList<string> Cells);

public sealed record BinaryAdditionVariantViewModel(
    string Label,
    string LeftAddend,
    string RightAddend,
    string Sum);

public sealed record PromptAnswerVariantViewModel(
    string Label,
    string Prompt,
    string Answer);
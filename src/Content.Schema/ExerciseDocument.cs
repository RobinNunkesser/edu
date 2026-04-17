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
[JsonDerivedType(typeof(ExerciseWorksheetResponseBlockViewModel), typeDiscriminator: "worksheet-response")]
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

public sealed record ExerciseWorksheetResponseBlockViewModel(
    string Title,
    string Intro,
    ExerciseFigureViewModel? Figure,
    ExerciseResponseTableViewModel? Table,
    string AnswerLabel,
    string SolutionLabel,
    string SolutionText,
    int AnswerLineCount) : ExerciseBlockViewModel;

public sealed record ExerciseFigureViewModel(
    string Source,
    string AltText);

public sealed record ExerciseResponseTableViewModel(
    string RowHeaderLabel,
    string TableAriaLabel,
    IReadOnlyList<ExerciseTableColumnViewModel> Columns,
    IReadOnlyList<ExerciseTableRowViewModel> Rows);

public sealed record ExerciseTableColumnViewModel(string Header);

public sealed record ExerciseTableRowViewModel(
    string Header,
    IReadOnlyList<string> Cells,
    IReadOnlyList<string>? SolutionCells = null);

public sealed record BinaryAdditionVariantViewModel(
    string Label,
    string LeftAddend,
    string RightAddend,
    string Sum);

public sealed record PromptAnswerVariantViewModel(
    string Label,
    string Prompt,
    string Answer);
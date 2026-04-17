using Content.Schema;
using Italbytz.ComputingSystems;
using Shared.Domain;

namespace Shared.Application;

public static class ExerciseDocumentFactory
{
    public static ExerciseDocumentViewModel CreateByKey(SiteLanguage language, string exerciseKey)
        => exerciseKey switch
        {
            "binary-addition" => CreateBinaryAddition(language),
            "binary-to-decimal" => CreateBinaryToDecimal(language),
            "decimal-to-binary" => CreateDecimalToBinary(language),
            "twos-complement" => CreateTwosComplement(language),
            _ => CreateBinaryAddition(language)
        };

    public static ExerciseDocumentViewModel CreateBySlug(SiteLanguage language, string slug)
        => CreateByKey(language, slug switch
        {
            "binaere-addition" or "binary-addition" => "binary-addition",
            "binaer-zu-dezimal" or "binary-to-decimal" => "binary-to-decimal",
            "dezimal-zu-binaer" or "decimal-to-binary" => "decimal-to-binary",
            "zweierkomplement" or "twos-complement" => "twos-complement",
            _ => "binary-addition"
        });

    public static ExerciseDocumentViewModel CreateBinaryAddition(SiteLanguage language)
    {
        var isEnglish = language == SiteLanguage.En;
        var variants = Enumerable.Range(0, 2)
            .Select(index => CreateVariant(((char)('A' + index)).ToString()))
            .ToArray();

        return new ExerciseDocumentViewModel(
            Title: isEnglish ? "Printable single-task sheet" : "Druckbares Einzelaufgabenblatt",
            Intro: isEnglish
                ? "This slice keeps the task semantics separate from the rendering. The same neutral document could later be rendered as HTML in edu and as LaTeX in the Exam Generator."
                : "Dieser Durchstich trennt Aufgabensemantik und Rendering. Dasselbe neutrale Dokument kann spaeter in edu als HTML und im Exam Generator als LaTeX gerendert werden.",
            SourceLabel: isEnglish ? "Source" : "Quelle",
            SourceValue: "Exam Generator / edu POC",
            PrintHint: isEnglish
                ? "Use the browser print dialog for a PDF export."
                : "Nutze den Browser-Druckdialog fuer einen PDF-Export.",
            PrintLabel: isEnglish ? "Print / PDF" : "Drucken / PDF",
            ShowSolutionLabel: isEnglish ? "Show solution" : "Loesung zeigen",
            HideSolutionLabel: isEnglish ? "Hide solution" : "Loesung ausblenden",
            Sections:
            [
                new ExerciseSectionViewModel(
                    Title: isEnglish ? "Task" : "Aufgabe",
                    Intro: isEnglish
                        ? "Add the following binary numbers. Work directly in the provided fields or print the sheet."
                        : "Addiere die folgenden Binaerzahlen. Arbeite direkt in den vorgesehenen Feldern oder drucke das Blatt aus.",
                    Blocks:
                    [
                        new ExerciseParagraphBlockViewModel(isEnglish
                            ? "The document model only contains task text, variants and optional solution data. Layout decisions stay in the renderer."
                            : "Das Dokumentmodell enthaelt nur Aufgabentext, Varianten und optionale Loesungsdaten. Layout-Entscheidungen bleiben im Renderer."),
                        new ExerciseBinaryAdditionBlockViewModel(
                            Title: isEnglish ? "Binary addition" : "Binaere Addition",
                            Intro: isEnglish
                                ? "Two variants are provided to demonstrate reuse for worksheets and worked solutions."
                                : "Zwei Varianten zeigen, wie sich dieselbe Struktur fuer Aufgabenblatt und Musterloesung wiederverwenden laesst.",
                            SolutionLabel: isEnglish ? "Solution" : "Loesung",
                            Variants: variants)
                    ])
            ]);
    }

    public static ExerciseDocumentViewModel CreateBinaryToDecimal(SiteLanguage language)
    {
        var isEnglish = language == SiteLanguage.En;
        var variants = Enumerable.Range(0, 2)
            .Select(index => CreateBinaryToDecimalVariant(((char)('A' + index)).ToString()))
            .ToArray();

        return CreatePromptAnswerDocument(
            language,
            titleDe: "Druckbares Einzelaufgabenblatt",
            titleEn: "Printable single-task sheet",
            introDe: "Dieses Einzelaufgabenblatt nutzt dieselbe neutrale Dokumentstruktur fuer Umwandlungsaufgaben.",
            introEn: "This worksheet uses the same neutral document structure for conversion exercises.",
            sectionIntroDe: "Wandle die folgenden Binaerzahlen in Dezimalzahlen um.",
            sectionIntroEn: "Convert the following binary numbers to decimal values.",
            blockTitleDe: "Binaer nach Dezimal",
            blockTitleEn: "Binary to decimal",
            blockIntroDe: "Die Werte werden aus echter Solver-Logik erzeugt.",
            blockIntroEn: "The values come from real solver logic.",
            promptLabelDe: "Binaerzahl",
            promptLabelEn: "Binary value",
            answerLabelDe: "Dezimalzahl",
            answerLabelEn: "Decimal value",
            variants: variants);
    }

    public static ExerciseDocumentViewModel CreateDecimalToBinary(SiteLanguage language)
    {
        var isEnglish = language == SiteLanguage.En;
        var variants = Enumerable.Range(0, 2)
            .Select(index => CreateDecimalToBinaryVariant(((char)('A' + index)).ToString()))
            .ToArray();

        return CreatePromptAnswerDocument(
            language,
            titleDe: "Druckbares Einzelaufgabenblatt",
            titleEn: "Printable single-task sheet",
            introDe: "Dieses Einzelaufgabenblatt nutzt dieselbe neutrale Dokumentstruktur fuer Umwandlungsaufgaben.",
            introEn: "This worksheet uses the same neutral document structure for conversion exercises.",
            sectionIntroDe: "Wandle die folgenden Dezimalzahlen in Binaerzahlen um.",
            sectionIntroEn: "Convert the following decimal numbers to binary values.",
            blockTitleDe: "Dezimal nach Binaer",
            blockTitleEn: "Decimal to binary",
            blockIntroDe: "Die Werte werden aus echter Solver-Logik erzeugt.",
            blockIntroEn: "The values come from real solver logic.",
            promptLabelDe: "Dezimalzahl",
            promptLabelEn: "Decimal value",
            answerLabelDe: "Binaerzahl",
            answerLabelEn: "Binary value",
            variants: variants);
    }

    public static ExerciseDocumentViewModel CreateTwosComplement(SiteLanguage language)
    {
        var variants = Enumerable.Range(0, 2)
            .Select(index => CreateTwosComplementVariant(((char)('A' + index)).ToString()))
            .ToArray();

        return CreatePromptAnswerDocument(
            language,
            titleDe: "Druckbares Einzelaufgabenblatt",
            titleEn: "Printable single-task sheet",
            introDe: "Dieses Einzelaufgabenblatt nutzt dieselbe neutrale Dokumentstruktur fuer Zweierkomplement-Aufgaben.",
            introEn: "This worksheet uses the same neutral document structure for two's complement exercises.",
            sectionIntroDe: "Berechne die Zweierkomplementdarstellung der folgenden negativen Zahlen bei 8 Bit.",
            sectionIntroEn: "Compute the two's complement representation of the following negative numbers using 8 bits.",
            blockTitleDe: "Zweierkomplement",
            blockTitleEn: "Two's complement",
            blockIntroDe: "Die Werte werden aus echter Solver-Logik erzeugt.",
            blockIntroEn: "The values come from real solver logic.",
            promptLabelDe: "Dezimalzahl",
            promptLabelEn: "Decimal value",
            answerLabelDe: "Zweierkomplement",
            answerLabelEn: "Two's complement",
            variants: variants);
    }

    private static ExerciseDocumentViewModel CreatePromptAnswerDocument(
        SiteLanguage language,
        string titleDe,
        string titleEn,
        string introDe,
        string introEn,
        string sectionIntroDe,
        string sectionIntroEn,
        string blockTitleDe,
        string blockTitleEn,
        string blockIntroDe,
        string blockIntroEn,
        string promptLabelDe,
        string promptLabelEn,
        string answerLabelDe,
        string answerLabelEn,
        IReadOnlyList<PromptAnswerVariantViewModel> variants)
    {
        var isEnglish = language == SiteLanguage.En;

        return new ExerciseDocumentViewModel(
            Title: isEnglish ? titleEn : titleDe,
            Intro: isEnglish ? introEn : introDe,
            SourceLabel: isEnglish ? "Source" : "Quelle",
            SourceValue: "Exam Generator / edu POC",
            PrintHint: isEnglish ? "Use the browser print dialog for a PDF export." : "Nutze den Browser-Druckdialog fuer einen PDF-Export.",
            PrintLabel: isEnglish ? "Print / PDF" : "Drucken / PDF",
            ShowSolutionLabel: isEnglish ? "Show solution" : "Loesung zeigen",
            HideSolutionLabel: isEnglish ? "Hide solution" : "Loesung ausblenden",
            Sections:
            [
                new ExerciseSectionViewModel(
                    Title: isEnglish ? "Task" : "Aufgabe",
                    Intro: isEnglish ? sectionIntroEn : sectionIntroDe,
                    Blocks:
                    [
                        new ExerciseParagraphBlockViewModel(isEnglish
                            ? "The document model stays format-neutral. HTML and LaTeX are just different renderers over the same payload."
                            : "Das Dokumentmodell bleibt formatneutral. HTML und LaTeX sind nur verschiedene Renderer ueber derselben Nutzlast."),
                        new ExercisePromptAnswerBlockViewModel(
                            Title: isEnglish ? blockTitleEn : blockTitleDe,
                            Intro: isEnglish ? blockIntroEn : blockIntroDe,
                            PromptLabel: isEnglish ? promptLabelEn : promptLabelDe,
                            AnswerLabel: isEnglish ? answerLabelEn : answerLabelDe,
                            SolutionLabel: isEnglish ? "Solution" : "Loesung",
                            Variants: variants)
                    ])
            ]);
    }

    private static BinaryAdditionVariantViewModel CreateVariant(string label)
    {
        var parameters = new BinaryAdditionParameters();
        var solver = new BinaryAdditionSolver();
        var solution = solver.Solve(parameters);

        return new BinaryAdditionVariantViewModel(
            Label: label,
            LeftAddend: Convert.ToString(parameters.Summand1, 2).PadLeft(9, '0'),
            RightAddend: Convert.ToString(parameters.Summand2, 2).PadLeft(9, '0'),
            Sum: Convert.ToString(solution.Sum, 2).PadLeft(9, '0'));
    }

    private static PromptAnswerVariantViewModel CreateBinaryToDecimalVariant(string label)
    {
        var parameters = new BinaryToDecimalParameters();
        var solver = new BinaryToDecimalSolver();
        var solution = solver.Solve(parameters);

        return new PromptAnswerVariantViewModel(
            Label: label,
            Prompt: Convert.ToString(parameters.Binary, 2).PadLeft(8, '0'),
            Answer: solution.Decimal.ToString());
    }

    private static PromptAnswerVariantViewModel CreateDecimalToBinaryVariant(string label)
    {
        var parameters = new DecimalToBinaryParameters();
        var solver = new DecimalToBinarySolver();
        var solution = solver.Solve(parameters);

        return new PromptAnswerVariantViewModel(
            Label: label,
            Prompt: parameters.Decimal.ToString(),
            Answer: Convert.ToString(solution.Binary, 2).PadLeft(8, '0'));
    }

    private static PromptAnswerVariantViewModel CreateTwosComplementVariant(string label)
    {
        var parameters = new TwosComplementParameters();
        var solver = new TwosComplementSolver();
        var solution = solver.Solve(parameters);

        return new PromptAnswerVariantViewModel(
            Label: label,
            Prompt: $"-{parameters.PositiveBinary}",
            Answer: Convert.ToString(solution.ComplementBinary, 2)[8..]);
    }
}
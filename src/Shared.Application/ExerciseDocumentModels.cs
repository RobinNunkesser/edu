using Content.Schema;
using Italbytz.ComputingSystems;
using Shared.Domain;

namespace Shared.Application;

public static class ExerciseDocumentFactory
{
    private static readonly string[] ShortestPathNodeHeaders = ["A", "B", "C", "D", "E", "F", "G", "H"];
    private static readonly string[] ShortestPathMetricHeaders = ["D", "V", "D", "V", "D", "V", "D", "V", "D", "V", "D", "V", "D", "V"];
    private static readonly string[] SpanningTreeSolutionEdges = ["A - B (2)", "B - D (3)", "B - E (4)", "D - G (2)", "E - H (1)", "A - C (5)", "C - F (5)"];

    public static ExerciseDocumentViewModel CreateByKey(SiteLanguage language, string exerciseKey)
        => exerciseKey switch
        {
            "binary-addition" => CreateBinaryAddition(language),
            "binary-to-decimal" => CreateBinaryToDecimal(language),
            "decimal-to-binary" => CreateDecimalToBinary(language),
            "shortest-path" => CreateShortestPath(language),
            "spanning-tree" => CreateSpanningTree(language),
            "twos-complement" => CreateTwosComplement(language),
            _ => CreateBinaryAddition(language)
        };

    public static ExerciseDocumentViewModel CreateBySlug(SiteLanguage language, string slug)
        => CreateByKey(language, slug switch
        {
            "binaere-addition" or "binary-addition" => "binary-addition",
            "binaer-zu-dezimal" or "binary-to-decimal" => "binary-to-decimal",
            "dezimal-zu-binaer" or "decimal-to-binary" => "decimal-to-binary",
            "kuerzeste-wege" or "shortest-path" => "shortest-path",
            "spannbaum" or "spanning-tree" => "spanning-tree",
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
            presentation: ExercisePromptAnswerPresentation.WideAnswerLine,
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
            presentation: ExercisePromptAnswerPresentation.WorkingBox,
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
            presentation: ExercisePromptAnswerPresentation.WideAnswerLine,
            variants: variants);
    }

    public static ExerciseDocumentViewModel CreateShortestPath(SiteLanguage language)
    {
        var isEnglish = language == SiteLanguage.En;

        return new ExerciseDocumentViewModel(
            Title: isEnglish ? "Printable single-task sheet" : "Druckbares Einzelaufgabenblatt",
            Intro: isEnglish
                ? "This worksheet exposes the generalized graph-and-table response block with the same semantic structure that now also feeds the LaTeX renderer in the Exam Generator."
                : "Dieses Arbeitsblatt macht den generalisierten Graph-und-Tabellen-Block sichtbar, der jetzt in gleicher semantischer Struktur auch den LaTeX-Renderer im Exam Generator speist.",
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
                    Intro: isEnglish
                        ? "Determine the shortest paths in the weighted graph. Use A as the start node and document your work in the worksheet table."
                        : "Bestimme die kuerzesten Wege im gewichteten Graphen. Verwende A als Startknoten und dokumentiere deinen Rechenweg in der Arbeitstabelle.",
                    Blocks:
                    [
                        new ExerciseParagraphBlockViewModel(isEnglish
                            ? "This example makes the worksheet-response block visible in edu: one figure, one reusable work table and one shared solution area."
                            : "Dieses Beispiel macht den Worksheet-Response-Block in edu sichtbar: eine Abbildung, eine wiederverwendbare Arbeitstabelle und ein gemeinsamer Loesungsbereich."),
                        new ExerciseWorksheetResponseBlockViewModel(
                            Title: isEnglish ? "Shortest paths" : "Kuerzeste Wege",
                            Intro: isEnglish
                                ? "Carry out the computation on the shown graph. Record distance estimates D and predecessors V after each step."
                                : "Fuehre die Berechnung im gezeigten Graphen durch. Trage nach jedem Schritt Distanzschaetzungen D und Vorgaenger V ein.",
                            Figure: new ExerciseFigureViewModel(
                                Source: "content/study/shortest-path-graph.svg",
                                AltText: isEnglish ? "Weighted graph for a shortest-path worksheet" : "Gewichteter Graph fuer ein Kuerzeste-Wege-Arbeitsblatt"),
                            Table: new ExerciseResponseTableViewModel(
                                RowHeaderLabel: "K",
                                TableAriaLabel: isEnglish ? "Worksheet table for shortest paths" : "Arbeitstabelle fuer kuerzeste Wege",
                                Columns: ShortestPathMetricHeaders.Select(header => new ExerciseTableColumnViewModel(header)).ToArray(),
                                Rows: ShortestPathNodeHeaders
                                    .Select(node => new ExerciseTableRowViewModel(
                                        Header: node,
                                        Cells: Enumerable.Repeat(string.Empty, ShortestPathMetricHeaders.Length).ToArray()))
                                    .ToArray()),
                            AnswerLabel: isEnglish ? "Worksheet" : "Arbeitsblatt",
                            SolutionLabel: isEnglish ? "Solution" : "Loesung",
                            SolutionText: CreateShortestPathSolutionText(isEnglish),
                            AnswerLineCount: 8)
                    ])
            ]);
    }

    public static ExerciseDocumentViewModel CreateSpanningTree(SiteLanguage language)
    {
        var isEnglish = language == SiteLanguage.En;

        return new ExerciseDocumentViewModel(
            Title: isEnglish ? "Printable single-task sheet" : "Druckbares Einzelaufgabenblatt",
            Intro: isEnglish
                ? "This worksheet uses the same generalized worksheet-response block for a graph task without a work table."
                : "Dieses Arbeitsblatt nutzt denselben generalisierten Worksheet-Response-Block fuer eine Graphaufgabe ohne Arbeitstabelle.",
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
                    Intro: isEnglish
                        ? "Determine a minimum spanning tree for the weighted graph and list the selected edges in the answer area."
                        : "Bestimme einen minimalen Spannbaum fuer den gewichteten Graphen und notiere die gewaehlen Kanten im Antwortbereich.",
                    Blocks:
                    [
                        new ExerciseParagraphBlockViewModel(isEnglish
                            ? "This second complex example reuses the same block type as the shortest-path worksheet, but without a response table."
                            : "Dieses zweite komplexe Beispiel verwendet denselben Blocktyp wie das Shortest-Path-Arbeitsblatt, aber ohne Antworttabelle."),
                        new ExerciseWorksheetResponseBlockViewModel(
                            Title: isEnglish ? "Minimum spanning tree" : "Minimaler Spannbaum",
                            Intro: isEnglish
                                ? "Use the graph as the basis for Prim or Kruskal and record the chosen edges in the answer area."
                                : "Nutze den Graphen als Grundlage fuer Prim oder Kruskal und notiere die gewaehlen Kanten im Antwortbereich.",
                            Figure: new ExerciseFigureViewModel(
                                Source: "content/study/spanning-tree-graph.svg",
                                AltText: isEnglish ? "Weighted graph for a minimum-spanning-tree worksheet" : "Gewichteter Graph fuer ein Spannbaum-Arbeitsblatt"),
                            Table: null,
                            AnswerLabel: isEnglish ? "Answer" : "Antwort",
                            SolutionLabel: isEnglish ? "Solution" : "Loesung",
                            SolutionText: string.Join("\n", SpanningTreeSolutionEdges),
                            AnswerLineCount: 8)
                    ])
            ]);
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
        ExercisePromptAnswerPresentation presentation,
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
                            Presentation: presentation,
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

    private static string CreateShortestPathSolutionText(bool isEnglish)
    {
        return string.Join("\n\n",
        [
            isEnglish ? "A: 0" : "A: 0",
            isEnglish ? "B: A -> B (2)" : "B: A -> B (2)",
            isEnglish ? "C: A -> C (5)" : "C: A -> C (5)",
            isEnglish ? "D: A -> B -> D (5)" : "D: A -> B -> D (5)",
            isEnglish ? "E: A -> B -> E (6)" : "E: A -> B -> E (6)",
            isEnglish ? "F: A -> C -> F (10)" : "F: A -> C -> F (10)",
            isEnglish ? "G: A -> B -> D -> G (7)" : "G: A -> B -> D -> G (7)",
            isEnglish ? "H: A -> B -> E -> H (7)" : "H: A -> B -> E -> H (7)"
        ]);
    }
}
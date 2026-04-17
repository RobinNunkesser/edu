using System.Text;
using System.Text.Json;
using Content.Schema;
using ExamGenerator.Core.Tasks;
using ExamGenerator.Infrastructure.LaTeX.Adapters;
using ExamGenerator.Core.Exams;
using Italbytz.Exam.Abstractions;
using Microsoft.Extensions.DependencyInjection;

var arguments = CliArguments.Parse(args);
var outputJsonPath = Path.GetFullPath(arguments.OutputJsonPath);
var outputTexPath = string.IsNullOrWhiteSpace(arguments.OutputTexPath)
	? null
	: Path.GetFullPath(arguments.OutputTexPath);

var services = new ServiceCollection();
services.AddSingleton<ITaskTextGenerator<BinaryAdditionTask>, BinaryAdditionTextGenerator>();
services.AddSingleton<ITaskTextGenerator<BinaryToDecimalTask>, BinaryToDecimalTextGenerator>();
services.AddSingleton<ITaskTextGenerator<DecimalToBinaryTask>, DecimalToBinaryTextGenerator>();
services.AddSingleton<ITaskTextGenerator<TwosComplementTask>, TwosComplementTextGenerator>();

using var serviceProvider = services.BuildServiceProvider();

var document = ExamTaskExerciseMapper.Map(arguments.Task, serviceProvider, arguments.Language);

Directory.CreateDirectory(Path.GetDirectoryName(outputJsonPath)!);
File.WriteAllText(
	outputJsonPath,
	JsonSerializer.Serialize(document, JsonOptions.Default),
	new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

if (outputTexPath is not null)
{
	Directory.CreateDirectory(Path.GetDirectoryName(outputTexPath)!);
	File.WriteAllText(outputTexPath, ExerciseDocumentLaTeXWriter.Write(document, includeSolutions: false), new UTF8Encoding(false));
	var solutionTexPath = Path.Combine(
		Path.GetDirectoryName(outputTexPath)!,
		$"{Path.GetFileNameWithoutExtension(outputTexPath)}.solution{Path.GetExtension(outputTexPath)}");
	File.WriteAllText(solutionTexPath, ExerciseDocumentLaTeXWriter.Write(document, includeSolutions: true), new UTF8Encoding(false));
}

Console.WriteLine($"Wrote exercise document JSON to {outputJsonPath}");
if (outputTexPath is not null)
{
	Console.WriteLine($"Wrote exercise document LaTeX to {outputTexPath}");
	Console.WriteLine($"Wrote exercise document solution LaTeX to {Path.Combine(Path.GetDirectoryName(outputTexPath)!, $"{Path.GetFileNameWithoutExtension(outputTexPath)}.solution{Path.GetExtension(outputTexPath)}")}");
}

internal sealed record CliArguments(string OutputJsonPath, string? OutputTexPath, string Language, string Task)
{
	public static CliArguments Parse(string[] args)
	{
		string? outputJsonPath = null;
		string? outputTexPath = null;
		var language = "de";
		var task = "binary-addition";

		for (var index = 0; index < args.Length; index++)
		{
			switch (args[index])
			{
				case "--output-json":
					outputJsonPath = GetValue(args, ++index, "--output-json");
					break;
				case "--output-tex":
					outputTexPath = GetValue(args, ++index, "--output-tex");
					break;
				case "--language":
					language = GetValue(args, ++index, "--language");
					break;
				case "--task":
					task = GetValue(args, ++index, "--task");
					break;
			}
		}

		if (string.IsNullOrWhiteSpace(outputJsonPath))
		{
			throw new ArgumentException("Usage: --output-json <path> [--output-tex <path>] [--language de|en] [--task binary-addition|binary-to-decimal|decimal-to-binary|twos-complement]");
		}

		return new CliArguments(outputJsonPath, outputTexPath, language, task);
	}

	private static string GetValue(string[] args, int index, string optionName)
	{
		if (index >= args.Length)
		{
			throw new ArgumentException($"Missing value for {optionName}");
		}

		return args[index];
	}
}

internal static class ExamTaskExerciseMapper
{
	public static ExerciseDocumentViewModel Map(string taskName, IServiceProvider serviceProvider, string language)
		=> taskName switch
		{
			"binary-to-decimal" => MapBinaryToDecimal(new BinaryToDecimalTask(serviceProvider, 0), language),
			"decimal-to-binary" => MapDecimalToBinary(new DecimalToBinaryTask(serviceProvider, 0), language),
			"twos-complement" => MapTwosComplement(new TwosComplementTask(serviceProvider, 0), language),
			_ => MapBinaryAddition(new BinaryAdditionTask(serviceProvider, 0), language)
		};

	public static ExerciseDocumentViewModel MapBinaryAddition(BinaryAdditionTask task, string language)
	{
		var isEnglish = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase);
		var variants = task.Parameters
			.Zip(task.Solutions, (parameters, solution) => (parameters, solution))
			.Select((entry, index) => new BinaryAdditionVariantViewModel(
				Label: ((char)('A' + index)).ToString(),
				LeftAddend: Convert.ToString(entry.parameters.Summand1, 2).PadLeft(9, '0'),
				RightAddend: Convert.ToString(entry.parameters.Summand2, 2).PadLeft(9, '0'),
				Sum: Convert.ToString(entry.solution.Sum, 2).PadLeft(9, '0')))
			.ToArray();

		return new ExerciseDocumentViewModel(
			Title: isEnglish ? "Printable single-task sheet" : "Druckbares Einzelaufgabenblatt",
			Intro: isEnglish
				? "This document was generated from an actual Exam Generator task instance and mapped to the shared edu exercise schema."
				: "Dieses Dokument wurde aus einer echten Exam-Generator-Task-Instanz erzeugt und auf das gemeinsame edu-Exercise-Schema abgebildet.",
			SourceLabel: isEnglish ? "Source" : "Quelle",
			SourceValue: "Exam Generator",
			PrintHint: isEnglish ? "Use browser print or convert the generated LaTeX in a downstream step." : "Nutze den Browser-Druck oder konvertiere das erzeugte LaTeX in einem nachgelagerten Schritt.",
			PrintLabel: isEnglish ? "Print / PDF" : "Drucken / PDF",
			ShowSolutionLabel: isEnglish ? "Show solution" : "Loesung zeigen",
			HideSolutionLabel: isEnglish ? "Hide solution" : "Loesung ausblenden",
			Sections:
			[
				new ExerciseSectionViewModel(
					Title: isEnglish ? "Task" : "Aufgabe",
					Intro: isEnglish
						? "Add the following binary numbers."
						: "Addiere die folgenden Binaerzahlen.",
					Blocks:
					[
						new ExerciseParagraphBlockViewModel(isEnglish
							? "The semantic payload is independent of HTML and LaTeX rendering."
							: "Die semantische Nutzlast ist unabhaengig von HTML- und LaTeX-Rendering."),
						new ExerciseBinaryAdditionBlockViewModel(
							Title: isEnglish ? "Binary addition" : "Binaere Addition",
							Intro: isEnglish
								? "The values come directly from the Exam Generator task parameters and solutions."
								: "Die Werte stammen direkt aus den Parametern und Loesungen der Exam-Generator-Task.",
							SolutionLabel: isEnglish ? "Solution" : "Loesung",
							Variants: variants)
					])
			]);
	}

	public static ExerciseDocumentViewModel MapBinaryToDecimal(BinaryToDecimalTask task, string language)
	{
		var isEnglish = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase);
		var variants = task.Parameters
			.Zip(task.Solutions, (parameters, solution) => (parameters, solution))
			.Select((entry, index) => new PromptAnswerVariantViewModel(
				Label: ((char)('A' + index)).ToString(),
				Prompt: Convert.ToString(entry.parameters.Binary, 2).PadLeft(8, '0'),
				Answer: entry.solution.Decimal.ToString()))
			.ToArray();

		return CreatePromptAnswerDocument(
			language,
			isEnglish ? "Binary to decimal" : "Binaer nach Dezimal",
			isEnglish ? "Convert the following binary numbers to decimal values." : "Wandle die folgenden Binaerzahlen in Dezimalzahlen um.",
			isEnglish ? "Binary value" : "Binaerzahl",
			isEnglish ? "Decimal value" : "Dezimalzahl",
			variants);
	}

	public static ExerciseDocumentViewModel MapDecimalToBinary(DecimalToBinaryTask task, string language)
	{
		var isEnglish = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase);
		var variants = task.Parameters
			.Zip(task.Solutions, (parameters, solution) => (parameters, solution))
			.Select((entry, index) => new PromptAnswerVariantViewModel(
				Label: ((char)('A' + index)).ToString(),
				Prompt: entry.parameters.Decimal.ToString(),
				Answer: Convert.ToString(entry.solution.Binary, 2).PadLeft(8, '0')))
			.ToArray();

		return CreatePromptAnswerDocument(
			language,
			isEnglish ? "Decimal to binary" : "Dezimal nach Binaer",
			isEnglish ? "Convert the following decimal numbers to binary values." : "Wandle die folgenden Dezimalzahlen in Binaerzahlen um.",
			isEnglish ? "Decimal value" : "Dezimalzahl",
			isEnglish ? "Binary value" : "Binaerzahl",
			variants);
	}

	public static ExerciseDocumentViewModel MapTwosComplement(TwosComplementTask task, string language)
	{
		var isEnglish = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase);
		var variants = task.Parameters
			.Zip(task.Solutions, (parameters, solution) => (parameters, solution))
			.Select((entry, index) => new PromptAnswerVariantViewModel(
				Label: ((char)('A' + index)).ToString(),
				Prompt: $"-{entry.parameters.PositiveBinary}",
				Answer: Convert.ToString(entry.solution.ComplementBinary, 2)[8..]))
			.ToArray();

		return CreatePromptAnswerDocument(
			language,
			isEnglish ? "Two's complement" : "Zweierkomplement",
			isEnglish ? "Compute the 8-bit two's complement representation of the following negative numbers." : "Berechne die 8-Bit-Zweierkomplementdarstellung der folgenden negativen Zahlen.",
			isEnglish ? "Decimal value" : "Dezimalzahl",
			isEnglish ? "Two's complement" : "Zweierkomplement",
			variants);
	}

	private static ExerciseDocumentViewModel CreatePromptAnswerDocument(
		string language,
		string title,
		string sectionIntro,
		string promptLabel,
		string answerLabel,
		IReadOnlyList<PromptAnswerVariantViewModel> variants)
	{
		var isEnglish = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase);
		return new ExerciseDocumentViewModel(
			Title: isEnglish ? "Printable single-task sheet" : "Druckbares Einzelaufgabenblatt",
			Intro: isEnglish
				? "This document was generated from an actual Exam Generator task instance and mapped to the shared edu exercise schema."
				: "Dieses Dokument wurde aus einer echten Exam-Generator-Task-Instanz erzeugt und auf das gemeinsame edu-Exercise-Schema abgebildet.",
			SourceLabel: isEnglish ? "Source" : "Quelle",
			SourceValue: "Exam Generator",
			PrintHint: isEnglish ? "Use browser print or convert the generated LaTeX in a downstream step." : "Nutze den Browser-Druck oder konvertiere das erzeugte LaTeX in einem nachgelagerten Schritt.",
			PrintLabel: isEnglish ? "Print / PDF" : "Drucken / PDF",
			ShowSolutionLabel: isEnglish ? "Show solution" : "Loesung zeigen",
			HideSolutionLabel: isEnglish ? "Hide solution" : "Loesung ausblenden",
			Sections:
			[
				new ExerciseSectionViewModel(
					Title: isEnglish ? "Task" : "Aufgabe",
					Intro: sectionIntro,
					Blocks:
					[
						new ExerciseParagraphBlockViewModel(isEnglish
							? "The semantic payload is independent of HTML and LaTeX rendering."
							: "Die semantische Nutzlast ist unabhaengig von HTML- und LaTeX-Rendering."),
						new ExercisePromptAnswerBlockViewModel(
							Title: title,
							Intro: isEnglish
								? "The values come directly from the Exam Generator task parameters and solutions."
								: "Die Werte stammen direkt aus den Parametern und Loesungen der Exam-Generator-Task.",
							PromptLabel: promptLabel,
							AnswerLabel: answerLabel,
							SolutionLabel: isEnglish ? "Solution" : "Loesung",
							Variants: variants)
					])
			]);
	}
}

internal static class ExerciseDocumentLaTeXWriter
{
	public static string Write(ExerciseDocumentViewModel document, bool includeSolutions)
	{
		var builder = new StringBuilder();
		builder.AppendLine("\\documentclass[12pt]{hshlexercise}");
		builder.AppendLine(includeSolutions
			? "\\newboolean{solution}\\setboolean{solution}{true}"
			: "\\newboolean{solution}\\setboolean{solution}{false}");
		builder.AppendLine("\\pdfpageheight\\paperheight");
		builder.AppendLine("\\pdfpagewidth\\paperwidth");
		builder.AppendLine("\\usepackage[parfill]{parskip}");
		builder.AppendLine("\\usepackage{graphicx}");
		builder.AppendLine("\\usepackage[T1]{fontenc}");
		builder.AppendLine("\\usepackage[utf8]{inputenc}");
		builder.AppendLine("\\usepackage{array}");
		builder.AppendLine("\\usepackage{longtable}");
		builder.AppendLine("\\begin{document}");
		builder.AppendLine($"\\uebungsname{{{Escape(document.Title)} \\ifthenelse{{\\boolean{{solution}}}}{{- Loesung}}{{}}}}");
		builder.AppendLine();
		builder.AppendLine(Escape(document.Intro));
		builder.AppendLine();
		builder.AppendLine($"\\textbf{{{Escape(document.SourceLabel)}}}: {Escape(document.SourceValue)}");
		builder.AppendLine();

		foreach (var section in document.Sections)
		{
			builder.AppendLine($"\\aufgabe{{}}{{{Escape(section.Title)}}}");
			builder.AppendLine(Escape(section.Intro));
			builder.AppendLine();

			foreach (var block in section.Blocks)
			{
				switch (block)
				{
					case ExerciseParagraphBlockViewModel paragraph:
						builder.AppendLine(Escape(paragraph.Text));
						builder.AppendLine();
						break;
					case ExerciseBinaryAdditionBlockViewModel addition:
						builder.AppendLine($"\\paragraph{{{Escape(addition.Title)}}} {Escape(addition.Intro)}");
						builder.AppendLine();
						foreach (var variant in addition.Variants)
						{
							builder.AppendLine($"\\textbf{{{Escape(variant.Label)}}}");
							builder.AppendLine("\\[");
							builder.AppendLine("\\begin{array}{r}");
							builder.AppendLine($"{EscapeMonospace(variant.LeftAddend)} \\\\");
							builder.AppendLine($"+ {EscapeMonospace(variant.RightAddend)} \\\\");
							builder.AppendLine("\\hline");
							builder.AppendLine(includeSolutions
								? EscapeMonospace(variant.Sum)
								: EscapeMonospace(new string('_', variant.Sum.Length)));
							builder.AppendLine("\\end{array}");
							builder.AppendLine("\\]");
						}
						break;
					case ExercisePromptAnswerBlockViewModel promptAnswer:
						builder.AppendLine($"\\paragraph{{{Escape(promptAnswer.Title)}}} {Escape(promptAnswer.Intro)}");
						builder.AppendLine("\\begin{longtable}{p{0.08\\textwidth}p{0.38\\textwidth}p{0.38\\textwidth}}");
						builder.AppendLine($"\\textbf{{Nr.}} & \\textbf{{{Escape(promptAnswer.PromptLabel)}}} & \\textbf{{{Escape(includeSolutions ? promptAnswer.SolutionLabel : promptAnswer.AnswerLabel)}}} \\\\");
						builder.AppendLine("\\hline");
						foreach (var variant in promptAnswer.Variants)
						{
							var answer = includeSolutions ? variant.Answer : new string('_', Math.Max(6, variant.Answer.Length));
							builder.AppendLine($"{Escape(variant.Label)} & {EscapeMonospace(variant.Prompt)} & {EscapeMonospace(answer)} \\\\");
						}
						builder.AppendLine("\\end{longtable}");
						builder.AppendLine();
						break;
				}
			}
		}

		builder.AppendLine("\\label{LastPage}");
		builder.AppendLine("\\end{document}");
		return builder.ToString();
	}

	private static string Escape(string value)
		=> value
			.Replace("\\", "\\textbackslash{}")
			.Replace("&", "\\&")
			.Replace("%", "\\%")
			.Replace("$", "\\$")
			.Replace("#", "\\#")
			.Replace("_", "\\_")
			.Replace("{", "\\{")
			.Replace("}", "\\}");

	private static string EscapeMonospace(string value) => value.Replace(" ", "~");
}

internal static class JsonOptions
{
	public static JsonSerializerOptions Default { get; } = new()
	{
		WriteIndented = true
	};
}

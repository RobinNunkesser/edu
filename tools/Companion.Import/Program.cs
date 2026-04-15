using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Content.Schema;

var arguments = CliArguments.Parse(args);
var companionRoot = Path.GetFullPath(arguments.CompanionRoot);
var outputPath = Path.GetFullPath(arguments.OutputPath);

var resourcesPath = Path.Combine(companionRoot, "ISDCompanion", "Resources", "Strings", "AppResources.de.resx");
var exercisesPath = Path.Combine(companionRoot, "ISDCompanion", "Tabs", "Exercises");

if (!File.Exists(resourcesPath))
{
	throw new FileNotFoundException($"Resource file not found: {resourcesPath}");
}

if (!Directory.Exists(exercisesPath))
{
	throw new DirectoryNotFoundException($"Exercises directory not found: {exercisesPath}");
}

var resources = ResourceReader.Read(resourcesPath);
var sourceFiles = new[]
{
	"AIExercisesPage.xaml",
	"ComputingSystemsExercisesPage.xaml",
	"NetworksExercisesPage.xaml",
	"OperatingSystemsExercisesPage.xaml"
};

var sections = sourceFiles
	.Select(fileName => Path.Combine(exercisesPath, fileName))
	.Where(File.Exists)
	.Select(path => XamlCatalogReader.Read(path, resources))
	.ToArray();

var document = new StudyCatalogDocument("ISD Companion", sections);

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
File.WriteAllText(
	outputPath,
	JsonSerializer.Serialize(document, JsonOptions.Default),
	new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

Console.WriteLine($"Wrote {sections.Length} sections to {outputPath}");

internal sealed record CliArguments(string CompanionRoot, string OutputPath)
{
	public static CliArguments Parse(string[] args)
	{
		string? companionRoot = null;
		string? outputPath = null;

		for (var index = 0; index < args.Length; index++)
		{
			switch (args[index])
			{
				case "--companion-root":
					companionRoot = GetValue(args, ++index, "--companion-root");
					break;
				case "--output":
					outputPath = GetValue(args, ++index, "--output");
					break;
			}
		}

		if (string.IsNullOrWhiteSpace(companionRoot) || string.IsNullOrWhiteSpace(outputPath))
		{
			throw new ArgumentException("Usage: --companion-root <path> --output <path>");
		}

		return new CliArguments(companionRoot, outputPath);
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

internal static class ResourceReader
{
	public static Dictionary<string, string> Read(string path)
	{
		var document = XDocument.Load(path);

		return document.Root!
			.Elements()
			.Where(element => element.Name.LocalName == "data")
			.Select(element => new
			{
				Name = element.Attribute("name")?.Value,
				Value = element.Elements().FirstOrDefault(child => child.Name.LocalName == "value")?.Value
			})
			.Where(item => !string.IsNullOrWhiteSpace(item.Name) && item.Value is not null)
			.ToDictionary(item => item.Name!, item => item.Value!.Trim());
	}
}

internal static class XamlCatalogReader
{
	private static readonly Regex ResourceKeyPattern = new(@"AppResources\.([A-Za-z0-9_]+)", RegexOptions.Compiled);
	private static readonly Regex TargetPagePattern = new("local:([A-Za-z0-9_]+)", RegexOptions.Compiled);

	public static StudySectionDocument Read(string path, IReadOnlyDictionary<string, string> resources)
	{
		var document = XDocument.Load(path);
		var contentPage = document.Root ?? throw new InvalidOperationException($"Missing root element in {path}");

		var title = ResolveLocalizedValue(contentPage.Attribute("Title")?.Value, resources, path);
		var sectionId = Slugify(title);
		var description = $"Importiert aus {Path.GetFileName(path)}.";

		var groups = contentPage
			.Descendants()
			.Where(element => element.Name.LocalName == "TableSection")
			.Select(section => new StudyGroupDocument(
				ResolveLocalizedValue(section.Attribute("Title")?.Value, resources, path),
				section.Elements()
					.Where(element => element.Name.LocalName == "TextCell")
					.Select(cell => CreateTopic(cell, resources, path))
					.ToArray()))
			.ToArray();

		return new StudySectionDocument(sectionId, title, description, groups);
	}

	private static StudyTopicDocument CreateTopic(XElement cell, IReadOnlyDictionary<string, string> resources, string path)
	{
		var title = ResolveLocalizedValue(cell.Attribute("Text")?.Value, resources, path);
		var targetPage = ResolveTargetPage(cell.Attribute("CommandParameter")?.Value, path);

		return new StudyTopicDocument(
			Slugify(title),
			title,
			$"Importierter Eintrag fuer {targetPage}.",
			"ISD Companion",
			true,
			targetPage);
	}

	private static string ResolveLocalizedValue(string? attributeValue, IReadOnlyDictionary<string, string> resources, string path)
	{
		if (string.IsNullOrWhiteSpace(attributeValue))
		{
			throw new InvalidOperationException($"Missing localized value in {path}");
		}

		var match = ResourceKeyPattern.Match(attributeValue);
		if (!match.Success)
		{
			return attributeValue.Trim();
		}

		var key = match.Groups[1].Value;
		if (!resources.TryGetValue(key, out var value))
		{
			throw new KeyNotFoundException($"Resource key '{key}' not found for {path}");
		}

		return value;
	}

	private static string ResolveTargetPage(string? attributeValue, string path)
	{
		if (string.IsNullOrWhiteSpace(attributeValue))
		{
			throw new InvalidOperationException($"Missing target page in {path}");
		}

		var match = TargetPagePattern.Match(attributeValue);
		if (!match.Success)
		{
			throw new InvalidOperationException($"Could not parse target page from '{attributeValue}' in {path}");
		}

		return match.Groups[1].Value;
	}

	private static string Slugify(string input)
	{
		var normalized = input
			.ToLowerInvariant()
			.Replace("ä", "ae")
			.Replace("ö", "oe")
			.Replace("ü", "ue")
			.Replace("ß", "ss");

		var builder = new StringBuilder();
		var previousDash = false;

		foreach (var character in normalized)
		{
			if (char.IsLetterOrDigit(character))
			{
				builder.Append(character);
				previousDash = false;
				continue;
			}

			if (previousDash)
			{
				continue;
			}

			builder.Append('-');
			previousDash = true;
		}

		return builder.ToString().Trim('-');
	}
}

internal static class JsonOptions
{
	public static JsonSerializerOptions Default { get; } = new()
	{
		WriteIndented = true
	};
}

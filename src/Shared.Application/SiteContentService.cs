using System.Reflection;
using System.Text.Json;
using Shared.Domain;

namespace Shared.Application;

public sealed class SiteContentService
{
    private const string LecturesWikiRepository = "https://github.com/isd-nunkesser/lectures.wiki/wiki";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Dictionary<SiteLanguage, SiteContentDocument> _cache = new();

    public async Task<UiTextViewModel> GetUiTextAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Ui;

    public async Task<SectionLandingViewModel> GetBookLandingAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Book;

    public async Task<IReadOnlyList<ResourceLinkViewModel>> GetBookLinksAsync(SiteLanguage language) => (await GetDocumentAsync(language)).BookLinks;

    public async Task<SectionLandingViewModel> GetTeachingLandingAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Teaching;

    public async Task<SectionLandingViewModel> GetResearchLandingAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Research;

    public async Task<SectionLandingViewModel> GetResearchPublicationsSectionAsync(SiteLanguage language) => (await GetDocumentAsync(language)).ResearchPublicationsSection;

    public async Task<IReadOnlyList<ResearchPublicationViewModel>> GetResearchPublicationsAsync(SiteLanguage language) => (await GetDocumentAsync(language)).ResearchPublications;

    public async Task<SectionLandingViewModel> GetSoftwareLandingAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Software;

    public async Task<IReadOnlyList<HomeAreaViewModel>> GetAreasAsync(SiteLanguage language)
    {
        var document = await GetDocumentAsync(language);

        return document.Areas
            .Select(area => new HomeAreaViewModel(area.Title, area.Summary, GetAreaRoute(language, area.Key), area.Highlights))
            .ToArray();
    }

    public async Task<IReadOnlyList<InfoCardViewModel>> GetSoftwareCardsAsync(SiteLanguage language) => (await GetDocumentAsync(language)).SoftwareCards;

    public async Task<IReadOnlyList<LectureOverviewViewModel>> GetTeachingLecturesAsync(SiteLanguage language)
    {
        var document = await GetDocumentAsync(language);

        return document.Lectures
            .Select(lecture => new LectureOverviewViewModel(
                lecture.Slug,
                lecture.Title,
                lecture.Semester,
                lecture.Summary,
                SiteRoutes.Lecture(language, lecture.Slug),
                lecture.HighlightTags))
            .ToArray();
    }

    public async Task<LectureDetailViewModel?> GetLectureBySlugAsync(SiteLanguage language, string slug)
    {
        var document = await GetDocumentAsync(language);
        var lecture = document.Lectures.FirstOrDefault(item => string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase));

        return lecture is null
            ? null
            : new LectureDetailViewModel(
                lecture.Slug,
                lecture.Title,
                lecture.Semester,
                lecture.Summary,
                $"{LecturesWikiRepository}/{lecture.WikiPath}",
                lecture.HighlightTags,
                lecture.Sections);
    }

    private async Task<SiteContentDocument> GetDocumentAsync(SiteLanguage language)
    {
        if (_cache.TryGetValue(language, out var cached))
        {
            return cached;
        }

        var resourceName = language == SiteLanguage.En
            ? "Shared.Application.Content.en.json"
            : "Shared.Application.Content.de.json";

        await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Could not load embedded site content resource {resourceName}.");

        var document = await JsonSerializer.DeserializeAsync<SiteContentDocument>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"Could not deserialize embedded site content resource {resourceName}.");

        _cache[language] = document;
        return document;
    }

    private static string GetAreaRoute(SiteLanguage language, string key) => key switch
    {
        "teaching" => SiteRoutes.Teaching(language),
        "research" => SiteRoutes.Research(language),
        "book" => SiteRoutes.Book(language),
        "software" => SiteRoutes.Software(language),
        _ => SiteRoutes.Home(language)
    };
}
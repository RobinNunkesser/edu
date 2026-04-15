using System.Net.Http.Json;
using Shared.Domain;

namespace Shared.Application;

public sealed class SiteContentService
{
    private const string LecturesWikiRepository = "https://github.com/isd-nunkesser/lectures.wiki/wiki";

    private readonly HttpClient _httpClient;
    private readonly Dictionary<SiteLanguage, SiteContentDocument> _cache = new();

    public SiteContentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<UiTextViewModel> GetUiTextAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Ui;

    public async Task<HomePageViewModel> GetHomePageAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Home;

    public async Task<SectionLandingViewModel> GetTeachingLandingAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Teaching;

    public async Task<SectionLandingViewModel> GetResearchLandingAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Research;

    public async Task<SectionLandingViewModel> GetSoftwareLandingAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Software;

    public async Task<IReadOnlyList<HomeAreaViewModel>> GetAreasAsync(SiteLanguage language)
    {
        var document = await GetDocumentAsync(language);

        return document.Areas
            .Select(area => new HomeAreaViewModel(area.Title, area.Summary, GetAreaRoute(language, area.Key), area.Highlights))
            .ToArray();
    }

    public async Task<IReadOnlyList<InfoCardViewModel>> GetResearchCardsAsync(SiteLanguage language) => (await GetDocumentAsync(language)).ResearchCards;

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

        var path = language == SiteLanguage.En ? "content/site/en.json" : "content/site/de.json";
        var document = await _httpClient.GetFromJsonAsync<SiteContentDocument>(path)
            ?? throw new InvalidOperationException($"Could not load site content from {path}.");

        _cache[language] = document;
        return document;
    }

    private static string GetAreaRoute(SiteLanguage language, string key) => key switch
    {
        "teaching" => SiteRoutes.Teaching(language),
        "research" => SiteRoutes.Research(language),
        "software" => SiteRoutes.Software(language),
        _ => SiteRoutes.Home(language)
    };
}
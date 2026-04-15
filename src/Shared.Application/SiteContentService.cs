using System.Reflection;
using System.Net.Http.Json;
using System.Text.Json;
using Shared.Domain;

namespace Shared.Application;

public sealed class SiteContentService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly Dictionary<SiteLanguage, SiteContentDocument> _cache = new();
    private readonly Dictionary<SiteLanguage, IReadOnlyList<TeachingLectureIndexItem>> _teachingLectureIndexCache = new();
    private readonly Dictionary<string, LectureDetailViewModel> _teachingLectureCache = new();

    public SiteContentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

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
        var lectures = await GetTeachingLectureIndexAsync(language);

        return lectures
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
        var index = await GetTeachingLectureIndexAsync(language);
        if (!index.Any(item => string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var cacheKey = $"{language}:{slug.ToLowerInvariant()}";
        if (_teachingLectureCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var lecture = await LoadTeachingLectureDetailAsync(language, slug);
        if (lecture is null)
        {
            return null;
        }

        _teachingLectureCache[cacheKey] = lecture;
        return lecture;
    }

    private async Task<IReadOnlyList<TeachingLectureIndexItem>> GetTeachingLectureIndexAsync(SiteLanguage language)
    {
        if (_teachingLectureIndexCache.TryGetValue(language, out var cached))
        {
            return cached;
        }

        foreach (var fileName in GetTeachingLectureIndexCandidates(language))
        {
            var response = await _httpClient.GetAsync($"content/site/{fileName}");
            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            var lectures = await response.Content.ReadFromJsonAsync<IReadOnlyList<TeachingLectureIndexItem>>(JsonOptions);
            if (lectures is not null)
            {
                _teachingLectureIndexCache[language] = lectures;
                return lectures;
            }
        }

        throw new InvalidOperationException("Could not load teaching lecture index.");
    }

    private async Task<LectureDetailViewModel?> LoadTeachingLectureDetailAsync(SiteLanguage language, string slug)
    {
        foreach (var fileName in GetTeachingLectureDetailCandidates(language, slug))
        {
            var response = await _httpClient.GetAsync($"content/site/{fileName}");
            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            var lecture = await response.Content.ReadFromJsonAsync<LectureDetailViewModel>(JsonOptions);
            if (lecture is not null)
            {
                return lecture;
            }
        }

        return null;
    }

    private static IReadOnlyList<string> GetTeachingLectureIndexCandidates(SiteLanguage language)
        => language == SiteLanguage.En
            ? ["teaching-lectures.en.json", "teaching-lectures.de.json"]
            : ["teaching-lectures.de.json"];

    private static IReadOnlyList<string> GetTeachingLectureDetailCandidates(SiteLanguage language, string slug)
        => language == SiteLanguage.En
            ? [$"lecture-template.{slug}.en.json", $"lecture-template.{slug}.de.json"]
            : [$"lecture-template.{slug}.de.json"];

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

    private sealed record TeachingLectureIndexItem(
        string Slug,
        string Title,
        string Semester,
        string Summary,
        IReadOnlyList<string> HighlightTags);
}
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
    private readonly bool _disableContentCache;
    private readonly Dictionary<SiteLanguage, SiteContentDocument> _cache = new();
    private readonly Dictionary<SiteLanguage, IReadOnlyList<ResearchPublicationViewModel>> _researchPublicationsCache = new();
    private readonly Dictionary<SiteLanguage, IReadOnlyList<TeachingLectureIndexItem>> _teachingLectureIndexCache = new();
    private readonly Dictionary<string, LectureDetailViewModel> _teachingLectureCache = new();

    public SiteContentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _disableContentCache = IsLocalDevelopment(httpClient.BaseAddress);
    }

    public async Task<UiTextViewModel> GetUiTextAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Ui;

    public async Task<SectionLandingViewModel> GetTeachingLandingAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Teaching;

    public async Task<SectionLandingViewModel> GetResearchLandingAsync(SiteLanguage language) => (await GetDocumentAsync(language)).Research;

    public async Task<SectionLandingViewModel> GetResearchPublicationsSectionAsync(SiteLanguage language) => (await GetDocumentAsync(language)).ResearchPublicationsSection;

    public async Task<IReadOnlyList<ResearchPublicationViewModel>> GetResearchPublicationsAsync(SiteLanguage language)
    {
        if (!_disableContentCache && _researchPublicationsCache.TryGetValue(language, out var cached))
        {
            return cached;
        }

        IReadOnlyList<ResearchPublicationViewModel> publications;

        if (_disableContentCache)
        {
            var fileName = GetResearchPublicationFileName(language);
            var response = await _httpClient.GetAsync($"content/site/{fileName}");
            response.EnsureSuccessStatusCode();

            publications = await response.Content.ReadFromJsonAsync<IReadOnlyList<ResearchPublicationViewModel>>(JsonOptions)
                ?? throw new InvalidOperationException($"Could not deserialize live research publications file {fileName}.");
        }
        else
        {
            var resourceName = GetResearchPublicationResourceName(language);

            await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Could not load embedded research publications resource {resourceName}.");

            publications = await JsonSerializer.DeserializeAsync<IReadOnlyList<ResearchPublicationViewModel>>(stream, JsonOptions)
                ?? throw new InvalidOperationException($"Could not deserialize embedded research publications resource {resourceName}.");
        }

        if (!_disableContentCache)
        {
            _researchPublicationsCache[language] = publications;
        }

        return publications;
    }

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
                SiteRoutes.Lecture(language, lecture.Slug)))
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
        if (!_disableContentCache && _teachingLectureCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var lecture = await LoadTeachingLectureDetailAsync(language, slug);
        if (lecture is null)
        {
            return null;
        }

        if (!_disableContentCache)
        {
            _teachingLectureCache[cacheKey] = lecture;
        }

        return lecture;
    }

    private async Task<IReadOnlyList<TeachingLectureIndexItem>> GetTeachingLectureIndexAsync(SiteLanguage language)
    {
        if (!_disableContentCache && _teachingLectureIndexCache.TryGetValue(language, out var cached))
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
                if (!_disableContentCache)
                {
                    _teachingLectureIndexCache[language] = lectures;
                }

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
        => ["teaching-lectures.de.json"];

    private static IReadOnlyList<string> GetTeachingLectureDetailCandidates(SiteLanguage language, string slug)
        => [$"lecture-template.{slug}.de.json"];

    private static string GetResearchPublicationFileName(SiteLanguage language)
        => language == SiteLanguage.En ? "research-publications.en.json" : "research-publications.de.json";

    private static string GetResearchPublicationResourceName(SiteLanguage language)
        => language == SiteLanguage.En
            ? "Shared.Application.Content.research-publications.en.json"
            : "Shared.Application.Content.research-publications.de.json";

    private async Task<SiteContentDocument> GetDocumentAsync(SiteLanguage language)
    {
        if (!_disableContentCache && _cache.TryGetValue(language, out var cached))
        {
            return cached;
        }

        if (_disableContentCache)
        {
            var fileName = language == SiteLanguage.En ? "en.json" : "de.json";
            var response = await _httpClient.GetAsync($"content/site/{fileName}");
            response.EnsureSuccessStatusCode();

            var liveDocument = await response.Content.ReadFromJsonAsync<SiteContentDocument>(JsonOptions)
                ?? throw new InvalidOperationException($"Could not deserialize live site content file {fileName}.");

            return liveDocument;
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

    private static bool IsLocalDevelopment(Uri? baseAddress)
    {
        if (baseAddress is null)
        {
            return false;
        }

        return string.Equals(baseAddress.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            || string.Equals(baseAddress.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetAreaRoute(SiteLanguage language, string key) => key switch
    {
        "teaching" => SiteRoutes.Teaching(language),
        "research" => SiteRoutes.Research(language),
        "software" => SiteRoutes.Software(language),
        _ => SiteRoutes.Home(language)
    };

    private sealed record TeachingLectureIndexItem(
        string Slug,
        string Title,
        string Semester);
}

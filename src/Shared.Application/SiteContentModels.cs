namespace Shared.Application;

public sealed record SiteContentDocument(
    UiTextViewModel Ui,
    HomePageViewModel Home,
    SectionLandingViewModel Book,
    SectionLandingViewModel Teaching,
    SectionLandingViewModel Research,
    SectionLandingViewModel Software,
    IReadOnlyList<HomeAreaDocument> Areas,
    IReadOnlyList<ResourceLinkViewModel> BookLinks,
    IReadOnlyList<InfoCardViewModel> ResearchCards,
    IReadOnlyList<InfoCardViewModel> SoftwareCards,
    IReadOnlyList<LectureDocument> Lectures);

public sealed record UiTextViewModel(
    string NavTeaching,
    string NavResearch,
    string NavBook,
    string NavSoftware,
    string HeaderPrototype,
    string LectureMetaLabel,
    string LectureSourceLabel,
    string OpenMaterialLabel,
    string InlineNote,
    string LectureNotFoundTitle,
    string LectureNotFoundText,
    string NotFoundTitle,
    string NotFoundText);

public sealed record HomePageViewModel(
    string HeroKicker,
    string HeroTitle,
    string HeroLead,
    string FocusLabel,
    string FocusTitle,
    string FocusText,
    string NavigationLabel,
    string NavigationTitle,
    string NavigationText,
    string NextLabel,
    string NextTitle,
    string NextText);

public sealed record SectionLandingViewModel(
    string Label,
    string Title,
    string Intro);

public sealed record HomeAreaDocument(
    string Key,
    string Title,
    string Summary,
    IReadOnlyList<string> Highlights);

public sealed record HomeAreaViewModel(
    string Title,
    string Summary,
    string Route,
    IReadOnlyList<string> Highlights);

public sealed record InfoCardViewModel(
    string Kicker,
    string Title,
    string Text);

public sealed record LectureDocument(
    string Slug,
    string Title,
    string Semester,
    string Summary,
    string WikiPath,
    IReadOnlyList<string> HighlightTags,
    IReadOnlyList<LectureSectionViewModel> Sections);

public sealed record LectureOverviewViewModel(
    string Slug,
    string Title,
    string Semester,
    string Summary,
    string Route,
    IReadOnlyList<string> HighlightTags);

public sealed record LectureDetailViewModel(
    string Slug,
    string Title,
    string Semester,
    string Summary,
    string SourceUrl,
    IReadOnlyList<string> HighlightTags,
    IReadOnlyList<LectureSectionViewModel> Sections);

public sealed record LectureSectionViewModel(
    string Title,
    string Intro,
    IReadOnlyList<ResourceLinkViewModel> Links);

public sealed record ResourceLinkViewModel(
    string Title,
    string Url,
    string Description);
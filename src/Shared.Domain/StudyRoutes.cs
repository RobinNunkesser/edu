namespace Shared.Domain;

public static class StudyRoutes
{
    public const string Home = "";
    public const string Study = "study";

    public static string Topic(string slug) => $"study/{slug}";

    public static string Topic(SiteLanguage language, string slug)
        => language == SiteLanguage.En ? $"en/study/{slug}" : Topic(slug);
}
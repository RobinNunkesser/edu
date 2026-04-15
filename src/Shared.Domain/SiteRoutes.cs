namespace Shared.Domain;

public enum SiteLanguage
{
    De,
    En
}

public static class SiteRoutes
{

    public static SiteLanguage GetLanguage(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return SiteLanguage.De;
        }

        var normalized = TrimQuery(relativePath).Trim('/');
        return normalized.Equals("en", StringComparison.OrdinalIgnoreCase) || normalized.StartsWith("en/", StringComparison.OrdinalIgnoreCase)
            ? SiteLanguage.En
            : SiteLanguage.De;
    }

    public static string Home(SiteLanguage language) => Teaching(language);

    public static string Teaching(SiteLanguage language) => language == SiteLanguage.En ? "/en/teaching" : "/lehre";

    public static string Research(SiteLanguage language) => language == SiteLanguage.En ? "/en/research" : "/forschung";

    public static string Book(SiteLanguage language) => language == SiteLanguage.En ? "/en/book" : "/buch";

    public static string Software(SiteLanguage language) => language == SiteLanguage.En ? "/en/software" : "/software";

    public static string Lecture(SiteLanguage language, string slug) => language == SiteLanguage.En ? $"/en/teaching/{slug}" : $"/lehre/{slug}";

    public static string SwitchLanguage(string? relativePath, SiteLanguage targetLanguage)
    {
        var normalized = TrimQuery(relativePath).Trim('/');

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Home(targetLanguage);
        }

        if (normalized.Equals("en", StringComparison.OrdinalIgnoreCase))
        {
            return Home(targetLanguage);
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var pathSegments = GetLanguage(normalized) == SiteLanguage.En ? segments.Skip(1).ToArray() : segments;

        if (pathSegments.Length == 0)
        {
            return Home(targetLanguage);
        }

        var head = pathSegments[0].ToLowerInvariant();

        if ((head == "lehre" || head == "teaching" || head == "study") && pathSegments.Length > 1)
        {
            return Lecture(targetLanguage, pathSegments[1]);
        }

        return head switch
        {
            "lehre" or "teaching" or "study" => Teaching(targetLanguage),
            "forschung" or "research" => Research(targetLanguage),
            "buch" or "book" => Book(targetLanguage),
            "software" => Software(targetLanguage),
            _ => Home(targetLanguage)
        };
    }

    private static string TrimQuery(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        var queryIndex = relativePath.IndexOfAny(new char[] { '?', '#' });
        return queryIndex >= 0 ? relativePath[..queryIndex] : relativePath;
    }
}
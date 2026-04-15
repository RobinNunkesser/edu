namespace Shared.Domain;

public static class StudyRoutes
{
    public const string Home = "/";
    public const string Study = "/study";

    public static string Topic(string slug) => $"/study/{slug}";
}
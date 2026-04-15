namespace Shared.Domain;

public static class SiteRoutes
{
    public const string Home = "/";
    public const string Teaching = "/lehre";
    public const string Research = "/forschung";
    public const string Software = "/software";

    public static string Lecture(string slug) => $"/lehre/{slug}";
}
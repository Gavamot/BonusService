namespace BonusService.Common;

public static class Extensions
{
    public static string GetUserName(this HttpContext httpContext)
    {
        return httpContext.User.Claims.FirstOrDefault(x => x.Type == "name")?.Value ?? "";
    }
}

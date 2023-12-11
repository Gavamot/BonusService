namespace BonusService.Common;

public static class Extensions
{
    public static string GetUserName(this HttpContext httpContext)
    {
        return httpContext.User.Claims.FirstOrDefault(x => x.Type == "name")?.Value ?? "";
    }

    public static DateTimeOffset ToDateTimeOffset(this DateOnly dateOnly)
    {
        return new DateTimeOffset(dateOnly.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }
}

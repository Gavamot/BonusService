namespace BonusService.Common;

public record DateTimeInterval(DateTimeOffset from, DateTimeOffset to);

public interface IDateTimeService
{
    DateTimeOffset GetNow();
    DateTimeInterval GetCurrentMonth();
    DateTimeOffset GetStartOfCurrentDay();
}

public class DateTimeService : IDateTimeService
{
    public DateTimeOffset GetNow() => DateTimeOffset.UtcNow;
    private readonly static TimeSpan MoscowTimezone = new (3, 0, 0);
    public DateTimeInterval GetCurrentMonth()
    {
        var cur = GetNow();
        var @from =  new DateTimeOffset(cur.Year, cur.Month, cur.Day, 0, 0, 0, cur.Offset);
        var @to = @from.AddMonths(1);
        return new DateTimeInterval(@from, to);
    }
    public DateTimeOffset GetStartOfCurrentDay()
    {
        var cur = GetNow();
        return new DateTimeOffset(cur.Year, cur.Month, cur.Day, 0, 0, 0, cur.Offset);
    }
}

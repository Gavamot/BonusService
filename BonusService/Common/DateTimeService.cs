namespace BonusService.Common;

public record DateTimeInterval(DateTimeOffset from, DateTimeOffset to);

public interface IDateTimeService
{
    DateTimeOffset GetNowUtc();
    DateTimeInterval GetCurrentMonth();
    DateTimeOffset GetStartOfCurrentDay();
}

public class DateTimeService : IDateTimeService
{
    public DateTimeOffset GetNowUtc() => DateTimeOffset.UtcNow;
    private readonly static TimeSpan MoscowTimezone = new (3, 0, 0);
    public DateTimeInterval GetCurrentMonth()
    {
        var cur = GetNowUtc();
        var @from =  new DateTimeOffset(cur.Year, cur.Month, 1, 0, 0, 0, cur.Offset);
        var @to = @from.AddMonths(1);
        return new DateTimeInterval(@from, to);
    }
    public DateTimeOffset GetStartOfCurrentDay()
    {
        var cur = GetNowUtc();
        return new DateTimeOffset(cur.Year, cur.Month, cur.Day, 0, 0, 0, cur.Offset);
    }
}

namespace BonusService.Common;

public interface IDateTimeService
{
    DateTimeOffset GetNowUtc();
}

public class DateTimeService : IDateTimeService
{
    public DateTimeOffset GetNowUtc() => DateTimeOffset.UtcNow;
    private readonly static TimeSpan MoscowTimezone = new (3, 0, 0);
}

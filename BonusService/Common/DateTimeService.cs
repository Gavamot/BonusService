namespace BonusService.Common;

public record DateTimeInterval(DateTime from, DateTime to);

public interface IDateTimeService
{
    DateTime GetNow();
    DateTimeInterval GetCurrentMonth();
}

public class DateTimeService : IDateTimeService
{
    public DateTime GetNow() => DateTime.Now;
    public DateTimeInterval GetCurrentMonth()
    {
        var cur = GetNow();
        var @from = new DateTime(cur.Year, cur.Month, cur.Day);
        var @to = new DateTime(cur.Year, cur.Month, DateTime.DaysInMonth(cur.Year, cur.Month), 23, 59, 59);
        return new DateTimeInterval(@from, to);
    }

}

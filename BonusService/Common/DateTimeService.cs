namespace BonusService.Common;

public interface IDateTimeService
{
    DateTime GetNow();
}

public class DateTimeService : IDateTimeService
{
    public DateTime GetNow() => DateTime.Now;
}

using BonusService.Common;
using BonusService.Common.Postgres.Entity;
namespace BonusService.Test;

public class DateTimeExtTest
{
    private DateTimeOffset date1 = new (2010, 2, 9, 1, 0, 0, TimeSpan.Zero);
    private DateTimeOffset date1StartOfMonth = new (2010, 2, 1, 0, 0, 0, TimeSpan.Zero);
    private DateTimeOffset date1StartOfWeek = new (2010, 2, 8, 0, 0, 0, TimeSpan.Zero);
    private DateTimeOffset date1StartOfDay = new (2010, 2, 9, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateWrongInterval_TrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new DateTimeExt(date1, date1.AddHours(-1));
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(23)]
    public void GetFromNowToFutureDateInterval_Moths(int frequency)
    {
        var interval = DateTimeExt.GetFromNowToFutureDateInterval(FrequencyTypes.Month, frequency, date1);
        interval.from.Should().Be(date1StartOfMonth);
        interval.to.Should().Be(date1StartOfMonth.AddMonths(frequency));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(23)]
    public void GetFromPrevMonthsToNowDateInterval_Moths(int frequency)
    {
        var interval = DateTimeExt.GetPrevToNowDateInterval(FrequencyTypes.Month, frequency, date1);
        interval.from.Should().Be(date1StartOfMonth.AddMonths(frequency * -1));
        interval.to.Should().Be(date1StartOfMonth);
    }


    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(23)]
    public void GetFromPrevToNowDateInterval_Days(int frequency)
    {

        var interval = DateTimeExt.GetPrevToNowDateInterval(FrequencyTypes.Day, frequency, date1);
        interval.from.Should().Be(date1StartOfDay.AddDays(frequency * -1));
        interval.to.Should().Be(date1StartOfDay);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(23)]
    public void  GetFromNowToFutureDateInterval_Days(int frequency)
    {

        var interval = DateTimeExt.GetFromNowToFutureDateInterval(FrequencyTypes.Day, frequency, date1);
        interval.from.Should().Be(date1StartOfDay);
        interval.to.Should().Be(date1StartOfDay.AddDays(frequency));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(23)]
    public void GetFromPrevToNowDateInterval_Weeks(int frequency)
    {

        var interval = DateTimeExt.GetPrevToNowDateInterval(FrequencyTypes.Week, frequency, date1);
        interval.from.Should().Be(date1StartOfWeek.AddDays(frequency * -7));
        interval.to.Should().Be(date1StartOfWeek);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(23)]
    public void  GetFromNowToFutureDateInterval_Weeks(int frequency)
    {

        var interval = DateTimeExt.GetFromNowToFutureDateInterval(FrequencyTypes.Week, frequency, date1);
        interval.from.Should().Be(date1StartOfWeek);
        interval.to.Should().Be(date1StartOfWeek.AddDays(frequency * 7));
    }
}

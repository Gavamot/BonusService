using BonusService.Common;
using BonusService.Common.Postgres.Entity;
using FakeItEasy;
using FluentAssertions;
namespace BonusService.Test;

public class DateIntervalTest
{
    private DateTimeOffset date1 = new (2010, 2, 3, 1, 0, 0, TimeSpan.Zero);
    private DateTimeOffset date1StartOfMonth = new (2010, 2, 1, 0, 0, 0, TimeSpan.Zero);
    [Fact]
    public void CreateWrongInterval_TrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new DateInterval(date1, date1.AddHours(-1));
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(23)]
    public void GetFromNowToFutureDateInterval_Moths(int frequency)
    {
        var interval = DateInterval.GetFromNowToFutureDateInterval(FrequencyTypes.Month, frequency, date1);
        interval.from.Should().Be(date1StartOfMonth);
        interval.to.Should().Be(date1StartOfMonth.AddMonths(frequency));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(23)]
    public void GetFromPrevMonthsToNowDateInterval_Moths(int frequency)
    {
        var interval = DateInterval.GetPrevToNowDateInterval(FrequencyTypes.Month, frequency, date1);
        interval.from.Should().Be(date1StartOfMonth.AddMonths(frequency * -1));
        interval.to.Should().Be(date1StartOfMonth);
    }
}

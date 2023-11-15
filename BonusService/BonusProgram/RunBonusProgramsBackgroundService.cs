using BonusService.Bonuses;
using Hangfire;

public class RunBonusProgramsBackgroundService : BackgroundService
{
    private readonly IBonusProgramRep _bonusProgramRep;
    private readonly IRecurringJobManagerV2 _scheduler;
    public RunBonusProgramsBackgroundService(
        IBonusProgramRep bonusProgramRep,
        IRecurringJobManagerV2 scheduler)
    {
        _bonusProgramRep = bonusProgramRep;
        _scheduler = scheduler;
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        var bp = _bonusProgramRep.Get();
        // https://crontab.guru/#0_9_1_*_*
        _scheduler.AddOrUpdate<MonthlySumBonusJob>("PeriodicalMonthlySumByLevels", x=> x.ExecuteAsync(), "0 9 1 * *");
        return Task.CompletedTask;
    }
}

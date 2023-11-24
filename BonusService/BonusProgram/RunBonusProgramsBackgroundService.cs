using BonusService.Bonuses;
using BonusService.Postgres;
using Hangfire;


public interface IBonusProgramsRunner
{
    public void Init();
    public void Update(BonusProgram bonusProgram);
}
public class BonusProgramsRunner : IBonusProgramsRunner
{
    private readonly IBonusProgramRep _bonusProgramRep;
    private readonly IRecurringJobManagerV2 _scheduler;
    public BonusProgramsRunner(
        IBonusProgramRep bonusProgramRep,
        IRecurringJobManagerV2 scheduler)
    {
        _bonusProgramRep = bonusProgramRep;
        _scheduler = scheduler;
    }

    private string GenerateJobId(int bonusProgramId, string name) => $"bonus_program_id{bonusProgramId}_{name}";

    public void Init()
    {
        var bp = _bonusProgramRep.Get();
        // https://crontab.guru/#0_9_1_*_*
        _scheduler.AddOrUpdate<MonthlySumBonusJob>(GenerateJobId(bp.Id, bp.Name), x=> x.ExecuteAsync(bp), bp.ExecutionCron);
    }
    public void Update(BonusProgram bonusProgram)
    {
        if (bonusProgram.IsDeleted)
        {
            _scheduler.RemoveIfExists(GenerateJobId(bonusProgram.Id, bonusProgram.Name));
        }
        else
        {
            // TODO Make something
        }

    }
}

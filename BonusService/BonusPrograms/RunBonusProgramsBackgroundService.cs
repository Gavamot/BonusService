using BonusService.BonusPrograms.SpendMoneyBonus;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Hangfire;
using Microsoft.EntityFrameworkCore;
namespace BonusService.BonusPrograms;

public interface IBonusProgramsRunner
{
    public void Init();
    public void Update(BonusProgram bonusProgram);
}
public class BonusProgramsRunner : IBonusProgramsRunner
{
    private readonly PostgresDbContext _postgres;
    private readonly IRecurringJobManagerV2 _scheduler;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<BonusProgramsRunner> _logger;
    public BonusProgramsRunner(
        PostgresDbContext postgres,
        IRecurringJobManagerV2 scheduler,
        IDateTimeService dateTimeService,
        ILogger<BonusProgramsRunner> logger)
    {

        _postgres = postgres;
        _dateTimeService = dateTimeService;
        _logger = logger;
        _scheduler = scheduler;
    }

    private string GenerateJobId(int bonusProgramId, string name) => $"bonus_program_id{bonusProgramId}_{name}";

    private void AddOrUpdateBonusProgram(BonusProgram bonusProgram)
    {
        string bonusProgramId = GenerateJobId(bonusProgram.Id, bonusProgram.Name);
        if (bonusProgram.BonusProgramType == BonusProgramType.SpendMoney)
        {
            _scheduler.AddOrUpdate<SpendMoneyBonusJob>(bonusProgramId, x => x.ExecuteAsync(bonusProgram.Id, _dateTimeService.GetNowUtc()), bonusProgram.ExecutionCron);
        }
        else
        {
            throw new NotImplementedException($"BonusProgramId = {bonusProgram.Id} Bonus program type {bonusProgram.BonusProgramType} not implemented yet");
        }
    }
    public void Init()
    {
        var now = _dateTimeService.GetNowUtc();
        var bonusPrograms = _postgres.GetActiveBonusPrograms(now).ToArray();
        foreach (var bonusProgram in bonusPrograms)
        {
            try
            {
                AddOrUpdateBonusProgram(bonusProgram);
            }
            catch(Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
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

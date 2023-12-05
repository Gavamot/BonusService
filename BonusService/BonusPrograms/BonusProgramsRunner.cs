using BonusService.BonusPrograms.SpendMoneyBonus;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Hangfire;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
namespace BonusService.BonusPrograms;

public interface IBonusProgramsRunner
{
    public Task RestartAsync();
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

    private const string jobIdPrefix = "bonus_program_";
    public string GenerateJobId(int bonusProgramId) => $"{jobIdPrefix}{bonusProgramId}";

    public async Task RestartAsync()
    {
        RemoveAll();
        await InitAsync();
    }
    public void RemoveAll()
    {
        using var con = JobStorage.Current.GetConnection();
        var jobs = con.GetRecurringJobs()
            .Where(x => x.Id.StartsWith(jobIdPrefix));
        foreach (var job in jobs)
        {
            _scheduler.RemoveIfExists(job.Id);
        }
    }
    public async Task InitAsync()
    {
       var bonusPrograms = await _postgres.GetBonusPrograms().ToArrayAsync();
       foreach (var bonusProgram in bonusPrograms)
       {
           Add(bonusProgram);
       }
    }

    private void Add(BonusProgram bonusProgram)
    {
        string jobId = GenerateJobId(bonusProgram.Id);
        if (bonusProgram.BonusProgramType == BonusProgramType.SpendMoney)
        {
            _scheduler.AddOrUpdate<SpendMoneyBonusJob>(jobId, x => x.ExecuteAsync(bonusProgram, _dateTimeService.GetNowUtc()), bonusProgram.ExecutionCron);
        }
        else
        {
            throw new NotImplementedException($"BonusProgramId = {bonusProgram.Id} Bonus program type {bonusProgram.BonusProgramType} not implemented yet");
        }
    }

    private void UpdateOrDelete(BonusProgram bonusProgram)
    {
        var jobId = GenerateJobId(bonusProgram.Id);
        _scheduler.RemoveIfExists(jobId); // Снимаем с исполнения в случаии удаления или изменения
        if (bonusProgram.IsDeleted) return;
        Add(bonusProgram);
    }
}

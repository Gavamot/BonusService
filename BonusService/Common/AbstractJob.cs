using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
namespace BonusService.Common;

public record BonusProgramJobResult(DateTimeInterval interval, int clientBalanceCount, long totalBonusSum);
public abstract class AbstractBonusProgramJob
{
    protected readonly ILogger _logger;
    protected readonly PostgresDbContext _postgres;
    protected readonly IDateTimeService _dateTimeService;
    public AbstractBonusProgramJob(ILogger logger, PostgresDbContext postgres, IDateTimeService dateTimeService)
    {
        _logger = logger;
        _postgres = postgres;
        _dateTimeService = dateTimeService;
    }

    protected abstract BonusProgramType BonusProgramType { get; }
    protected void Validate(BonusProgram bonusProgram, DateTimeOffset now)
    {
        try
        {
            if (bonusProgram.BonusProgramType != BonusProgramType.SpendMoney)
                throw new ArgumentException($"BonusProgram.Id = {bonusProgram.Id} Неверный тип бонусной программы начисляющей джобы. Должен быть {BonusProgramType.SpendMoney} а задан {bonusProgram.BonusProgramType}");
            if (bonusProgram.IsDeleted)
                throw new ArgumentException($"BonusProgram.Id = {bonusProgram.Id} попытка начислить бонусы по удаленной бонусной программе");
            if (bonusProgram.DateStart > now)
                throw new ArgumentException($"BonusProgram.Id = {bonusProgram.Id} Попытка запустить джобу раньше срока бонусной программы");
            if ((bonusProgram.DateStop ?? DateTimeOffset.MaxValue) < now)
                throw new ArgumentException($"BonusProgram.Id = {bonusProgram.Id} Попытка запустить джобу на завершенную бонусную программу");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    public async Task ExecuteAsync(BonusProgram bonusProgram, DateTimeOffset now)
    {
        Validate(bonusProgram, now);
        Stopwatch stopwatch = Stopwatch.StartNew();
        var bonusProgramMark = bonusProgram.CreateMark();
        using var activity = new Activity(bonusProgramMark);
        activity.Start();
        _logger.LogInformation("Job {bonusProgramMark} start", bonusProgramMark);
        try
        {
            var bonusProgramJobResult = await ExecuteJobAsync(bonusProgram, now);
            _logger.LogInformation("Job {bonusProgramMark} end", bonusProgramMark);
            stopwatch.Stop();
            var history = new BonusProgramHistory()
            {
                BonusProgramId = bonusProgram.Id,
                BankId = bonusProgram.BankId,
                ExecTimeStart = bonusProgramJobResult.interval.from,
                ExecTimeEnd = bonusProgramJobResult.interval.to,
                DurationMilliseconds = stopwatch.ElapsedMilliseconds,
                ClientBalancesCount = bonusProgramJobResult.clientBalanceCount,
                TotalBonusSum = bonusProgramJobResult.totalBonusSum,
                LastUpdated = _dateTimeService.GetNowUtc(),
            };

            await _postgres.BonusProgramHistory.AddAsync(history);
            await _postgres.SaveChangesAsync();
            var json = JsonSerializer.Serialize(history,new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
            _logger.LogInformation("Job {bonusProgramMark} status => {History}", bonusProgramMark, json);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "job { {bonusProgramMark} } execution error -> {Error}", bonusProgramMark, e.Message);
        }
        activity.Stop();
    }
    protected abstract Task<BonusProgramJobResult> ExecuteJobAsync(BonusProgram bonusProgram, DateTimeOffset now);
}

public abstract class AbstractJob : IJob
{
    protected readonly ILogger logger;
    public AbstractJob(ILogger logger)
    {
        this.logger = logger;
    }

    public abstract string Name { get; }

    public async Task ExecuteAsync(object parameter)
    {
        var jobContext = new JobLogContext(Name, Guid.NewGuid());
        ScopeContext.PushNestedState(jobContext);
        logger.LogInformation("Job start");
        try
        {
            await ExecuteJobAsync(parameter);
            logger.LogInformation("Job end");
        }
        catch (Exception e)
        {
            logger.LogError(e, "job execution error -> {Error}", e.Message);
        }
    }
    protected abstract Task ExecuteJobAsync(object parameter);
}

// https://blog.elmah.io/nlog-tutorial-the-essential-guide-for-logging-from-csharp/
public record JobLogContext(string JobName, Guid jobGuid);

public interface IJob
{
    string Name { get; }
    Task ExecuteAsync(object parameter);
}

public interface IJobRunParameter {}

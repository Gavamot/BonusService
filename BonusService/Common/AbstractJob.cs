using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Hangfire.Console;
using Hangfire.Server;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
namespace BonusService.Common;

public record BonusProgramJobResult(DateInterval interval, int clientBalanceCount, long totalBonusSum);
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
    protected void Validate(BonusProgram? bonusProgram, DateTimeOffset now)
    {
        try
        {
            if (bonusProgram == null)
                throw new ArgumentException($"BonusProgram.Id = {bonusProgram.Id} бонусной программы нет в бд");
            if (bonusProgram.IsDeleted)
                throw new ArgumentException($"BonusProgram.Id = {bonusProgram.Id} попытка начислить бонусы по удаленной бонусной программе");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }
    protected string GetBonusProgramMark(BonusProgram bonusProgram) => $"{bonusProgram.Id}_{bonusProgram.Name}";
    protected PerformContext ctx;
    public async Task ExecuteAsync(PerformContext ctx, BonusProgram bonusProgram, DateTimeOffset now)
    {
        this.ctx = ctx;
        Validate(bonusProgram, now);
        Stopwatch stopwatch = Stopwatch.StartNew();
        string bonusProgramMark = GetBonusProgramMark(bonusProgram);

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
            _logger.LogInformation("Job {BonusProgramMark} status => {History}", bonusProgramMark, json);
            ctx.WriteLine($"To db added BonusProgramHistory => {json}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "job {bonusProgramMark} execution error -> {Error}", bonusProgramMark, e.Message);
            activity.Stop();
            throw;
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
    protected PerformContext ctx;
    public async Task ExecuteAsync(PerformContext ctx, object parameter)
    {
        this.ctx = ctx;
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
    Task ExecuteAsync(PerformContext ctx, object parameter);
}

public interface IJobRunParameter {}

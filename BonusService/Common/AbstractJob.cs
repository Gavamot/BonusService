using System.Diagnostics;
using System.Text.Json;
using BonusService.Postgres;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
namespace BonusService.Common;

public record BonusProgramJobResult(int clientBalanceCount, long totalSum);
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

    public async Task ExecuteAsync(BonusProgram bonusProgram)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        var bonusProgramMark = bonusProgram.CreateMark();
        using var activity = new Activity(bonusProgramMark);
        activity.Start();
        _logger.LogInformation("Job {bonusProgramMark} start", bonusProgramMark);
        try
        {
            var bonusProgramJobResult = await ExecuteJobAsync(bonusProgram);
            _logger.LogInformation("Job {bonusProgramMark} end", bonusProgramMark);
            stopwatch.Stop();
            var interval = bonusProgram.CreateDateTimeInterval(_dateTimeService);
            var history = new BonusProgramHistory()
            {
                BonusProgramId = bonusProgram.Id,
                BankId = bonusProgram.BankId,
                ClientAccountsCount = bonusProgramJobResult.clientBalanceCount,
                TotalSum = bonusProgramJobResult.totalSum,
                StartPeriod = interval.from,
                EndPeriod = interval.to,
                DurationMilliseconds = stopwatch.ElapsedMilliseconds,
            };
            await _postgres.BonusProgramHistory.AddAsync(history);
            await _postgres.SaveChangesAsync();
            _logger.LogInformation("Job {bonusProgramMark} status => {History}", bonusProgramMark, JsonSerializer.Serialize(history));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "job { {bonusProgramMark} } execution error -> {Error}", bonusProgramMark, e.Message);
        }
        activity.Stop();
    }
    protected abstract Task<BonusProgramJobResult> ExecuteJobAsync(BonusProgram bonusProgram);
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

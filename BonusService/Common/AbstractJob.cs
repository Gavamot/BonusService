using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Hangfire.Console;
using Hangfire.Server;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
namespace BonusService.Common;

public record BonusProgramJobResult(Interval TimeExt, int clientBalanceCount, long totalBonusSum);
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public abstract class AbstractBonusProgramJob(ILogger logger, PostgresDbContext postgres, IDateTimeService dateTimeService)
{
    protected readonly ILogger _logger = logger;
    protected readonly PostgresDbContext _postgres = postgres;
    protected readonly IDateTimeService _dateTimeService = dateTimeService;
    protected abstract BonusProgramType BonusProgramType { get; }
    protected void Validate(BonusProgram? bonusProgram, Interval interval)
    {
        try
        {
            if (bonusProgram == null)
                throw new ArgumentException($"бонусной программы не задана");
            if (bonusProgram.IsDeleted)
                throw new ArgumentException($"BonusProgram.Id = {bonusProgram.Id} попытка начислить бонусы по удаленной бонусной программе");
            if (bonusProgram.IsActive(interval.from) == false)
                throw new AggregateException($"Bonus program is not active from {bonusProgram.DateStart} to {bonusProgram.DateStop} dateForCalculation={interval.from}");
            if (bonusProgram.BonusProgramType != BonusProgramType)
                throw new ArgumentException("Неверная связка в коде джобы и бонусной программы в бд. Обратитесь к разработчику сисетмы");
            if(bonusProgram.DateStart < interval.from)
                throw new ArgumentException($"bonusProgram.DateStart = {bonusProgram.DateStart} | interval = {interval}.Попытка начисления бонусных за неативный интервал бонусной программы Бонусная программа еще не активна. Поменяйте bonusProgram.DateStart на входящую в интервал дату или дождитесь начисления в дату входящую в интервал.");
            if(bonusProgram.DateStop != default && bonusProgram.DateStop < interval.to)
                throw new ArgumentException($"bonusProgram.DateEnd = {bonusProgram.DateStop} | interval = {interval}.Попытка начисления бонусных за неативный интервал бонусной программы Бонусная программа уже закончилась. Поменяйте bonusProgram.DateEnd на входящую в интервал дату или создайте новую.");
            if (bonusProgram.ProgramLevels == null || bonusProgram.ProgramLevels.Count <= 0)
                throw new ArgumentException("Бонусная программа не имеет уровней. Добавте хотябы 1 уровень для бонусной программы");
        }
        catch (Exception e)
        {
            _ctx.WriteLine(e.Message);
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogError(e, e.Message);
            throw;
        }
    }
    protected string GetBonusProgramMark(BonusProgram bonusProgram) => $"{bonusProgram.Id}_{bonusProgram.Name}";
    protected PerformContext _ctx = null!;
    public async Task ExecuteAsync(PerformContext ctx, BonusProgram bonusProgram, DateTimeOffset now)
    {
        var interval = Interval.GetPrevToNowDateInterval(bonusProgram.FrequencyType, bonusProgram.FrequencyValue, now);
        _ctx = ctx;
        Validate(bonusProgram, interval);
        Stopwatch stopwatch = Stopwatch.StartNew();
        string bonusProgramMark = GetBonusProgramMark(bonusProgram);

        using var activity = new Activity(bonusProgramMark);
        activity.Start();
        _logger.LogInformation("Job {BonusProgramMark} start for interval {Interval}", bonusProgramMark, interval);
        try
        {
            ctx.WriteLine($"Starting job execution for interval = {interval}. bonusProgram.Id = {bonusProgram.Id} , bonusProgram.BankId = {bonusProgram.BankId}, bonusProgram.BonusProgramType = {bonusProgram.BonusProgramType}");
            var bonusProgramJobResult = await ExecuteJobAsync(bonusProgram, interval, now);
            _logger.LogInformation("Job {BonusProgramMark} end for interval {Interval}", bonusProgramMark, interval);
            stopwatch.Stop();
            var history = new BonusProgramHistory()
            {
                BonusProgramId = bonusProgram.Id,
                BankId = bonusProgram.BankId,
                ExecTimeStart = bonusProgramJobResult.TimeExt.from,
                ExecTimeEnd = bonusProgramJobResult.TimeExt.to,
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
            _logger.LogInformation("Job {BonusProgramMark}  for {Interval} status => {History}", bonusProgramMark, interval, json);
            ctx.WriteLine($"To db added BonusProgramHistory => {json}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "job {BonusProgramMark} for {Interval} execution error -> {Error}", bonusProgramMark, interval, e.Message);
            activity.Stop();
            throw;
        }
        activity.Stop();
    }
    protected abstract Task<BonusProgramJobResult> ExecuteJobAsync(BonusProgram bonusProgram, Interval interval, DateTimeOffset now);
}

public abstract class AbstractJob : IJob
{
    protected readonly ILogger _logger;
    public AbstractJob(ILogger logger)
    {
        this._logger = logger;
    }

    public abstract string Name { get; }
    protected PerformContext _ctx = null!;
    public async Task ExecuteAsync(PerformContext ctx, object parameter)
    {
        this._ctx = ctx;
        var jobContext = new JobLogContext(Name, Guid.NewGuid());
        ScopeContext.PushNestedState(jobContext);
        _logger.LogInformation("Job start");
        try
        {
            await ExecuteJobAsync(parameter);
            _logger.LogInformation("Job end");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "job execution error -> {Error}", e.Message);
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

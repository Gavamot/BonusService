using BonusService.Common;
using BonusService.Postgres;

namespace BonusService.Bonuses;

/// <summary>
/// Начисление бонувов каждый календарный месяц(с 1 по последние число) по уровням от общей суммы затрат персоны
/// https://rnd.sitronics.com/jira/browse/EZSPLAT-244
/// </summary>
public class MonthlySumBonusJob : AbstractJob
{
    private readonly MongoDbContext _mongo;
    private readonly PostgresDbContext _postgres;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<MonthlySumBonusJob> _logger;

    public readonly ProgramTypes programType = ProgramTypes.PeriodicalMonthlySumByLevels;
    public override string Name => "[Бонусная программна по ежемесечной сумме затрап по уровням]";

    public MonthlySumBonusJob(MongoDbContext mongo,
        PostgresDbContext postgres,
        IDateTimeService dateTimeService,
        ILogger<MonthlySumBonusJob> logger) : base(logger)
    {
        _mongo = mongo;
        _postgres = postgres;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    protected override Task ExecuteJobAsync()
    {
        return Task.CompletedTask;
    }
}

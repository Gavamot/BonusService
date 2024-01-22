using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Microsoft.Extensions.DependencyInjection;
namespace BonusService.Test.Common;

#pragma warning disable CS1998
public static class BonusProgramSeed
{
    public static void AddPostgresSeed(IServiceProvider serviceProvider)
    {
        // Времянка пока юонусные программы захардкоженны
        using var scope1 = serviceProvider.CreateScope();
        var postgres = scope1.ServiceProvider.GetRequiredService<PostgresDbContext>();
        var bp = postgres.BonusPrograms.FirstOrDefault(x => x.Id == BonusTestApi.Q.BonusProgramId1);
        if (bp == null)
        {
            bp = BonusProgramSeed.Get();
            bp.Id = BonusTestApi.Q.BonusProgramId1;
            postgres.BonusPrograms.Add(bp);
            postgres.SaveChanges();
        }
    }
    public static  BonusProgram Get()
    {
        var lastUpdated = new DateTimeOffset(2023, 11, 1, 0, 0, 0, new TimeSpan(0));
        return new BonusProgram()
        {
            BankId = 1,
            Name = "Кэшбэк",
            Description = "Начиляется 1 числа каждого месяца изходя из общей суммы затрат за предыдущий месяц",
            BonusProgramType  = BonusProgramType.SpendMoney,
            DateStart = new DateTimeOffset(2023, 11, 1, 0,0, 0, TimeSpan.Zero),
            DateStop = DateTimeOffset.MaxValue,
            IsDeleted = false,
            ExecutionCron = "0 9 1 * *",
            LastUpdated = lastUpdated,
            FrequencyType = BonusService.Common.Postgres.Entity.FrequencyTypes.Month,
            FrequencyValue = 1,
            BonusProgramHistory = new List<BonusProgramHistory>(),
            ProgramLevels = new List<BonusProgramLevel>()
            {
                new ()
                {
                    LastUpdated = lastUpdated,
                    Name = "Нет кэшбэка",
                    AwardSum = 0,
                    AwardPercent = 0,
                    Condition = 0,
                    BonusProgramId = 1,
                    Level = 1,
                },
                new ()
                {
                    LastUpdated = lastUpdated,
                    Name = "Кэшбэк 1 уровня",
                    AwardSum = 0,
                    AwardPercent = 1,
                    Condition = 1_000_00,
                    BonusProgramId = 1,
                    Level = 2,
                },
                new ()
                {
                    LastUpdated = lastUpdated,
                    Name = "Кэшбэк 2 уровня",
                    AwardSum = 0,
                    AwardPercent = 5,
                    Condition = 3_000_00,
                    BonusProgramId = 1,
                    Level = 3,
                },
                new ()
                {
                    LastUpdated = lastUpdated,
                    Name = "Кэшбэк 3 уровня",
                    AwardSum = 0,
                    AwardPercent = 10,
                    Condition =5_000_00,
                    BonusProgramId = 1,
                    Level = 4,
                }
            }
        };
    }
}

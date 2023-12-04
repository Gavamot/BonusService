using BonusService.Common.Postgres.Entity;
namespace BonusService.BonusPrograms;

public static class BonusProgramSeed
{
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
            DateStop = null,
            IsDeleted = false,
            ExecutionCron = "0 9 1 * *",
            LastUpdated = lastUpdated,
            FrequencyType = FrequencyTypes.Month,
            FrequencyValue = 1,
            BonusProgramHistory = new List<BonusProgramHistory>(),
            ProgramLevels = new List<BonusProgramLevel>()
            {
                new ()
                {
                    LastUpdated = lastUpdated,
                    Name = "Нет кэшбэка",
                    AwardSum = 0,
                    Condition = 0,
                    BonusProgramId = 1,
                    Level = 1,
                },
                new ()
                {
                    LastUpdated = lastUpdated,
                    Name = "Кэшбэк 1 уровня",
                    AwardSum = 1,
                    Condition = 1_000_00,
                    BonusProgramId = 1,
                    Level = 2,
                },
                new ()
                {
                    LastUpdated = lastUpdated,
                    Name = "Кэшбэк 2 уровня",
                    AwardSum = 5,
                    Condition = 3_000_00,
                    BonusProgramId = 1,
                    Level = 3,
                },
                new ()
                {
                    LastUpdated = lastUpdated,
                    Name = "Кэшбэк 3 уровня",
                    AwardSum = 10,
                    Condition =5_000_00,
                    BonusProgramId = 1,
                    Level = 4,
                }
            }
        };
    }
}

using BonusService.Postgres;
using MongoDB.Driver.Core.WireProtocol.Messages;
namespace BonusService.Bonuses;

public interface IBonusProgramRep
{
    BonusProgram Get();
}

public class BonusProgramRep : IBonusProgramRep
{
    public BonusProgram Get()
    {
        return new BonusProgram()
        {
            Id = 1,
            Name = "Кэшбэк",
            Description = "Начиляется 1 числа каждого месяца изходя из общей суммы затрат за предыдущий месяц",
            ProgramTypes = ProgramTypes.PeriodicalMonthlySumByLevels,
            ActiveFrom = new DateTimeOffset(2023, 11, 1, 0,0, 0, TimeSpan.Zero),
            ActiveTo = null,
            IsDeleted = false,
            LastUpdated = new DateTime(2023, 11, 1),
            ProgramLevels = new List<BonusProgramLevel>()
            {
                new ()
                {
                    Id = 1,
                    LastUpdated = new DateTime(2023, 11, 1),
                    Name = "Нет кэшбэка",
                    Description = "Зарядился на сумму менее 1000 рублей за календарный месяц",
                    Benefit = 0,
                    Condition = 0,
                    ProgramId = 1,
                    Level = 1,
                },
                new ()
                {
                    Id = 2,
                    LastUpdated = new DateTime(2023, 11, 1),
                    Name = "Кэшбэк 1 уровня",
                    Description = "Зарядился на 1000 руб за календарный месяц",
                    Benefit = 1,
                    Condition = 1_000_00,
                    ProgramId = 1,
                    Level = 2,
                },
                new ()
                {
                    Id = 3,
                    LastUpdated = new DateTime(2023, 11, 1),
                    Name = "Кэшбэк 2 уровня",
                    Description = "Зарядился на 3000 руб за календарный месяц",
                    Benefit = 5,
                    Condition = 3_000_00,
                    ProgramId = 1,
                    Level = 3,
                },
                new ()
                {
                    Id = 4,
                    LastUpdated = new DateTime(2023, 11, 1),
                    Name = "Кэшбэк 3 уровня",
                    Description = "Зарядился на 5000 руб за календарный месяц",
                    Benefit = 10,
                    Condition =5_000_00,
                    ProgramId = 1,
                    Level = 4,
                }
            },
            BankId = new List<int>()
        };
    }
}

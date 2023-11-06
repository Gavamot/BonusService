using BonusService.Common;
using BonusService.Postgres;
namespace BonusService.Rep;

public interface IBonusProgramLevelsRep : ICatalogRep<BonusProgramLevel> { }

public class BonusProgramLevelsRep : CatalogGenericRep<BonusProgramLevel>, IBonusProgramLevelsRep
{
    public BonusProgramLevelsRep(PostgresDbContext db, IDateTimeService dateTimeService) : base(db, dateTimeService)
    {

    }
}

using BonusService.Common;
using BonusService.Postgres;
namespace BonusService.Rep;

public interface IBonusProgramRep : ICatalogRep<BonusProgram> { }

public class BonusProgramRep : CatalogGenericRep<BonusProgram>, IBonusProgramRep
{
    public BonusProgramRep(PostgresDbContext db, IDateTimeService dateTimeService) : base(db, dateTimeService)
    {

    }

    public IQueryable<BonusProgram> GetAll(CancellationToken cs)
    {
        return _db.BonusPrograms.Where(x => x.IsDeleted == false);
    }
}

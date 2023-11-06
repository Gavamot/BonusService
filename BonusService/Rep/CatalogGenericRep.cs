using BonusService.Common;
using BonusService.Postgres;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Rep;

public class CatalogGenericRep<T> : ICatalogRep<T> where T : class, ICatalogEntity
{
    protected readonly PostgresDbContext _db;
    protected  readonly IDateTimeService _dateTimeService;

    public CatalogGenericRep(PostgresDbContext db, IDateTimeService dateTimeService)
    {
        _db = db;
        _dateTimeService = dateTimeService;
    }

    public async virtual Task<T> AddAsync(T entity, CancellationToken cs)
    {
        entity.LastUpdated = _dateTimeService.GetNow();
        await _db.Set<T>().AddAsync(entity, cs).ConfigureAwait(false);
        await _db.SaveChangesAsync(cs).ConfigureAwait(false);
        return entity;
    }

    public async virtual Task<T> UpdateAsync(T entity, CancellationToken cs)
    {
        entity.LastUpdated = _dateTimeService.GetNow();
        _db.Set<T>().Update(entity);
        await _db.SaveChangesAsync(cs).ConfigureAwait(false);
        return entity;
    }

    public async virtual Task DeleteAsync(int id, CancellationToken cs)
    {
        var entity = await GetAsync(id, cs).ConfigureAwait(false);
        if(entity == null) return;

        if (entity is IDeletable dbEntity)
        {
            if(dbEntity.IsDeleted) return;
            dbEntity.IsDeleted = true;
            entity.LastUpdated = _dateTimeService.GetNow();
        }
        else
        {
            _db.Set<T>().Remove(entity);
        }
        await _db.SaveChangesAsync(cs).ConfigureAwait(false);
    }
    public async virtual Task<T?> GetAsync(int id, CancellationToken cs)
    {
        return await _db.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cs).ConfigureAwait(false);
    }

    public virtual IQueryable<T> GetAll()
    {
        return _db.Set<T>().AsNoTracking();
    }
}

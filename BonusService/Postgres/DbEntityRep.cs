using BonusService.Common;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Postgres;

public interface IDbEntityRep<T> where T : class, IHaveId<int>, IHaveDateOfChange
{
    Task<T> AddAsync(T entity, CancellationToken cs);
    Task<T> UpdateAsync(T entity, CancellationToken cs);
    Task DeleteAsync(int id, CancellationToken cs);
    Task<T?> GetAsync(int id, CancellationToken cs);
    IQueryable<T> GetAll();
}

public abstract class DbEntityRep<T> : IDbEntityRep<T> where T : class, IHaveId<int>, IHaveDateOfChange
{
    protected readonly PostgresDbContext _postgres;
    protected  readonly IDateTimeService _dateTimeService;

    protected DbEntityRep(PostgresDbContext postgres, IDateTimeService dateTimeService)
    {
        _postgres = postgres;
        _dateTimeService = dateTimeService;
    }
    public virtual async Task<T> AddAsync(T entity, CancellationToken cs)
    {
        entity.LastUpdated = _dateTimeService.GetNowUtc();
        await _postgres.Set<T>().AddAsync(entity, cs);
        await _postgres.SaveChangesAsync(cs);
        return entity;
    }
    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cs)
    {
        entity.LastUpdated = _dateTimeService.GetNowUtc();
        _postgres.Set<T>().Update(entity);
        await _postgres.SaveChangesAsync(cs);
        return entity;
    }
    public virtual async Task DeleteAsync(int id, CancellationToken cs)
    {
        var entity = await GetAsync(id, cs);
        if(entity == null) return;
        if (entity is IDeletable dbEntity)
        {
            if(dbEntity.IsDeleted) return;
            dbEntity.IsDeleted = true;
            entity.LastUpdated = _dateTimeService.GetNowUtc();
        }
        else
        {
            _postgres.Set<T>().Remove(entity);
        }
        await _postgres.SaveChangesAsync(cs);
    }
    public virtual async Task<T?> GetAsync(int id, CancellationToken cs)
    {
        return await _postgres.Set<T>().AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cs);
    }
    public virtual IQueryable<T> GetAll()
    {
        return _postgres.Set<T>().AsNoTracking();
    }
}

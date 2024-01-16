using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
namespace BonusService.Common.Postgres;

public interface IDbEntityRep<T>
    where T : class, IHaveDateOfChange
{
    Task<T> AddAsync(T entity, CancellationToken cs);
    Task<T> UpdateAsync(T entity, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken cs);
    Task<T?> GetAsync(int id, CancellationToken cs);
    IQueryable<T> GetAll();
}

public class CrudException : Exception
{

}

public class CrudNotFoundException : CrudException
{

}

public interface IUpdateMapper<in TDto, in TEntity>
    where TDto : CrudDto<TEntity>
    where TEntity: IHaveDateOfChange
{
    public void Map(TDto dto, TEntity entity);
}

public abstract class CrudDto <TEntity> : IHaveId<int> where TEntity : IHaveDateOfChange
{
    [Required]
    public int Id { get; set; }
}

public abstract class DbEntityRep<T> : IDbEntityRep<T>
    where T : class, IHaveId<int>, IHaveDateOfChange
{
    protected readonly PostgresDbContext _postgres;
    protected  readonly IDateTimeService _dateTimeService;

    protected DbEntityRep(PostgresDbContext postgres,
        IDateTimeService dateTimeService)
    {
        _postgres = postgres;
        _dateTimeService = dateTimeService;
    }
    public virtual async Task<T> AddAsync(T entity, CancellationToken cs)
    {
        entity.Id = default; // ! Id должно быть по дефолту чтобы двигалась последовательность
        entity.LastUpdated = _dateTimeService.GetNowUtc();
        await _postgres.Set<T>().AddAsync(entity, cs);
        await _postgres.SaveChangesAsync(cs);
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken ct)
    {
        if(entity.Id == 0 || (entity as IDeletable)?.IsDeleted == true)
        {
            throw new ArgumentException("Удаление через update не возможно воспользуйтесь операций Delete");
        }
        entity.LastUpdated = _dateTimeService.GetNowUtc();
        var res = _postgres.Update(entity);
        await _postgres.SaveChangesAsync(ct);
        return res.Entity;
    }

    public virtual async Task DeleteAsync(int id, CancellationToken cs)
    {
        var entity = await _postgres.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cs);
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
        var res = _postgres.Set<T>().AsNoTracking();
        if (typeof(T).GetInterface(nameof(IDeletable)) != null)
        {
            res  = ((IQueryable<IDeletable>)res).Where(x => x.IsDeleted == false).Cast<T>();
        }
        return res;
    }
}

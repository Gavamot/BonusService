using BonusService.Postgres;
namespace BonusService.Rep;

public interface ICatalogRep<T> where T : class, ICatalogEntity
{
    public Task<T> AddAsync(T entity, CancellationToken cs);
    public Task<T> UpdateAsync(T entity, CancellationToken cs);
    public Task DeleteAsync(int id, CancellationToken cs);
    public Task<T?> GetAsync(int id, CancellationToken cs);
    public IQueryable<T> GetAll(CancellationToken cs);
}

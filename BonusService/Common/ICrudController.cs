using System.ComponentModel.DataAnnotations;
using BonusService.Postgres;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Pay;

public interface ICrudController<T>
{
    Task<T> GetById(int id, CancellationToken ct);
    Task<T []> GetAll(CancellationToken ct);
    Task<T> Add(T entity, CancellationToken ct);
    Task<T> Update(T entity, CancellationToken ct);
    Task DeleteById(int id, CancellationToken ct);
}

public abstract class CrudController<T> : ControllerBase, ICrudController<T> where T : class, IHaveId<int>, IHaveDateOfChange
{
    protected readonly IDbEntityRep<T> _rep;
    protected CrudController(IDbEntityRep<T> rep)
    {
        _rep = rep;
    }

    [HttpGet("{id}")]
    public virtual async Task<T> GetById([FromRoute][Required]int id, CancellationToken ct)
    {
        return await _rep.GetAsync(id, ct);
    }

    [HttpGet]
    public virtual async Task<T[]> GetAll(CancellationToken ct)
    {
        return await _rep.GetAll().ToArrayAsync(ct);
    }

    [HttpPut]
    public virtual async Task<T> Add([Required]T entity, CancellationToken ct)
    {
        return await _rep.AddAsync(entity, ct);
    }

    [HttpPost]
    public virtual async Task<T> Update([Required]T entity, CancellationToken ct)
    {
        return await _rep.UpdateAsync(entity, ct);
    }

    [HttpDelete("{id}")]
    public virtual async Task DeleteById([FromRoute][Required]int id, CancellationToken ct)
    {
        await _rep.DeleteAsync(id, ct);
    }
}

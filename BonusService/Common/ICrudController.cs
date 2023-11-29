using System.ComponentModel.DataAnnotations;
using BonusService.Common.Postgres;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Common;

public interface ICrudController<T>
{
    Task<T?> GetById(int id, CancellationToken ct);
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

    [HttpGet("{id:int}")]
    public async Task<T?> GetById([FromRoute][Required]int id, CancellationToken ct)
    {
        return await _rep.GetAsync(id, ct);
    }

    [HttpGet]
    public async Task<T[]> GetAll(CancellationToken ct)
    {
        return await _rep.GetAll().ToArrayAsync(ct);
    }

    [HttpPut]
    public async Task<T> Add([Required]T entity, CancellationToken ct)
    {
        Response.StatusCode = StatusCodes.Status200OK;
        return await _rep.AddAsync(entity, ct);
    }

    [HttpPost]
    public async Task<T> Update([Required]T entity, CancellationToken ct)
    {
        return await _rep.UpdateAsync(entity, ct);
    }

    [HttpDelete("{id:int}")]
    public async Task DeleteById([FromRoute][Required]int id, CancellationToken ct)
    {
        await _rep.DeleteAsync(id, ct);
    }
}

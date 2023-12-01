using System.ComponentModel.DataAnnotations;
using BonusService.Common.Postgres;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Common;

public interface ICrudController<TEntity>
    where TEntity : class, IHaveId<int>, IHaveDateOfChange
{
    Task<TEntity?> GetById(int id, CancellationToken ct);
    Task GetAll(CancellationToken ct);
    Task<TEntity> Add(TEntity entity, CancellationToken ct);
    //Task<TEntity> Update(TDto entity, CancellationToken ct);
    Task DeleteById(int id, CancellationToken ct);
}


public abstract class CrudController<TEntity> : ControllerBase, ICrudController<TEntity>
    where TEntity : class, IHaveId<int>, IHaveDateOfChange
{
    protected readonly IDbEntityRep<TEntity> _rep;
    protected CrudController(IDbEntityRep<TEntity> rep)
    {
        _rep = rep;
    }

    [HttpGet("{id:int}")]
    public async Task<TEntity?> GetById([FromRoute][Required]int id, CancellationToken ct)
    {
        return await _rep.GetAsync(id, ct);
    }
    Task ICrudController<TEntity>.GetAll(CancellationToken ct) => GetAll(ct);

    [HttpGet]
    public async Task<TEntity[]> GetAll(CancellationToken ct)
    {
        return await _rep.GetAll().ToArrayAsync(ct);
    }

    [HttpPost]
    public async Task<TEntity> Add([Required]TEntity entity, CancellationToken ct)
    {
        Response.StatusCode = StatusCodes.Status200OK;
        return await _rep.AddAsync(entity, ct);
    }

    /*[HttpPatch]
    public async Task<TEntity> Update([Required]TDto entity, CancellationToken ct)
    {

    }*/

    /*[HttpPut]
    public async Task<T> Update([Required]T entity, CancellationToken ct)
    {
        return await _rep.UpdateAsync(entity, ct);
    }*/


    [HttpDelete("{id:int}")]
    public async Task DeleteById([FromRoute][Required]int id, CancellationToken ct)
    {
        await _rep.DeleteAsync(id, ct);
    }
}

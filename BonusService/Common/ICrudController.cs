using System.ComponentModel.DataAnnotations;
using BonusService.Auth.Policy;
using BonusService.Common.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Common;

public interface ICrudController<TEntity, TDto>
    where TEntity : class, IHaveId<int>, IHaveDateOfChange
    where TDto : CrudDto<TEntity>
{
    Task<TEntity?> GetById(int id, CancellationToken ct);
    Task<TEntity[]> GetAll(CancellationToken ct);
    Task<TEntity> Add(TEntity entity, CancellationToken ct);
    //Task Update([Required]TDto dto, CancellationToken ct);
    Task DeleteById(int id, CancellationToken ct);
}


public abstract class CrudController<TEntity, TDto> : ControllerBase, ICrudController<TEntity, TDto>
    where TEntity : class, IHaveId<int>, IHaveDateOfChange
    where TDto : CrudDto<TEntity>
{
    protected readonly IDbEntityRep<TEntity> _rep;
    private readonly IUpdateMapper<TDto, TEntity> _mapper;
    protected CrudController(IDbEntityRep<TEntity> rep, IUpdateMapper<TDto, TEntity> mapper)
    {
        _rep = rep;
        _mapper = mapper;
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PolicyNames.BonusServiceRead)]
    public async Task<TEntity?> GetById([FromRoute][Required]int id, CancellationToken ct)
    {
        return await _rep.GetAsync(id, ct);
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.BonusServiceRead)]
    public async Task<TEntity[]> GetAll(CancellationToken ct)
    {
        return await _rep.GetAll().ToArrayAsync(ct);
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.BonusServiceWrite)]
    public async Task<TEntity> Add([Required]TEntity entity, CancellationToken ct)
    {
        return await _rep.AddAsync(entity, ct);
    }

    [HttpPatch]
    [Authorize(Policy = PolicyNames.BonusServiceWrite)]
    public async Task Update([Required]TDto dto, CancellationToken ct)
    {
        var entity = await _rep.GetAsync(dto.Id, ct);
        if (entity == null || (entity as IDeletable)?.IsDeleted == true) throw new CrudNotFoundException();
        _mapper.Map(dto, entity);
        await _rep.UpdateAsync(entity, ct);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.BonusServiceWrite)]
    public async Task DeleteById([FromRoute][Required]int id, CancellationToken ct)
    {
        await _rep.DeleteAsync(id, ct);
    }
}

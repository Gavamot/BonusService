using System.ComponentModel.DataAnnotations;
using BonusService.Auth.Policy;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

// ReSharper disable once CheckNamespace
namespace BonusService.Balance.OwnerByPayCrud;

public sealed class OwnerByPayValidator : AbstractValidator<OwnerMaxBonusPay>
{
    public OwnerByPayValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.MaxBonusPayPercentages).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
    }
}

public class OwnerByPayDto : CrudDto<OwnerMaxBonusPay>
{
    public int? OwnerId { get; set; }
    public int? MaxBonusPayPercentages { get; set; }
}

[Mapper(AllowNullPropertyAssignment = false)]
public partial class OwnerByPayMapper : IUpdateMapper<OwnerByPayDto, OwnerMaxBonusPay>
{
    public partial void Map(OwnerByPayDto dto, OwnerMaxBonusPay entity);
}

public class OwnerByPayRep : DbEntityRep<OwnerMaxBonusPay>
{
    public OwnerByPayRep(BonusDbContext bonus, IDateTimeService dateTimeService)
        : base(bonus, dateTimeService)
    {

    }
}

/// <summary>
/// Справочник пользователей
/// </summary>
[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public sealed class OwnerMaxBonusPayController : ICrudController<OwnerMaxBonusPay, OwnerByPayDto>
{
    private readonly OwnerByPayRep _rep;
    private readonly BonusDbContext _bonus;
    private readonly OwnerByPayMapper _mapper = new ();
    public OwnerMaxBonusPayController(OwnerByPayRep rep, BonusDbContext bonus)
    {
        _rep = rep;
        _bonus = bonus;

    }
    [HttpGet("{id:int}")]
    [Authorize(Policy = PolicyNames.OwnerRead)]
    public async Task<OwnerMaxBonusPay?> GetById([FromRoute][Required]int id, CancellationToken ct)
    {
        return await _rep.GetAsync(id, ct);
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.OwnerRead)]
    public async Task<OwnerMaxBonusPay[]> GetAll(CancellationToken ct)
    {
        return await _rep.GetAll().ToArrayAsync(ct);
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.OwnerWrite)]
    public async Task<OwnerMaxBonusPay> Add([Required]OwnerMaxBonusPay entity, CancellationToken ct)
    {
        return await _rep.AddAsync(entity, ct);
    }

    [HttpPatch]
    [Authorize(Policy = PolicyNames.OwnerWrite)]
    public async Task Update([Required]OwnerByPayDto dto, CancellationToken ct)
    {
        var entity = await _rep.GetAsync(dto.Id, ct);
        if (entity == null) throw new CrudNotFoundException();
        _mapper.Map(dto, entity);
        await _rep.UpdateAsync(entity, ct);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PolicyNames.OwnerWrite)]
    public async Task DeleteById([FromRoute][Required]int id, CancellationToken ct)
    {
        await _rep.DeleteAsync(id, ct);
    }

    [HttpGet("{ownerId:int}")]
    [Authorize(Policy = PolicyNames.OwnerRead)]
    public async Task<OwnerMaxBonusPay?> GetByOwnerId([FromRoute][Required]int ownerId, CancellationToken ct)
    {
        return await _bonus.OwnerMaxBonusPays.FirstOrDefaultAsync(x=> x.OwnerId == ownerId, ct);
    }
}

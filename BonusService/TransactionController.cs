using BonusService.Auth.Policy;
using BonusService.Common.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
namespace BonusService;

//[Authorize]
public class TransactionController : ODataController
{
    private readonly PostgresDbContext _db;
    public TransactionController(PostgresDbContext db)
    {
        _db = db;

    }

    /// <summary>
    /// Поддержка OData
    /// По умолчанию выдается 15 записей. Одним запросом лимит 100 записей
    /// https://devblogs.microsoft.com/odata/aggregation-extensions-in-odata-asp-net-core/
    /// Документация https://learn.microsoft.com/ru-ru/azure/search/search-query-odata-comparison-operators
    /// Примеры :
    /// Агрегация {{baseUrl}}/api/transaction?$apply=aggregate($count as OrderCount, BonusSum with sum  as TotalAmount)
    /// Пользователь Admin
    /// http://localhost:9100/api/transaction?$filter=userName eq % 'Admin'
    /// Начисления бонусов
    /// http://localhost:9100/api/transaction?$filter=bonusSum ge 0
    /// Списание бонусов
    /// http://localhost:9100/api/transaction?$filter=bonusSum lt 0
    /// http://localhost:9100/api/transaction?$filter=bonusSum ge 1000 and bonusProgramId eq 2
    /// </summary>
    [Authorize(Policy = PolicyNames.BonusServiceExecute)]
    [HttpGet("api/transaction")]
    [EnableQuery(PageSize = 15)]
    public IActionResult Get()
    {
        return Ok(_db.Transactions.AsQueryable());
    }

    /*[HttpGet]
    [EnableQuery]
    public SingleResult<Transaction> Get([FromODataUri] long key)
    {
        var result = _db.Transactions.Where(c => c.Id == key);
        return SingleResult.Create(result);
    }

    [HttpPost]
    [EnableQuery]
    public async Task<IActionResult> Post([FromBody] Transaction note)
    {
        _db.Transactions.Add(note);
        await _db.SaveChangesAsync();
        return Created(note);
    }

    [HttpPatch]
    [EnableQuery]
    public async Task<IActionResult> Patch([FromODataUri] long key, Delta<Transaction> note)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var existingNote = await _db.Transactions.FindAsync(key);
        if (existingNote == null)
        {
            return NotFound();
        }

        note.Patch(existingNote);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!NoteExists(key))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return Updated(existingNote);
    }

    [HttpDelete]
    [EnableQuery]
    public async Task<IActionResult> Delete([FromODataUri] long key)
    {
        var existingNote = await _db.Transactions.FindAsync(key);
        if (existingNote == null)
        {
            return NotFound();
        }

        _db.Transactions.Remove(existingNote);
        await _db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status204NoContent);
    }

    private bool NoteExists(long key)
    {
        return _db.Transactions.Any(p => p.Id == key);
    }*/
}

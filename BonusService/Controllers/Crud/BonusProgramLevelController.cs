using BonusService.Postgres;
using BonusService.Rep;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Controllers.Crud;

[ApiController]
[Route("/api/[controller]")]
public class BonusProgramLevelController : ControllerBase
{
    private readonly IBonusProgramLevelsRep _rep;
    public BonusProgramLevelController(IBonusProgramLevelsRep rep)
    {
        _rep = rep;
    }

    [HttpGet]
    public async Task<BaseResponse<BonusProgramLevel[]>> GetAll(CancellationToken cs)
    {
        var res = await _rep.GetAll().ToArrayAsync(cs);
        return new BaseResponse<BonusProgramLevel[]>(res);
    }

    [HttpGet("{id}")]
    public async Task<BaseResponse<BonusProgramLevel>> GetById(int id, CancellationToken cs)
    {
        var res = await _rep.GetAsync(id, cs);
        return new BaseResponse<BonusProgramLevel>(res);
    }

    [HttpPost]
    public async Task<BaseResponse<BonusProgramLevel>> Add(BonusProgramLevel model, CancellationToken cs)
    {
        var res = await _rep.AddAsync(model, cs);
        return new BaseResponse<BonusProgramLevel>(res);
    }

    [HttpPut]
    public async Task<BaseResponse<BonusProgramLevel>> Update(BonusProgramLevel model, CancellationToken cs)
    {
        var res = await _rep.UpdateAsync(model, cs);
        return new BaseResponse<BonusProgramLevel>(res);
    }

    [HttpDelete("{id}")]
    public async Task<BaseResponseEmpty> Delete(int id, CancellationToken cs)
    {
        await _rep.DeleteAsync(id, cs);
        return new BaseResponseEmpty();
    }
}

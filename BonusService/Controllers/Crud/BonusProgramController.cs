using BonusService.Postgres;
using BonusService.Rep;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Controllers.Crud;

///https://restfulapi.net/http-status-codes/
[ApiController]
[Route("/api/[controller]")]
public class BonusProgramController : ControllerBase
{
    private readonly IBonusProgramRep _rep;
    public BonusProgramController(IBonusProgramRep rep)
    {
        _rep = rep;
    }

    [HttpGet]
    public async Task<BaseResponse<BonusProgram[]>> GetAll(CancellationToken cs)
    {
        var res = await _rep.GetAll(cs).ToArrayAsync(cs);
        return new BaseResponse<BonusProgram[]>(res);
    }

    [HttpGet("{id}")]
    public async Task<BaseResponse<BonusProgram>> GetById(int id, CancellationToken cs)
    {
        var res = await _rep.GetAsync(id, cs);
        return new BaseResponse<BonusProgram>(res);
    }

    [HttpPost]
    public async Task<BaseResponse<BonusProgram>> Add(BonusProgram model, CancellationToken cs)
    {
        var res = await _rep.AddAsync(model, cs);
        return new BaseResponse<BonusProgram>(res);
    }

    [HttpPut]
    public async Task<BaseResponse<BonusProgram>> Update(BonusProgram model, CancellationToken cs)
    {
        var res = await _rep.UpdateAsync(model, cs);
        return new BaseResponse<BonusProgram>(res);
    }

    [HttpDelete("{id}")]
    public async Task<BaseResponseEmpty> Delete(int id, CancellationToken cs)
    {
        await _rep.DeleteAsync(id, cs);
        return new BaseResponseEmpty();
    }
}

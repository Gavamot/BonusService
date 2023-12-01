using System.ComponentModel.DataAnnotations;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BonusService.BonusPrograms
{
    [ApiController]
    [Authorize]
    [Route("/[controller]/[action]")]
    public sealed partial class BonusLevelsController
    {
        private readonly PostgresDbContext _db;
        public BonusLevelsController(PostgresDbContext db)
        {
            _db = db;

        }

        /*[HttpPatch()]
        public async Task<> GetById([FromRoute][Required]int id, CancellationToken ct)
        {
            return await _rep.GetAsync(id, ct);
        }*/


    }
}

// ReSharper disable once CheckNamespace
namespace BonusService.BonusPrograms.Crud
{
    [ApiController]
    [Authorize]
    [Route("/[controller]/[action]")]
    public sealed partial class BonusLevelsController : CrudController<BonusProgramLevel>
    {
        public BonusLevelsController(IDbEntityRep<BonusProgramLevel> rep) : base(rep)
        {

        }

    }
}

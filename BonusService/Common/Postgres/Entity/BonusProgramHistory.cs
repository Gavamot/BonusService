using BonusService.Common.Swagger;
namespace BonusService.Common.Postgres.Entity;

#pragma warning disable CS8618
public class BonusProgramHistory : IHaveId<int>, IHaveDateOfChange
{
    public int Id { get; set; }
    [SwaggerExclude]
    public BonusProgram BonusProgram { get; set; }
    public int BonusProgramId { get; set; }
    public DateTimeOffset ExecTimeStart { get; set; }
    public DateTimeOffset ExecTimeEnd { get; set; }
    public int BankId { get; set; }
    public long TotalBonusSum { get; set; }
    public int ClientBalancesCount { get; set; }
    public long DurationMilliseconds { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}

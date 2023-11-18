using System.ComponentModel.DataAnnotations.Schema;
namespace BonusService.Postgres;

#pragma warning disable CS8618
public class BonusProgramHistory : IHaveId<long>
{
    public long Id { get; set; }
    public BonusProgram BonusProgram { get; set; }
    public int BonusProgramId { get; set; }
    public DateTimeOffset ExecTimeStart { get; set; }
    public DateTimeOffset ExecTimeEnd { get; set; }
    [Column(TypeName = "jsonb")]
    public BonusProgramHistoryInfo Info { get; set; }
}

public class BonusProgramHistoryInfo
{
    public int BankId { get; set; }
    public long TotalSum { get; set; }
    public int ClientCount { get; set; }
}

#nullable enable
using System.ComponentModel.DataAnnotations.Schema;
namespace BonusService.Postgres;

public class ProgramLevel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public long Sum { get; set; }
    public int CashbackPercentage { get; set; }
    public int BonusProgramId { get; set; }
    public virtual Program Program { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime LastUpdated { get; set; }
    [Column(TypeName = "jsonb")]
    public string? Options { get; set; }
}

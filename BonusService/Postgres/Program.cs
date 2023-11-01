#nullable enable
using System.ComponentModel.DataAnnotations.Schema;
namespace BonusService.Postgres;

public class Program
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ProgramTypes ProgramTypes { get; set; }
    public string Description { get; set; }
    public DateOnly ActiveFrom { get; set; }
    public DateOnly? ActiveTo { get; set; }
    public virtual List<Transaction> ProgramLevels { get; set; } = new ();
    public bool IsDeleted { get; set; }
    public DateTime LastUpdated { get; set; }
    [Column(TypeName = "jsonb")]
    public string? Options { get; set; }
}

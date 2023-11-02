#nullable enable

#pragma warning disable CS8618
namespace BonusService.Postgres;

public class Program : ICatalogEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ProgramTypes ProgramTypes { get; set; }
    public string Description { get; set; } = "";
    public DateOnly ActiveFrom { get; set; }
    public DateOnly? ActiveTo { get; set; }
    public virtual List<Transaction> ProgramLevels { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTime LastUpdated { get; set; }
}

#nullable enable

#pragma warning disable CS8618
namespace BonusService.Postgres;

public class BonusProgram : ICatalogEntity, IDeletable
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ProgramTypes ProgramTypes { get; set; }
    public string Description { get; set; } = "";
    public DateTimeOffset ActiveFrom { get; set; }
    public DateTimeOffset? ActiveTo { get; set; }
    public virtual List<BonusProgramLevel> ProgramLevels { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    /// <summary>
    /// Если валюта пуста то для всех валют елси нет то для указанных в массиве
    /// </summary>
    public List<int> BankId { get; set; } = new();
}

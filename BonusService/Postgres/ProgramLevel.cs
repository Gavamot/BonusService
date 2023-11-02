#pragma warning disable CS8618
#nullable enable
namespace BonusService.Postgres;

public class ProgramLevel : ICatalogEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime LastUpdated { get; set; }

    public int Level { get; set; }

    public int ProgramId { get; set; }
    public virtual Program Program { get; set; }
    /// <summary>
    /// Условие срабатывания программы например общая сумма в рублях
    /// </summary>
    public long Condition { get; set; }
    /// <summary>
    /// Получаемая выгода при срабатывании например в процентах или бонусах
    /// </summary>

    public int Benefit { get; set; }
}

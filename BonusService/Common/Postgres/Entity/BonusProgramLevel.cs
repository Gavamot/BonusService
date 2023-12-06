using System.Text.Json.Serialization;
using BonusService.Common.Swagger;
using Swashbuckle.AspNetCore.Annotations;
#pragma warning disable CS8618
#nullable enable
namespace BonusService.Common.Postgres.Entity;

public class BonusProgramLevel : ICatalogEntity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateTimeOffset LastUpdated { get; set; }

    public int Level { get; set; }

    public int BonusProgramId { get; set; }

    [JsonIgnore]
    [SwaggerSchema(ReadOnly = true)]
    [SwaggerExclude]
    public BonusProgram? BonusProgram { get; set; } = null;
    /// <summary>
    /// Условие срабатывания программы например общая сумма в рублях
    /// </summary>
    public long Condition { get; set; }
    /// <summary>
    /// Получаемая выгода при срабатывании например в процентах или бонусах
    /// </summary>

    public int AwardPercent  { get; set; }
    public int AwardSum { get; set; }
}

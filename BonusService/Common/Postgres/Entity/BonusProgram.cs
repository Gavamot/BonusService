#nullable enable

using System.Text.Json.Serialization;
using BonusService.Common.Swagger;
using Swashbuckle.AspNetCore.Annotations;
#pragma warning disable CS8618
namespace BonusService.Common.Postgres.Entity;

public enum FrequencyTypes
{
    /// <summary>
    /// С 1 числа месяца - поледниего числа месяца (включительно)
    /// </summary>
    Month,
    /// <summary>
    /// С Понедельника по Воскресенье (включительно)
    /// </summary>
    Week,
    /// <summary>
    /// Каждый календарный день
    /// </summary>
    Day,
}

public enum BonusProgramType
{
    /// <summary>
    /// заряжено Х кВт
    /// </summary>
    ChargedByCapacity,
    /// <summary>
    /// зарядка на Х разных станциях
    /// </summary>
    ChargedByStations,
    /// <summary>
    /// потрачено Х денег
    /// </summary>
    SpendMoney,
    /// <summary>
    /// день рождения
    /// </summary>
    Birthday
}

public class BonusProgram : ICatalogEntity, IDeletable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public BonusProgramType BonusProgramType { get; set; }
    public string Description { get; set; }
    public DateTimeOffset DateStart { get; set; }
    public DateTimeOffset? DateStop { get; set; }
    public int BankId { get; set; }
    public string ExecutionCron { get; set; }
    public FrequencyTypes FrequencyType { get; set; }
    public int FrequencyValue { get; set; }
    public DateTimeOffset LastUpdated { get; set; }

    [JsonIgnore]
    [SwaggerSchema(ReadOnly = true)]
    [SwaggerExclude]
    public bool IsDeleted { get; set; }

    [JsonIgnore]
    [SwaggerSchema(ReadOnly = true, Nullable = true)]
    [SwaggerExclude]
    public List<BonusProgramLevel> ProgramLevels { get; set; } = new List<BonusProgramLevel>();
    [JsonIgnore]
    [SwaggerSchema(ReadOnly = true, Nullable = true)]
    [SwaggerExclude]
    public List<BonusProgramHistory> BonusProgramHistory { get; set; } = new List<BonusProgramHistory>();
}

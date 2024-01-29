namespace BonusService.Common.Postgres.Entity;

public enum EventTypes
{
    NewUserRegistration,
    FirstPayedSession,
    UserDataCommitted
}

public class EventReward : IForeignCatalogEntity, IHaveActivePeriod
{
    public int Id { get; set; }
    public EventTypes Type { get; set; }
    public int Reward { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public int BankId { get; set; }
    public DateTimeOffset DateStart { get; set; }
    public DateTimeOffset DateStop { get; set; }
}

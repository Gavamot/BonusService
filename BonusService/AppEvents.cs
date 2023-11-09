namespace BonusService.Bonuses;

public static class AppEvents
{
    public readonly static  EventId FiscalizeBalanceRegisterJobEvent = new (1, nameof(FiscalizeBalanceRegisterJob));
}

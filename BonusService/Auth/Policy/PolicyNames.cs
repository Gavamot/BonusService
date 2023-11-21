namespace BonusService.Auth.Policy;

public static class PolicyNames
{
    public const string AccrualManualRead = "AccrualManualRead";
    public const string AccrualManualWrite = "AccrualManualWrite";
    public const string AccrualManualExecute = "AccrualManualExcecute";

    public const string BalanceRead = "BalanceRead";
    public const string BalanceWrite = "BalanceWrite";
    public const string BalanceExecute = "BalanceExecute";

    public const string BonusProgramRead = "BonusProgramRead";
    public const string BonusProgramWrite = "BonusProgramWrite";
    public const string BonusProgramExecute = "BonusProgramExecute";

    public const string OwnerMaxBonusPayRead = "OwnerMaxBonusPayRead";
    public const string OwnerMaxBonusPayWrite = "OwnerMaxBonusPayWrite";
    public const string OwnerMaxBonusPayExecute = "OwnerMaxBonusPayExecute";

    public const string PayRead = "PayRead";
    public const string PayWrite = "PayWrite";
    public const string PayExecute = "PayExecute";

    public const string PayManualRead = "PayManualRead";
    public const string PayManualWrite = "PayManualWrite";
    public const string PayManualExecute = "PayManualExecute";

    public const string GetBonusProgramAchievementRead = nameof(GetBonusProgramAchievementRead);

}

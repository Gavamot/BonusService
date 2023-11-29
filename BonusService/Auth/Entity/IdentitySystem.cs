namespace BonusService.Auth.Entity;

public class IdentitySystem
{
    public string Name { set; get; } = null!;
    public string Value { set; get; } = null!;
    public string UserNameLastUpdated { get; set; } = null!;
    public DateTime LastUpdated { get; set; }
}
public class IdentitySystemNames
{
    public const string SoleJwtKey = "SoleJwtKey";
}

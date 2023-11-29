#pragma warning disable CS8618
namespace BonusService.Auth.Settings;

public class IdentitySettings
{
    public string ConnectionString { init; get; }
    public TokenSettings TokenSettings { init; get; }
}
public record TokenSettings
(
    string Issuer,
    string SecretKey
);


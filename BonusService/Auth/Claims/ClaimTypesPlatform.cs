using System.Security.Claims;
namespace BonusService.Auth.Claims;

public static class ClaimTypesPlatform
{
    public const string Read = "Read";
    public const string ReadWrite = "ReadWrite";
    public const string ReadExecute = "ReadExecute";
    public const string Write = "Write";
    public const string WriteExecute = "WriteExecute";
    public const string Execute = "Execute";
    public const string ReadWriteExecute = "ReadWriteExecute";
    public const string Roles = "Roles";
    public const string Groups = "Groups";
    public const string IsRefreshToken = "IsRefreshToken";
    public const string IsAllCpRights = "IsAllCpRights";
}
public static class ClaimsTypeHelper
{
    public static bool Read(Claim claim) => new[]
    {
        ClaimTypesPlatform.Read,
        ClaimTypesPlatform.ReadWrite,
        ClaimTypesPlatform.ReadWriteExecute,
        ClaimTypesPlatform.ReadExecute
    }.Contains(claim.Type);

    public static bool Write(Claim claim) => new[]
    {
        ClaimTypesPlatform.Write,
        ClaimTypesPlatform.ReadWrite,
        ClaimTypesPlatform.ReadWriteExecute,
        ClaimTypesPlatform.WriteExecute
    }.Contains(claim.Type);

    public static bool Execute(Claim claim) => new[]
    {
        ClaimTypesPlatform.Execute,
        ClaimTypesPlatform.ReadExecute,
        ClaimTypesPlatform.WriteExecute,
        ClaimTypesPlatform.ReadWriteExecute
    }.Contains(claim.Type);
}

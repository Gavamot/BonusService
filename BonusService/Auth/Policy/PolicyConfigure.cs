using Microsoft.AspNetCore.Authorization;
using PlatformWebApi.Identity.Claims;
using PlatformWebApi.Identity.Rights;
using PlatformWebApi.Identity.Roles;

namespace BonusService.Auth.Policy;

public static class PolicyConfigure
{

    public static void AccrualManualRead(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Read(claim) &&
                claim.Value == ControllerNames.AccrualManual) ||
            context.User.IsInRole(RolesPlatform.Admin));
    public static void AccrualManualWrite(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Write(claim) &&
                claim.Value == ControllerNames.AccrualManual) ||
            context.User.IsInRole(RolesPlatform.Admin));
    public static void AccrualManualExecute(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Execute(claim) &&
                claim.Value == ControllerNames.AccrualManual) ||
            context.User.IsInRole(RolesPlatform.Admin));

    public static void BalanceRead(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Read(claim) &&
                claim.Value == ControllerNames.Balance) ||
            context.User.IsInRole(RolesPlatform.Admin));
    public static void BalanceWrite(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Write(claim) &&
                claim.Value == ControllerNames.Balance) ||
            context.User.IsInRole(RolesPlatform.Admin));
    public static void BonusProgramRead(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Read(claim) &&
                claim.Value == ControllerNames.BonusProgram) ||
            context.User.IsInRole(RolesPlatform.Admin));
    public static void BonusProgramWrite(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Write(claim) &&
                claim.Value == ControllerNames.BonusProgram) ||
            context.User.IsInRole(RolesPlatform.Admin));

    public static void OwnerMaxBonusPayRead(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Read(claim) &&
                claim.Value == ControllerNames.OwnerMaxBonusPay) ||
            context.User.IsInRole(RolesPlatform.Admin));
    public static void OwnerMaxBonusPayWrite(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Write(claim) &&
                claim.Value == ControllerNames.OwnerMaxBonusPay) ||
            context.User.IsInRole(RolesPlatform.Admin));

    public static void PayRead(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Read(claim) &&
                claim.Value == ControllerNames.Pay) ||
            context.User.IsInRole(RolesPlatform.Admin));
    public static void PayWrite(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Write(claim) &&
                claim.Value == ControllerNames.Pay) ||
            context.User.IsInRole(RolesPlatform.Admin));

    public static void PayWriteExecute(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Execute(claim) &&
                claim.Value == ControllerNames.Pay) ||
            context.User.IsInRole(RolesPlatform.Admin));

    public static void PayManualRead(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Read(claim) &&
                claim.Value == ControllerNames.PayManual) ||
            context.User.IsInRole(RolesPlatform.Admin));

    public static void PayManualWrite(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Write(claim) &&
                claim.Value == ControllerNames.PayManual) ||
            context.User.IsInRole(RolesPlatform.Admin));
    public static void PayManualExecute(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Execute(claim) &&
                claim.Value == ControllerNames.PayManual) ||
            context.User.IsInRole(RolesPlatform.Admin));

    public static void BonusProgramAchievementExecute(AuthorizationPolicyBuilder policy)
        => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                ClaimsTypeHelper.Execute(claim) &&
                claim.Value == ControllerNames.BonusProgramAchievement) ||
            context.User.IsInRole(RolesPlatform.Admin));
}

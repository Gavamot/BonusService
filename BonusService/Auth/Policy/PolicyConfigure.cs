using System.Net;
using System.Security.Claims;
using Amazon.Runtime.Internal.Transform;
using BonusService.Auth.Claims;
using BonusService.Auth.Roles;
using Microsoft.AspNetCore.Authorization;

namespace BonusService.Auth.Policy;

public static class PolicyNames
{
    public const string BonusServiceRead = nameof(BonusServiceRead);
    public const string BonusServiceWrite = nameof(BonusServiceWrite);
    public const string BonusServiceExecute = nameof(BonusServiceExecute);
    public const string PersonRead = nameof(PersonRead);
    public const string OwnerRead = nameof(OwnerRead);
    public const string OwnerWrite = nameof(OwnerWrite);
}

public static class PolicyConfigure
{
    private readonly static Dictionary<string, Action<AuthorizationPolicyBuilder>> ALl = new()
    {
        new (PolicyNames.PersonRead, Read(ClaimsNames.Person)),
        new (PolicyNames.BonusServiceRead, Read(ClaimsNames.BonusService)),
        new (PolicyNames.BonusServiceWrite, Write(ClaimsNames.BonusService)),
        new (PolicyNames.BonusServiceExecute, Execute(ClaimsNames.BonusService)),
        new (PolicyNames.OwnerRead, Read(ClaimsNames.Owner)),
        new (PolicyNames.OwnerWrite, Write(ClaimsNames.Owner)),
    };

    public static class ClaimsNames
    {
        public const string BonusService = nameof(BonusService);
        public const string Person = nameof(Person);
        public const string Owner = nameof(Owner);
    }

    public static void AddBonusServicePolices(AuthorizationOptions options)
    {
        foreach (var policy in ALl)
        {
            options.AddPolicy(policy.Key, policy.Value);
        }
    }

    private static Action<AuthorizationPolicyBuilder> GetAction(string claimValue, Func<Claim, bool> checkClaim)
    {
        return policy => policy.RequireAssertion(context =>
            context.User.HasClaim(claim =>
                checkClaim.Invoke(claim) && claim.Value == claimValue) ||
            context.User.IsInRole(RolesPlatform.Admin));
    }
    private static Action<AuthorizationPolicyBuilder> Read(string claimValue) => GetAction(claimValue, ClaimsTypeHelper.Read);
    private static Action<AuthorizationPolicyBuilder> Write(string claimValue)=> GetAction(claimValue, ClaimsTypeHelper.Write);
    private static Action<AuthorizationPolicyBuilder> Execute(string claimValue)=> GetAction(claimValue, ClaimsTypeHelper.Execute);
}

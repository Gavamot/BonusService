using System.Text;
using BonusService.Auth.Policy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace BonusService.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddAndAuthorization(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(opt =>
                {
                    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                }
            )
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["IdentitySettings:TokenSettings:Issuer"],
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeyResolver = (token, secutiryToken, kid, validationParameters) =>
                        {
                            //для того чтобы на каждый запрос снова устанавливался ключ
                            SecurityKey issuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(JwtKeyProvider.Key));
                            return new List<SecurityKey>
                            {
                                issuerSigningKey
                            };
                        },
                        RoleClaimType = "Roles",
                        NameClaimType = "Name"
                    };
                });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy =
                new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            
            options.AddPolicy(PolicyNames.AccrualManualRead, PolicyConfigure.AccrualManualRead);
            options.AddPolicy(PolicyNames.AccrualManualWrite, PolicyConfigure.AccrualManualWrite);
            
            options.AddPolicy(PolicyNames.BalanceRead, PolicyConfigure.BalanceRead);
            options.AddPolicy(PolicyNames.BalanceWrite, PolicyConfigure.BalanceWrite);
            
            options.AddPolicy(PolicyNames.BonusProgramRead, PolicyConfigure.BonusProgramRead);
            options.AddPolicy(PolicyNames.BonusProgramWrite, PolicyConfigure.BonusProgramWrite);
            
            options.AddPolicy(PolicyNames.OwnerMaxBonusPayRead, PolicyConfigure.OwnerMaxBonusPayRead);
            options.AddPolicy(PolicyNames.OwnerMaxBonusPayWrite, PolicyConfigure.OwnerMaxBonusPayWrite);
            
            options.AddPolicy(PolicyNames.PayManualRead, PolicyConfigure.PayManualRead);
            options.AddPolicy(PolicyNames.PayManualWrite, PolicyConfigure.PayManualWrite);
            
            options.AddPolicy(PolicyNames.PayRead, PolicyConfigure.PayRead);
            options.AddPolicy(PolicyNames.PayWrite, PolicyConfigure.PayWrite);

        });
        return services;
    }
    

}
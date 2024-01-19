using System.Configuration;
using System.Text;
using BonusService.Auth.DbContext;
using BonusService.Auth.Entity;
using BonusService.Auth.Policy;
using BonusService.Auth.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BonusService.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuthorization(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<IdentitySettings>(configuration.GetSection(nameof(IdentitySettings)));
        services.AddDbContext<IdentityPlatformDbContext>();

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
                        NameClaimType = "LevelName"
                    };
                });
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy =
                new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();

            PolicyConfigure.AddBonusServicePolices(options);

        });

        return services;
    }

    public static void UseJwtAuthorization(this IApplicationBuilder app)
    {
        if(Program.IsNswagBuild()) return;
        using var scope = app.ApplicationServices.CreateScope();
        var identitySettings = scope.ServiceProvider.GetRequiredService<IOptions<IdentitySettings>>();
        var jwtKeyFromSettings = identitySettings.Value.TokenSettings.SecretKey;
        if (jwtKeyFromSettings.IsNullOrEmpty())
        {
            Console.WriteLine("Error JwtKeyGenerator init(). jwtKeyFromSettings Is Null Or Empty!");
            Console.WriteLine("Выход из приложения exit(1), bonus сервер не запущен!");
            Environment.Exit(1);
        }

        string salt = GetSalt(app);
        JwtKeyProvider.SetJwtSecretKey(jwtKeyFromSettings, salt);

        app.UseAuthentication();
        app.UseAuthorization();
    }

    private static string GetSalt(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContextIdentity = scope.ServiceProvider.GetRequiredService<IdentityPlatformDbContext>();
        if (Program.IsLocal() || Program.IsAppTest())
        {
           return "qB)4(KCIzvSQ4u7jX8s";
        }
        else
        {
            var identitySystemSole = dbContextIdentity.IdentitySystem.FirstOrDefault(x => x.Name == IdentitySystemNames.SoleJwtKey);
            if (identitySystemSole == null)
                throw new Exception("Error: IdentitySystemSole is null. Running autogen sole");
            return identitySystemSole.Value;
        }
    }

}

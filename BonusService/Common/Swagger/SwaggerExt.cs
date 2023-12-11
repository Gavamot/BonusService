using System.Reflection;
using BonusService.Common.Swagger;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using SwaggerExampleAttrLib;
using Swashbuckle.AspNetCore.Filters;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
namespace BonusService.Common;

public static class SwaggerExt
{
    public static void UseAppSwagger(this WebApplication app)
    {
        if (Program.IsAppTest()) return;
        app.UseSwagger(c =>
        {

        });
        app.UseSwaggerUI(c=>
        {
            c.SwaggerEndpoint("v1/swagger.json", "Bonus API V1");
            c.EnableTryItOutByDefault();
            c.DisplayRequestDuration();
        });
    }

    public static void AddAppSwagger(this IServiceCollection services)
    {
        if (Program.IsAppTest()) return;
        // API geteway настроен криво поэтому нужно добавлять префиксы чтобы с внешнего адреса можно было вызывать свагер методы через API Gateway
        // Но для локального окружения и для генеренного клиента это ненужно так как наши сервисы могут обращатся в контейнер напрямую миную API Geteway
        var isInternal = Program.IsLocal() || Program.IsNswagBuild();
        services.AddDateOnlyTimeOnlyStringConverters();
        services.AddFluentValidationRulesToSwagger();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(
            c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                c.SchemaFilter<SwaggerExcludeFilter>();

                c.UseDateOnlyTimeOnlyStringConverters();
                if (isInternal == false)
                {
                    c.DocumentFilter<PathPrefixInsertDocumentFilter>("/api/bonus");
                }

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlCommentsWithRemarks(xmlPath);
                c.SchemaFilter<EnumTypesSchemaFilter>(xmlPath);
                c.SchemaFilter<ExampleSchemaFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                //c.OperationFilter<SecurityRequirementsOperationFilter>(xmlPath);

                c.AddSecurityDefinition(name: "Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme, Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });
    }
}

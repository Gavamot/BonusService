using System.Configuration;
using System.Text.Json.Serialization;
using BonusService.Auth;
using BonusService.Bonuses;
using BonusService.Common;
using BonusService.Pay;
using BonusService.Postgres;
using Correlate.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using NLog.Web;
using PlatformWebApi.Identity.Settings;

Console.WriteLine($"Environment = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var urls= configuration.GetSection("Urls").Value;
Console.WriteLine($"Running address is urls={urls}");

var services = builder.Services;

services.AddCorrelate(options => options.RequestHeaders = new []{ "X-Correlation-ID" });

services.AddScoped<IBonusProgramRep, BonusProgramRep>();
services.AddScoped<OwnerByPayRep>();
builder.Logging.ClearProviders();
builder.Host.UseNLog();

services.TryAddSingleton<IDateTimeService, DateTimeService>();
services.AddPostgres(configuration);
services.AddMongoService(configuration);
services.AddHangfireService(configuration);

services.AddFluentValidationAutoValidation();
services.AddFluentValidationClientsideAdapters();
services.AddValidatorsFromAssemblyContaining<Program>();
services.AddFluentValidationRulesToSwagger();

services.AddControllers().AddJsonOptions(opt=>
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

/*
if (IsNswagBuild())
{
    services.AddOpenApiDocument();
}*/

//auth
services.Configure<IdentitySettings>(configuration.GetSection(nameof(IdentitySettings)));
services.AddJwtAuthorization(configuration);


services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

services.AddSwagger();

services.AddMediator(opt =>
{
    opt.ServiceLifetime = ServiceLifetime.Scoped;
});

WebApplication app = builder.Build();

/*if (IsNswagBuild())
{
    app.UseOpenApi();
    app.UseSwaggerUi3();
}*/
app.Services.AuthInitJwtJey();

app.UseSwagger();
app.UseSwaggerUI(c=>
{
    c.SwaggerEndpoint("v1/swagger.json", "My API V1");
    c.EnableTryItOutByDefault();
});
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.ApplyPostgresMigrations();

app.Run();

public partial class Program
{
    public const string AppTest = nameof(AppTest);
    public static bool IsAppTest() => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(AppTest)) == false;
    public static bool IsNswagBuild() => string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NswagGen")) == false;
}

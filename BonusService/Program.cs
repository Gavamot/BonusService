using System.Text.Json.Serialization;
using BonusService.Common;
using BonusService.Postgres;
using Correlate.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using MongoDB.Driver.Linq;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

services.AddCorrelate(options => options.RequestHeaders = new []{ "X-Correlation-ID" });

builder.Logging.ClearProviders();
builder.Host.UseNLog();

services.TryAddTransient<IDateTimeService, DateTimeService>();
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

if (IsNswagBuild())
{
    //services.AddOpenApiDocument();
}


services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
services.AddSwaggerGen(c=> c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" }));

services.AddMediator(opt =>
{
    opt.ServiceLifetime = ServiceLifetime.Scoped;
});


var app = builder.Build();

if (IsNswagBuild())
{
    //app.UseOpenApi();
    //app.UseSwaggerUi3();
}


app.UseSwagger();
app.UseSwaggerUI(c=> c.SwaggerEndpoint("v1/swagger.json", "My API V1"));

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.ApplyPostgresMigrations();


var s = app.Services.CreateScope();
var _postgres = s.ServiceProvider.GetRequiredService<PostgresDbContext>();

var a = _postgres.Transactions.Where(x => x.LastUpdated < DateTimeOffset.MinValue && x.BonusSum > 0)
    .GroupBy(x => new { x.PersonId, x.BankId })
    .Select(x => new { x.Key.PersonId, x.Key.BankId, Sum = x.Sum(y => y.BonusSum) })
    .ToQueryString();
var b = 1;
/*
var maxFiscalDate = await _postgres.BalanceRegister.MaxAsync(x => (DateTimeOffset?)x.Date) ?? DateTimeOffset.MinValue;
var fiscalizedBalanceQuery = _postgres.BalanceRegister.Where(x=> x.Date == maxFiscalDate);
var fiscalizedBalance = await fiscalizedBalanceQuery.AsNoTracking().ToDictionaryAsync(x => new BalanceKey(x.PersonId, x.BankId), x => new { x.PersonId, x.BankId, x.Date, x.Sum });

var fiscalizedBalanceCount = await fiscalizedBalanceQuery.CountAsync();

int chunkSize = 1000;
for (int cur = 0; cur < fiscalizedBalanceCount; cur += chunkSize)
{
    fiscalizedBalanceQuery.Skip(cur).Take(chunkSize).ToArr;
}
var fiscalTransactionsQuery = _postgres.Transactions.Where(t => fiscalizedBalanceQuery.Any(r => t.PersonId == r.PersonId && t.BankId == r.BankId) && t.LastUpdated >= maxFiscalDate);



var notFiscalTransactionsQuery = _postgres.Transactions.Where(t => fiscalizedBalanceQuery.Any(r => t.PersonId == r.PersonId && t.BankId == r.BankId) == false);
*/



app.Run();

public partial class Program
{
    public static bool IsNswagBuild() => string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NswagGen")) == false;
}

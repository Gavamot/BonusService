// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using CommandLine;
using ImportBonuse.Postgres;
using ImportBonuse.Postgres.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Transaction = ImportBonuse.Postgres.Entity.Transaction;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(o =>
    {
        var services = new ServiceCollection();
        services.AddDbContext<PostgresDbContext>(opt =>
        {
            services.AddDbContext<PostgresDbContext>(opt => opt.UseNpgsql(o.ConStr));
        });
        var p = services.BuildServiceProvider();
        using var scope = p.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        using StreamReader read = new StreamReader(o.File);
        int n = 1;
        while (read.ReadLine() is { } line)
        {
            try
            {
                string personId = line.Split(';')[0];
                long sum = long.Parse(line.Split(';')[1]);
                if (sum == 0)
                {
                    throw new Exception($"Нет бонусов");
                }
                var t = new Transaction()
                {
                    Type = TransactionType.Shrink,
                    BankId = 1,
                    Description = "Перенесено с основного счета со старого бонусного счета",
                    TransactionId = $"migrate_{personId}",
                    BonusSum = sum,
                    PersonId = personId,
                    LastUpdated = DateTimeOffset.Now,
                };
                db.Transactions.Add(t);
                db.SaveChanges();
                Console.WriteLine($"[{n}] red {line} - added - {JsonSerializer.Serialize(t)}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{n}] can not insert the line - {line} - error = {e.Message}");
            }
            n++;
        }


    }).WithNotParsed(errors =>
    {
        foreach (var error in errors)
        {
            Console.Error.WriteLine(error);
        }
    });

class Options
{
    [Option('f', "file", Required = true, HelpText = "Path to file csv")]
    public string File { get; set; }

    [Option('p', "postgres", Required = true, HelpText = "Connection string for postgres")]
    public string ConStr { get; set; }
}

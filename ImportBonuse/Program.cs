// See https://aka.ms/new-console-template for more information

using System.Text;
using CommandLine;
using ImportBonuse.Postgres;
using ImportBonuse.Postgres.Entity;
using Transaction = ImportBonuse.Postgres.Entity.Transaction;

Console.OutputEncoding = Encoding.UTF8;
Parser.Default.ParseArguments<Options>(args)
    .WithParsed(o =>
    {
        using var db = new PostgresDbContext(o.ConStr);
        using StreamReader read = new StreamReader(o.File);
        int n = 1;
        var dt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.Zero);
        Transaction t = null;
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

                t = new Transaction()
                {
                    Type = TransactionType.Shrink,
                    BankId = 1,
                    Description = "Перенесено с основного счета со старого бонусного счета",
                    TransactionId = $"migrate_{personId}",
                    BonusSum = sum,
                    PersonId = personId,
                    LastUpdated = dt,
                };
                db.Transactions.Add(t);
                db.SaveChanges();
                Console.WriteLine($"[{n}] red {line} - added - id = {t.Id}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{n}] can not insert the line - {line} - error = {e.InnerException?.Message}");
                if (t != null)
                {
                    db.Transactions.Remove(t);
                }
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
    [Option('f', "file", Required = true, HelpText = "Path to file csv - фортмат -> PersonId;BonusSum -> Пример: 1.txt")]
    public string File { get; set; }

    [Option('p', "postgres", Required = true, HelpText = "Connection string for postgres -> Host=sql-postgre;Port=5432;Database=Identity;Username=postgres;Password=pass;")]
    public string ConStr { get; set; }
}

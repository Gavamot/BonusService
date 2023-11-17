using System.Diagnostics;
using System.Runtime.InteropServices;
namespace BonusService.Test.Common;

public static class SystemTerminalHelper
{
    public static Task ExecuteCommand(string command)
    {
        using var proc = new Process();
        proc.StartInfo.FileName = "/bin/bash";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            proc.StartInfo.FileName = "powershell";
        }
        proc.StartInfo.Arguments = "-c \" " + command + " \"";
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.Start();
        while (!proc.StandardOutput.EndOfStream) {
            Console.WriteLine (proc.StandardOutput.ReadLine());
        }
        return proc.WaitForExitAsync();
    }
}

public static class InfraHelper
{
    public const string PostgresContainerName = "postgres-test";
    public const string PostgresContainerPort = "9999";

    public static Task RunPostgresContainer()
    {
        var dockerCmd = $"""docker run --name {PostgresContainerName} -e POSTGRES_HOST_AUTH_METHOD=trust -e POSTGRES_USER=postgres -d -p {PostgresContainerPort}:5432 postgres""";
        return SystemTerminalHelper.ExecuteCommand(dockerCmd);
    }

    public const string MongoContainerName = "mongo-test";
    public const string MongoContainerPort = "9998";

    public static Task RunMongo(string dbName)
    {
        var dockerCmd =  $"""docker run --name {MongoContainerName} -d -p {MongoContainerPort}:27017 mongo""";;
        return SystemTerminalHelper.ExecuteCommand(dockerCmd);
    }

    public static Task CreateMongoDatabase(string dbName)
    {
        var dockerCmd = $"""docker exec -i {MongoContainerName} mongosh use {dbName}""";
        return SystemTerminalHelper.ExecuteCommand(dockerCmd);
    }

    public static Task DropMongoDatabase(string dbName)
    {
        var dockerCmd = $"""docker exec -i {MongoContainerName} mongosh --eval 'use {dbName}' --eval  'db.dropDatabase()'""";
        return SystemTerminalHelper.ExecuteCommand(dockerCmd);
    }
}

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
namespace BonusService.Test.Common;

public static class SystemTerminalHelper
{
    public static void ExecuteCommand(string command)
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
        StringBuilder output = new StringBuilder();
        while (!proc.StandardOutput.EndOfStream)
        {
            output.Append(proc.StandardOutput.ReadLine());
        }
    }
}

public static class InfraHelper
{
    public const string PostgresContainerName = "postgres-test";
    public const string PostgresContainerPort = "9999";

    public static void RunPostgresContainer()
    {
        var dockerCmd = $"""docker run --name {PostgresContainerName} -e POSTGRES_HOST_AUTH_METHOD=trust -e POSTGRES_USER=postgres -d -p {PostgresContainerPort}:5432 postgres -N 500""";
        SystemTerminalHelper.ExecuteCommand(dockerCmd);
    }

    public const string MongoContainerName = "mongo-test";
    public const string MongoContainerPort = "9998";

    public static void RunMongo(string dbName)
    {
        var dockerCmd =  $"""docker run --name {MongoContainerName} -d -p {MongoContainerPort}:27017 mongo""";;
        SystemTerminalHelper.ExecuteCommand(dockerCmd);
    }

    public static void CreateMongoDatabase(string dbName)
    {
        var dockerCmd = $"""docker exec -i {MongoContainerName} mongosh use --eval 'use {dbName}''""";
        SystemTerminalHelper.ExecuteCommand(dockerCmd);
    }

    public static void DropMongoDatabase(string dbName)
    {
        var dockerCmd = $"""docker exec -i {MongoContainerName} mongosh --eval 'use {dbName}' --eval  'db.dropDatabase()'""";
        SystemTerminalHelper.ExecuteCommand(dockerCmd);
    }
}

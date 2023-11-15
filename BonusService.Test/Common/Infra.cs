using System.Diagnostics;
using System.Runtime.InteropServices;
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
        while (!proc.StandardOutput.EndOfStream) {
            Console.WriteLine (proc.StandardOutput.ReadLine());
        }
        proc.WaitForExit();
    }
}

public static class InfraHelper
{
    public const string PostgresContainerName = "postgres-test";
    public const string PostgresContainerPort = "9999";

    public static void RunPostgresContainer()
    {
        var dockerCmd = $"""docker run --name {PostgresContainerName} -e POSTGRES_HOST_AUTH_METHOD=trust -e POSTGRES_USER=postgres -d -p {PostgresContainerPort}:5432 postgres""";
        SystemTerminalHelper.ExecuteCommand(dockerCmd);
    }

    public const string MongoContainerName = "mongo-test";
    public const string MongoContainerPort = "9998";

    public static string RunMongo()
    {
        var dbName = Guid.NewGuid().ToString();
        var dockerCmd =  $"""docker run --name {MongoContainerName} -d -p {MongoContainerPort}:27017 mongo""";;
        SystemTerminalHelper.ExecuteCommand(dockerCmd);
        return dbName;
    }

    public static string CreateMongoDatabase(string dbName)
    {
        var dockerCmd = $"""docker exec -i {MongoContainerName} mongosh use {dbName}""";
        SystemTerminalHelper.ExecuteCommand(dockerCmd);
        return dbName;
    }

    public static string DropMongoDatabase(string dbName)
    {
        var dockerCmd = $"""docker exec -i {MongoContainerName} mongosh --eval 'use {dbName}' --eval  'db.dropDatabase()'""";
        SystemTerminalHelper.ExecuteCommand(dockerCmd);
        return dbName;
    }


}

using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core;
using Cake.Frosting;
using Spectre.Console;

namespace ScriptCompiler;

public static class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Any() && args[0] == "install")
        {
            await Systemd.InstallServiceAsync();
            return;
        }

        AnsiConsole.MarkupLine("Starting up...");

        DotEnv.Load(".env");
        DotEnv.Load(".env.scriptcompiler"); // in case there are other apps in the same dir as ScriptCompiler

        string watchDirectory = Environment.GetEnvironmentVariable("ScriptCompiler_Watch_Directory") ?? GetDefaultScriptDir();

        InitializeScriptDirectory(watchDirectory);
        AnsiConsole.MarkupLine($"Watching script directory [green]{watchDirectory}[/]");

        using var fsw = new FSWGen(watchDirectory, "*.cs");
        await foreach (FileSystemEventArgs fse in fsw.Watch())
        {
            Console.WriteLine($"{fse.ChangeType} {fse.Name}");

            switch (fse.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    new CakeHost()
                        .UseContext<FrostingContext>()
                        .UseCakeSetting("ScriptName", fse.Name)
                        .UseCakeSetting("ScriptDir", watchDirectory)
                        .Run(args.Append("--verbosity=diagnostic"));
                    break;
                default:
                    Console.WriteLine("Not implemented yet");
                    break;
            }
        }
    }

    private static string GetDefaultScriptDir()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "cs-scripts");

    private static void InitializeScriptDirectory(string scriptDirectory)
    {
        // set up scripts directory
        Directory.CreateDirectory(scriptDirectory);
        var csprojContents = Utils.ReadTextResource("Scripts.csproj");
        File.WriteAllText(Path.Combine(scriptDirectory, "Scripts.csproj"), csprojContents);

        var helpersDirectory = Path.Combine(scriptDirectory, "Helpers");
        Directory.CreateDirectory(helpersDirectory);
        var cliHelper = Utils.ReadTextResource("Cli.cs");
        File.WriteAllText(Path.Combine(helpersDirectory, "Cli.cs"), cliHelper);
    }
}

[TaskName("Build")]
public sealed class BuildTask : FrostingTask<FrostingContext>
{
    public override void Run(FrostingContext context)
    {
        var scriptName = context.Configuration.GetValue("ScriptName");
        if (string.IsNullOrEmpty(scriptName))
            throw new ArgumentException("ScriptName not set");

        var scriptDir = context.Configuration.GetValue("ScriptDir");
        if (string.IsNullOrEmpty(scriptDir))
            throw new ArgumentException("Script directory not set");

        var scriptNameNoExtension = Path.GetFileNameWithoutExtension(scriptName);

        context.DotNetPublish(Path.Combine(scriptDir, "Scripts.csproj"), new DotNetPublishSettings
        {
            Configuration = "Debug",
            OutputDirectory = Path.Combine(scriptDir, "publish/"),
            Runtime = Utils.GetRid(),
            SelfContained = false,
            MSBuildSettings = new DotNetMSBuildSettings()
                .WithProperty("ProgramFile", scriptName)
                .WithProperty("AssemblyName", scriptNameNoExtension)
                .WithProperty("PublishSingleFile", "true")
                .WithProperty("DebugType", "embedded")
                .WithProperty("DeleteExistingFiles", "false")
        });

        context.EnsureDirectoryExists(Path.Combine(scriptDir, "compiled"));
        context.CopyFiles(Path.Combine(scriptDir, "publish/*"), Path.Combine(scriptDir, "compiled/"));
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(BuildTask))]
public class DefaultTask : FrostingTask
{
}

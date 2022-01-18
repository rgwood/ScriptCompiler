using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Frosting;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace ScriptCompiler;

public class BuildRunner : BackgroundService
{
    private readonly string _watchDirectory;

    public BuildRunner(string watchDirectory)
    {
        _watchDirectory = watchDirectory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        AnsiConsole.MarkupLine($"Watching script directory [green]{_watchDirectory}[/]");

        using var fsw = new FSWGen(_watchDirectory, "*.cs");
        await foreach (FileSystemEventArgs fse in fsw.Watch(stoppingToken))
        {
            Console.WriteLine($"{fse.ChangeType} {fse.Name}");

            switch (fse.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:
                    new CakeHost()
                        .UseContext<FrostingContext>()
                        .UseCakeSetting("ScriptName", fse.Name)
                        .UseCakeSetting("ScriptDir", _watchDirectory)
                        .Run(new string[] {"--verbosity=diagnostic"});
                    break;
                default:
                    Console.WriteLine("Not implemented yet");
                    break;
            }
        }
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

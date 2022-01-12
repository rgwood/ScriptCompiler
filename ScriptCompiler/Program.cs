using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core;
using Cake.Frosting;

namespace ScriptCompiler;

public static class Program
{
    // TODO don't hardcode this
    private const string ScriptsDir = @"C:\Users\reill\cs-scripts";

    public static async Task Main(string[] args)
    {
        InitializeScriptsDirectory(ScriptsDir);

        using var fsw = new FSWGen(ScriptsDir, "*.cs");
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
                        .UseCakeSetting("ScriptsDir", ScriptsDir)
                        .Run(args.Append("--verbosity=diagnostic"));
                    break;
                default:
                    Console.WriteLine("Not implemented yet");
                    break;
            }
        }
    }

    private static void InitializeScriptsDirectory(string scriptsDirectory)
    {
        // set up scripts directory
        Directory.CreateDirectory(scriptsDirectory);
        var csprojContents = Utils.ReadTextResource("Scripts.csproj");
        File.WriteAllText(Path.Combine(scriptsDirectory, "Scripts.csproj"), csprojContents);

        var helpersDirectory = Path.Combine(scriptsDirectory, "Helpers");
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

        var scriptsDir = context.Configuration.GetValue("ScriptsDir");
        if (string.IsNullOrEmpty(scriptsDir))
            throw new ArgumentException("Script directory not set");

        var scriptNameNoExtension = Path.GetFileNameWithoutExtension(scriptName);

        context.DotNetPublish(Path.Combine(scriptsDir, "Scripts.csproj"), new DotNetPublishSettings
        {
            Configuration = "Debug",
            OutputDirectory = Path.Combine(scriptsDir, "publish/"),
            Runtime = Utils.GetRid(),
            SelfContained = false,
            MSBuildSettings = new DotNetMSBuildSettings()
                .WithProperty("ProgramFile", scriptName)
                .WithProperty("AssemblyName", scriptNameNoExtension)
                .WithProperty("PublishSingleFile", "true")
                .WithProperty("DebugType", "embedded")
                .WithProperty("DeleteExistingFiles", "false")
        });

        context.EnsureDirectoryExists(Path.Combine(scriptsDir, "compiled"));
        context.CopyFiles(Path.Combine(scriptsDir, "publish/*"), Path.Combine(scriptsDir, "compiled/"));
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(BuildTask))]
public class DefaultTask : FrostingTask
{
}

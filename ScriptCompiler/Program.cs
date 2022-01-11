using Cake.Core;
using Cake.Frosting;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Publish;

namespace CakeScratch;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using var fsw = new FSWGen("../scripts", "*.cs");
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
                        .Run(args.Append("--verbosity=diagnostic"));
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
        var scriptNameNoExtension = Path.GetFileNameWithoutExtension(scriptName);

        context.DotNetPublish("../scripts/Scripts.csproj", new DotNetPublishSettings
        {
            Configuration = "Debug",
            OutputDirectory = "../scripts/publish/",
            Runtime = Utils.GetRid(),
            MSBuildSettings = new DotNetMSBuildSettings()
                .WithProperty("ProgramFile", scriptName)
                .WithProperty("AssemblyName", scriptNameNoExtension)
                .WithProperty("PublishSingleFile", "true")
                .WithProperty("SelfContained", "false")
                .WithProperty("DebugType", "embedded")
                .WithProperty("DeleteExistingFiles", "false")
        });

        context.EnsureDirectoryExists("../scripts/compiled");
        context.CopyFiles("../scripts/publish/*", "../scripts/compiled/");
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(BuildTask))]
public class DefaultTask : FrostingTask
{
}

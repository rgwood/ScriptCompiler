
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Utils;

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

        IHost host = new HostBuilder()
            .UseWindowsService() // only takes effect on Windows
            .ConfigureServices(services =>
            {
                // TODO configure Serilog
                services.AddHostedService<BuildRunner>(_ => new BuildRunner(watchDirectory));
            })
            .Build();

        await host.RunAsync();
    }

    private static string GetDefaultScriptDir()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "cs-scripts");

    private static void InitializeScriptDirectory(string scriptDirectory)
    {
        Directory.CreateDirectory(scriptDirectory);
        ResourceUtils.CopyAllEmbeddedResources("EmbeddedResources", scriptDirectory, overwrite: true);
    }
}

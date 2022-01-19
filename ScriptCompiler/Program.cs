using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utils;

namespace ScriptCompiler;

public static class Program
{
    public static async Task Main()
    {
        await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync();
    }
}

[Command]
public class DefaultCommand : ICommand
{
    public async ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine("Starting up...");

        DotEnv.Load(".env");
        DotEnv.Load(".env.scriptcompiler"); // in case there are other apps in the same dir as ScriptCompiler

        string watchDirectory = Environment.GetEnvironmentVariable("ScriptCompiler_Watch_Directory") ?? GetDefaultScriptDir();

        InitializeScriptDirectory(watchDirectory);

        var host = new HostBuilder()
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

[Command("install")]
public class InstallCommand : ICommand
{
    public async ValueTask ExecuteAsync(IConsole console)
    {
        await Systemd.InstallServiceAsync();
    }
}

using System.Diagnostics;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console;
using Utils;

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

        string scriptProjPath = Path.Combine(_watchDirectory, "Scripts.csproj");

        using var fsw = new FSWGen(_watchDirectory, "*.cs");
        await foreach (FileSystemEventArgs fse in fsw.Watch(stoppingToken))
        {
            var sw = Stopwatch.StartNew();
            Console.WriteLine($"{fse.ChangeType} {fse.Name}");

            var scriptNameNoExtension = Path.GetFileNameWithoutExtension(fse.Name);

            switch (fse.ChangeType)
            {
                case WatcherChangeTypes.Created:
                case WatcherChangeTypes.Changed:

                    Log.Information($"Starting build of {fse.Name}");
                    // TODO handle failures with a nice big error message
                    var result = await Cli.Wrap("dotnet")
                        .WithWorkingDirectory(_watchDirectory)
                        .WithArguments(new string[]{
                            "publish",
                            "--configuration=Release",
                            $"--runtime={DotnetUtils.GetRid()}",
                            "--self-contained=true",
                            $"\"{scriptProjPath}\"",
                            $"-p:ProgramFile={fse.Name}",
                            $"-p:AssemblyName={scriptNameNoExtension}",
                            $"-p:PublishSingleFile=true",
                            $"-p:DebugType=embedded",
                            $"-p:DeleteExistingFiles=false",
                            "--output=publish/"})
                            .WithPipeToConsole()
                            .WithValidation(CommandResultValidation.None)
                            .ExecuteBufferedAsync(stoppingToken);

                    if(result.ExitCode == 0)
                    {
                        Log.Information(result.StandardOutput);
                    }
                    else
                    {
                        Log.Error(result.StandardOutput);
                        continue;
                    }

                    var compiledDir = Path.Combine(_watchDirectory, "compiled");
                    AnsiConsole.WriteLine($"Copying compiled script to {compiledDir}");
                    Directory.CreateDirectory(compiledDir);
                    var publishDir = Path.Combine(_watchDirectory, "publish");

                    foreach(string filePath in Directory.GetFiles(publishDir))
                    {
                        var fileName = Path.GetFileName(filePath);
                        File.Copy(filePath, Path.Combine(compiledDir, fileName), overwrite: true);
                    }
                    AnsiConsole.MarkupLine($"Finished in [green]{sw.ElapsedMilliseconds}[/]ms");

                    break;
                default:
                    Console.WriteLine("Not implemented yet");
                    break;
            }
        }
    }
}

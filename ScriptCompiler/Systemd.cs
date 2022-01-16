﻿using System.Diagnostics;
using CliWrap;
using Spectre.Console;

namespace ScriptCompiler;

public static class Systemd
{
    /// <summary>
    /// Attempt to install the current process as a systemd service
    /// </summary>
    /// <returns>bool indicating whether the operation succeeded</returns>
    public static async Task<bool> InstallServiceAsync(string? name = null, string description = "")
    {
        string processPath = Environment.ProcessPath!;
        string processFileName = Path.GetFileName(processPath);
        string serviceName = name ?? processFileName;

        AnsiConsole.WriteLine($"Installing {processFileName} as a systemd service...");

        string unitFileContents = @$"
[Unit]
Description={description}

[Service]
Type=simple
ExecStart={processPath}

[Install]
WantedBy=multi-user.target";

        string unitFilePath = $"/etc/systemd/system/{serviceName}.service";

        try
        {
            AnsiConsole.WriteLine("Writing unit file...");
            File.WriteAllText(unitFilePath, unitFileContents);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] write failed to {unitFilePath}. Did you forget to use sudo?");
            AnsiConsole.WriteException(ex.Demystify());
            return false;
        }

        try
        {
            AnsiConsole.WriteLine("Enabling service...");
            await (Cli.Wrap("systemctl").WithArguments($"enable {serviceName}") |
                (Console.WriteLine, Console.Error.WriteLine)).ExecuteAsync();

            AnsiConsole.WriteLine("Starting service...");
            await (Cli.Wrap("systemctl").WithArguments($"start {serviceName}") |
                (Console.WriteLine, Console.Error.WriteLine)).ExecuteAsync();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] failed to enable+start service");
            AnsiConsole.WriteException(ex.Demystify());
            return false;
        }

        AnsiConsole.MarkupLine($"[green]Done! Install succeeded.[/]");
        return true;
    }
}

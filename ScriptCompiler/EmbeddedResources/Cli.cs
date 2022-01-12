using CliWrap;

namespace Scripts;

public static class Cli
{
    public static Command WithPipeToConsole(this Command cmd)
    {
        var stdout = Console.OpenStandardOutput();
        var stderr = Console.OpenStandardError();

        return cmd | (stdout, stderr);
    }

    public static async Task Run(string executable, string args)
    {
        await CliWrap.Cli.Wrap(executable).WithArguments(args).WithPipeToConsole().ExecuteAsync();
    }
}

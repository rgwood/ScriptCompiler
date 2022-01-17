namespace Scripts;

public static class Utils
{
    public static Command WithPipeToConsole(this Command cmd)
    {
        var stdout = Console.OpenStandardOutput();
        var stderr = Console.OpenStandardError();

        return cmd | (stdout, stderr);
    }

    public static async Task Run(string executable, string args)
    {
        await Cli.Wrap(executable).WithArguments(args).WithPipeToConsole().ExecuteAsync();
    }
}

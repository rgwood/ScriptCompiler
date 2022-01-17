using CliWrap;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ScriptCompiler;

public static class Utils
{
    public static Command WithPipeToConsole(this Command cmd)
    {
        var stdout = Console.OpenStandardOutput();
        var stderr = Console.OpenStandardError();

        return cmd | (stdout, stderr);
    }

    public static string GetRid()
    {
        string os;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            os = "osx";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            os = "win";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            os = "linux";
        else
            throw new Exception("Unsupported OS");

        string arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            _ => throw new Exception("Unsupported architecture")
        };

        return $"{os}-{arch}";
    }
}

using System.Runtime.InteropServices;

namespace CakeScratch;

public static class Utils
{
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

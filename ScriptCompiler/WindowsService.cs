
using System.Runtime.InteropServices;

namespace Utils;

public class WindowsService
{
    public static async Task InstallAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new NotSupportedException("Windows Services are only supported on Windows");
        }

        // TODO: check whether we're running as administrator

        // TODO: install service
        // sc create "ScriptRunner" binpath="C:\Users\reill\source\ScriptCompiler\ScriptCompiler\publish\ScriptCompiler.exe"

        Console.WriteLine("Installed as a Windows Service, running as LocalSystem. You may need to manually configure the service to run as another user.");

        // TODO: start service
    }

    public static void Uninstall()
    {
        // TODO implement
        throw new NotImplementedException();
    }
}

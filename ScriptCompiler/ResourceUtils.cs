
using System.Reflection;

namespace Utils;

public static class ResourceUtils
{
    public static string ReadTextResource(string name)
    {
        // Determine path
        var assembly = Assembly.GetExecutingAssembly();
        string resourcePath = name;
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        // TODO: use System.Reflection.Assembly.GetExecutingAssembly().GetName().Name instead
        if (!name.StartsWith(nameof(ScriptCompiler)))
        {
            resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(name));
        }

        using Stream stream = assembly!.GetManifestResourceStream(resourcePath)!;
        using StreamReader reader = new(stream!);
        return reader.ReadToEnd();
    }

    public static byte[] ReadBinaryResource(string name)
    {
        string resourceName = name.Replace(Environment.NewLine, ".");

        // Determine path
        var assembly = Assembly.GetExecutingAssembly();
        string resourcePath = name;
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        if (!name.StartsWith(Assembly.GetExecutingAssembly().GetName().Name!))
        {
            resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(resourceName));
        }

        using Stream stream = assembly!.GetManifestResourceStream(resourcePath)!;
        using BinaryReader r = new(stream);
        return r.ReadBytes(int.MaxValue);
    }

    public static void CopyFileIfNotExists(string fromName, string toDirectory)
    {
        string destPath = Path.Combine(toDirectory, fromName);

        if (File.Exists(destPath))
        {
            var fileContents = ReadBinaryResource(fromName);
            File.WriteAllBytes(destPath, fileContents);
        }
    }

    // TODO write a test for this
    public static string FileNameFromResourceName(string fullResourceName)
    {
        var allResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        var currAssembly = Assembly.GetExecutingAssembly();
        var assemblyName = currAssembly.GetName().Name!;
        // TODO parameterize the EmbeddedResources folder
        var prefix = $"{assemblyName}.EmbeddedResources.";

        var validExtensions = new List<string>() { "cs", "csproj" };

        // ex: Helpers.GlobalUsings.cs
        var resourceNameNoPrefix = fullResourceName[prefix.Length..];
        var splitName = resourceNameNoPrefix.Split('.');

        if (validExtensions.Contains(splitName[^1], StringComparer.OrdinalIgnoreCase))
        {
            return $"{string.Join(Path.DirectorySeparatorChar, splitName[..^1])}.{splitName[^1]}";
        }
        else
        {
            return string.Join(Path.DirectorySeparatorChar, splitName);
        }
    }

    public static void CopyAllEmbeddedResources(string baseDir, string destDir, bool overwrite = true)
    {
        var allResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        var currAssembly = Assembly.GetExecutingAssembly();
        var assemblyName = currAssembly.GetName().Name!;

        baseDir = baseDir.Trim().Replace('/', '.').Replace('\\', '.');

        var prefix = $"{assemblyName}.{baseDir}.";

        foreach (string resourceName in allResourceNames.Where(rn => rn.StartsWith(prefix)))
        {
            string destFilePath = Path.Combine(destDir, FileNameFromResourceName(resourceName));
            /// aaargh handling extensions is hard
            if (overwrite || !File.Exists(destFilePath))
            {
                // create directory if necessary
                Directory.CreateDirectory(Path.GetDirectoryName(destFilePath)!);

                Console.WriteLine($"Copying {resourceName} to {destFilePath}");
                using Stream stream = currAssembly!.GetManifestResourceStream(resourceName)!;
                // TODO: optimize this so we don't read the whole g-d file into memory
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                // using BinaryReader r = new(stream);
                // TODO: optimize this so we don't read the whole g-d file into memory
                // var fileBytes = r.ReadBytes(int.MaxValue);
                File.WriteAllBytes(destFilePath, bytes);
            }    
        }
    }

}

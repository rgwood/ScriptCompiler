using System.Reflection;

namespace Utils;

public static class ResourceUtils
{
    public static string ReadTextResource(string fileNameOrResourcePath)
    {
        // Determine path
        var assembly = Assembly.GetExecutingAssembly();
        string resourcePath = fileNameOrResourcePath;
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        if (!fileNameOrResourcePath.StartsWith(Assembly.GetExecutingAssembly().GetName().Name!))
        {
            string resourceName = fileNameOrResourcePath.Replace(Environment.NewLine, ".");

            resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(resourceName));
        }

        using Stream stream = assembly!.GetManifestResourceStream(resourcePath)!;
        using StreamReader reader = new(stream!);
        return reader.ReadToEnd();
    }

    public static byte[] ReadBinaryResource(string fileNameOrResourcePath)
    {
        // Determine path
        var assembly = Assembly.GetExecutingAssembly();
        string resourcePath = fileNameOrResourcePath;
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
        if (!fileNameOrResourcePath.StartsWith(Assembly.GetExecutingAssembly().GetName().Name!))
        {
            string resourceName = fileNameOrResourcePath.Replace(Environment.NewLine, ".");

            resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(resourceName));
        }

        using Stream stream = assembly!.GetManifestResourceStream(resourcePath)!;
        using BinaryReader r = new(stream);
        return r.ReadBytes(int.MaxValue);
    }

    // Do a best-effort attempt to map an embedded resource path to a file path.
    // Bit tricky because file extensions are ambiguous when encoded in a resource name
    public static string FileNameFromResourceName(string fullResourceName)
    {
        var validExtensions = new string[] { "cs", "csproj" };

        var allResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        var currAssembly = Assembly.GetExecutingAssembly();
        var assemblyName = currAssembly.GetName().Name!;
        // TODO parameterize the EmbeddedResources folder
        var prefix = $"{assemblyName}.EmbeddedResources.";

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
            if (overwrite || !File.Exists(destFilePath))
            {
                Console.WriteLine($"Copying {resourceName} to {destFilePath}");

                // create directory if necessary
                Directory.CreateDirectory(Path.GetDirectoryName(destFilePath)!);
                
                using Stream fromStream = currAssembly!.GetManifestResourceStream(resourceName)!;
                using var toStream = File.Create(destFilePath);
                fromStream.CopyTo(toStream);
            }    
        }
    }
}

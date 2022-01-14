// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Also has a bunch of tweaks by Reilly Wood, do whatever the heck you like with those

namespace ScriptCompiler;

public static class DotEnv
{
    public static void Load(string envFileName)
    {
        // let's find the root folder where the .env file can be found
        var rootPath = AppContext.BaseDirectory;
        var filePath = Path.Combine(rootPath, envFileName);

        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            if (line.TrimStart().StartsWith("#"))
            {
                // It's a comment
                continue;
            }

            var parts = line.Split(
                '=',
                2,
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                continue;

            var p0 = parts[0].Trim();
            var p1 = parts[1].Trim();

            if (p1.StartsWith("\""))
            {
                p1 = p1[1..^1];
            }

            Environment.SetEnvironmentVariable(p0, p1);
        }
    }
}

# ScriptCompiler

ScriptCompiler watches a directory with C# files for changes. Whenever files change, it recompiles them and puts the resulting executable in a known directory.

This automates much of the remaining hassle involved in using C# for scripting purposes.

## Why?

C# the language is pretty succinct these days ([top-level statements](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/top-level-statements), [global usings](https://www.hanselman.com/blog/implicit-usings-in-net-6)). But the tooling around the language still isn't quite perfect for scripting; you still need a project file for every program, and `dotnet run` takes a second or two.

Also, this has been stuck in my head for a while:

> [My OS has no notion of build system. It has all the source code on it and I can edit anything and run the command again with the change immediately applied. Interpreter or compiler, I don't know, that's an implementation detail. Then I wake up.](https://twitter.com/davidcrawshaw/status/1300614954865876992?s=20)

## Installation

TODO

## Build From Source

Clone the repo, `cd ScriptCompiler`, `dotnet build`.

To build a self-contained executable for Linux: `dotnet publish --configuration Release --runtime linux-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=embedded --output publish/`

## Configuration

By default, ScriptCompiler watches for changes in `~/cs-scripts`. To change that, set the `ScriptCompiler_Watch_Directory` environment variable.

ScriptCompiler can also load environment variables from a `.env` or `.env.scriptcompiler` file in the same directory as the executable. It should be formatted something like this:

```
ScriptCompiler_Watch_Directory="/foo/bar/scripts"
```

## Known Limitations

IDE features (IntelliSense, etc.) don't light up automatically in script files. This is an artifact of the MSBuild hackery I'm doing to enable multiple Program files in the same folder, there might be a way to fix this.

Compilation could theoretically be faster. We're currently just calling `dotnet publish`, and that means the compiler is starting with a cold cache every time. Might be interesting to explore hosting Roslyn in-process.

## To-do

- [ ] Windows: automatically add compiled dir to path: `Environment.SetEnvironmentVariable("Path", newPath, EnvironmentVariableTarget.User);`
- [ ] Windows: get installation as service working
- [ ] add screenshots/examples to readme
- [ ] Fancy error reporting mechanism: when compiling foo.exe fails, replace foo.exe with an exe that displays the error message and asks "would you like to rerun the last version?"
- [ ] GitHub Actions - build
- [ ] GitHub Actions - test
- [ ] GitHub Actions - publish
- [x] Try developing entirely on a Multipass instance
- [x] Log to file so we have better visibility when running as a Windows Service
- [x] Replace Cake with simple `dotnet publish` call. Can't programmatically get build output from Cake, lame
- [x] Add Serilog
- [x] Use a CLI framework. System.CommandLine? Cocona? CliFx?
- [x] Try trimming ScriptCompiler
- [x] Windows: get it running as a Windows Service
- [x] Add test project
- [x] better extraction of embedded resources. enumerate all embedded resources
- [x] systemd: set working directory = executable directory
- [x] systemd: detect user based on file location. if in `/home/foo/`, the user is `foo`
- [x] Get this working as a Systemd service
- [x] Embed Scripts.csproj. Write it to scripts directory as needed
- [x] read .env file
- [x] read scripts directory from env var
- [x] decide on a default script folder to use
- [x] Use ~/scripts folder
- [x] Add Spectre.Console to scripts
- [x] Add helper namespace via implicit usings
- [x] Easy systemd integration a la https://gist.github.com/rgwood/ede13b8324e4c63855f9b016d8104634

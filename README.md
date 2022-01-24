# ScriptCompiler

![image](https://user-images.githubusercontent.com/26268125/150707633-82eb85fd-3247-450f-b918-879c41ac4090.png)

ScriptCompiler is a simple tool for scripting in C#. It watches a directory with C# files for changes, and whenever a file changes it gets compiled into its own executable. It also comes with an opinionated set of utilities to make scripting easier.

## Why?

C# the language is pretty succinct these days ([top-level statements](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/program-structure/top-level-statements), [global usings](https://www.hanselman.com/blog/implicit-usings-in-net-6)) and the compiler is fast - seems like it should be good for scripting! But the tooling around the language still isn't quite there yet; you (normally) need a project file for every program, and `dotnet run` takes a second or two.

Also, this has been stuck in my head for a while:

> [My OS has no notion of build system. It has all the source code on it and I can edit anything and run the command again with the change immediately applied. Interpreter or compiler, I don't know, that's an implementation detail. Then I wake up.](https://twitter.com/davidcrawshaw/status/1300614954865876992?s=20)

## Get Started

ScriptCompiler builds are provided for Windows, Linux, and macOS.

1. Install [the .NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) if you haven't already.
2. [Navigate to the latest workflow on the GitHub Actions page](https://github.com/rgwood/ScriptCompiler/actions), then download the relevant build artifact for your OS (ex: `linux-arm64` for Linux on ARM).
3. Extract the executable to wherever you would like, and run it.
4. Create a `~/cs-scripts/hello.cs` file with 1 line: `Console.WriteLine("Hello World!");`
5. Run `~/cs-scripts/compiled/hello` (`hello.exe` on Windows). Done!

## Install as Service

ScriptCompiler comes with systemd and Windows Service integration, in case you'd like to leave it running all the time.

On Linux, run `sudo ./scriptcompiler install` to install it as a systemd service.

On Windows, run `scriptcompiler.exe install` from an administrator terminal.

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
- [ ] Fancy error reporting mechanism: when compiling foo.exe fails, replace foo.exe with an exe that displays the error message and asks "would you like to rerun the last version?"
- [x] add screenshots/examples to readme
- [x] GitHub Actions - build
- [x] GitHub Actions - test
- [x] GitHub Actions - publish
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

# ScriptCompiler

An attempt to blur the lines between source code and compiled executables.

> [My OS has no notion of build system. It has all the source code on it and I can edit anything and run the command again with the change immediately applied. Interpreter or compiler, I don't know, that's an implementation detail. Then I wake up.](https://twitter.com/davidcrawshaw/status/1300614954865876992?s=20)

ScriptCompiler watches a directory with C# files for changes. Whenever files change, it recompiles them and puts the resulting executable in a known directory.

This automates much of the remaining hassle involved in using C# for scripting purposes.

# Configuration

By default, ScriptCompiler watches for changes in `~/cs-scripts`. To change that, set the `ScriptCompiler_Watch_Directory` environment variable.

ScriptCompiler can also loads environment variables from a `.env` or `.env.scriptcompiler` file in the same directory as the executable:

```
ScriptCompiler_Watch_Directory="/foo/bar/scripts"
```

## Known Limitations

Compiling fails if you run this as a Systemd service (as root or as a specific user). This is a pain because it'd be nice to have the watcher running all the time.

IDE features (IntelliSense, etc.) don't light up automatically in script files. This is an artifact of the MSBuild hackery I'm doing, there might be a way to fix this.

## To-do

- [ ] Get this working as a Systemd service
- [x] Embed Scripts.csproj. Write it to scripts directory as needed
- [x] read .env file
- [x] read scripts directory from env var
- [x] decide on a default script folder to use
- [x] Use ~/scripts folder
- [ ] systemd: set working directory = executable directory
- [ ] systemd: detect user based on file location. if in `/home/foo/`, the user is `foo`
- [ ] add screenshots/examples to readme
- [ ] Easy systemd integration a la https://gist.github.com/rgwood/ede13b8324e4c63855f9b016d8104634
- [ ] same but for a Windows Service?

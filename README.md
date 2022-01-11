# ScriptCompiler

An attempt to blur the lines between source code and compiled executables.

> [My OS has no notion of build system. It has all the source code on it and I can edit anything and run the command again with the change immediately applied. Interpreter or compiler, I don't know, that's an implementation detail. Then I wake up.](https://twitter.com/davidcrawshaw/status/1300614954865876992?s=20)

ScriptCompiler watches a directory with C# files for changes. Whenever files change, it recompiles them and puts the resulting executable in a known directory.

This automates much of the remaining hassle involved in using C# for scripting purposes.

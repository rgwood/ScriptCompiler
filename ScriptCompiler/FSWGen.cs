using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace CakeScratch;

/// <summary>
/// A helper to turn FileSystemWatcher events into an async enumerable
/// Mostly taken from https://github.com/dotnet/hotreload-utils/blob/0c5f0aae8daacf231880d14cb1cd5a43e174161b/src/Microsoft.DotNet.HotReload.Utils.Generator/Util/FSWGen.cs
/// with debouncing functionality added
/// </summary>
public class FSWGen : IDisposable
{
    Channel<FileSystemEventArgs>? _channel;
    readonly FileSystemWatcher _fsw;
    List<string> _recentlyChangedFiles = new(); // for debouncing

    public FSWGen(string directoryPath, string filter)
    {
        _channel = Channel.CreateUnbounded<FileSystemEventArgs>(new UnboundedChannelOptions { SingleReader = true, AllowSynchronousContinuations = true });
        _fsw = new FileSystemWatcher(directoryPath, filter)
        {
        };
        _fsw.Changed += OnChanged;
        _fsw.Created += OnChanged;
        _fsw.Deleted += OnChanged;
    }

    private void OnChanged(object sender, FileSystemEventArgs eventArgs)
    {
        lock (_recentlyChangedFiles)
        {
            if (_recentlyChangedFiles.Contains(eventArgs.FullPath))
            {
                return;
            }
            _recentlyChangedFiles.Add(eventArgs.FullPath);
        }

        _channel?.Writer.WriteAsync(eventArgs).AsTask().Wait();

        System.Timers.Timer timer = new(100) { AutoReset = false };
        timer.Elapsed += (timerElapsedSender, timerElapsedArgs) =>
        {
            lock (_recentlyChangedFiles)
            {
                _recentlyChangedFiles.Remove(eventArgs.FullPath);
            }
        };
        timer.Start();
    }

    ~FSWGen() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fsw.EnableRaisingEvents = false;
            _fsw.Dispose();

            _channel?.Writer.Complete();
            _channel = null;
        }
    }

    enum WhenAnyResult
    {
        Completion,
        Read
    }
    public async IAsyncEnumerable<FileSystemEventArgs> Watch([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        try
        {
            _fsw.EnableRaisingEvents = true;
            var completion = _channel!.Reader.Completion.ContinueWith((t) => WhenAnyResult.Completion);
            while (true)
            {
                var readOne = _channel!.Reader.ReadAsync(cancellationToken).AsTask();
                Task<WhenAnyResult> t = await Task.WhenAny(completion, readOne.ContinueWith((t) => WhenAnyResult.Read)).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                switch (t.Result)
                {
                    case WhenAnyResult.Completion:
                        yield break;
                    case WhenAnyResult.Read:
                        yield return readOne.Result;
                        break;
                }
            }
        }
        finally
        {
            var fsw = _fsw;
            if (fsw != null)
                fsw.EnableRaisingEvents = false;
        }
    }
}

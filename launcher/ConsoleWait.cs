namespace PriorityAgent;

/// <summary>
/// FR #613: reusable console wait spinner (glyphs | / - \ on one line).
/// Quiet when <see cref="Quiet"/> is set (e.g. --mcp-catalog) or output is redirected — no spinner
/// writes (S5: MCP stdout stays JSON-RPC only). Call sites for clone/fetch/gh/fuel wired in later FRs.
/// </summary>
public interface IConsoleWaitIO
{
    bool IsOutputRedirected { get; }
    void Write(string value);
    void WriteLine(string value);
}

/// <summary>Default console adapter.</summary>
public sealed class SystemConsoleWaitIO : IConsoleWaitIO
{
    public bool IsOutputRedirected => Console.IsOutputRedirected;
    public void Write(string value) => Console.Write(value);
    public void WriteLine(string value) => Console.WriteLine(value);
}

public static class ConsoleWait
{
    public static readonly char[] Glyphs = ['|', '/', '-', '\\'];

    /// <summary>When true, <see cref="WithWait{T}"/> never writes (set by --mcp-catalog).</summary>
    public static bool Quiet { get; set; }

    /// <summary>
    /// Run <paramref name="action"/> while cycling spinner glyphs after <paramref name="label"/>.
    /// Returns the action result. On TTY settles with "<label> done".
    /// </summary>
    public static T WithWait<T>(
        string label,
        Func<T> action,
        IConsoleWaitIO? io = null,
        TimeSpan? frameDelay = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        io ??= new SystemConsoleWaitIO();
        if (Quiet || io.IsOutputRedirected)
            return action();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        TimeSpan delay = frameDelay ?? TimeSpan.FromMilliseconds(80);
        var spinner = Task.Run(() => RunSpinner(label, io, delay, cts.Token), CancellationToken.None);
        try
        {
            return action();
        }
        finally
        {
            cts.Cancel();
            try { spinner.Wait(TimeSpan.FromSeconds(2)); }
            catch (AggregateException) { /* cancelled */ }
            Settle(label, io);
        }
    }

    public static void WithWait(string label, Action action, IConsoleWaitIO? io = null, TimeSpan? frameDelay = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        WithWait(label, () => { action(); return 0; }, io, frameDelay, cancellationToken);
    }

    /// <summary>Synchronous glyph frames for unit tests (no thread / sleep).</summary>
    public static IReadOnlyList<string> PreviewFrames(string label, int count)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        var frames = new string[count];
        for (int i = 0; i < count; i++)
            frames[i] = Frame(label, Glyphs[i % Glyphs.Length]);
        return frames;
    }

    public static string Frame(string label, char glyph) => $"\r{label} {glyph}";

    public static void RunSpinner(string label, IConsoleWaitIO io, TimeSpan frameDelay, CancellationToken cancellationToken)
    {
        int i = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            io.Write(Frame(label, Glyphs[i % Glyphs.Length]));
            i++;
            if (frameDelay <= TimeSpan.Zero)
            {
                // test / busy path: yield once then check cancel
                Thread.Yield();
                if (i >= Glyphs.Length * 2)
                    cancellationToken.ThrowIfCancellationRequested();
                continue;
            }
            if (cancellationToken.WaitHandle.WaitOne(frameDelay))
                break;
        }
    }

    static void Settle(string label, IConsoleWaitIO io)
    {
        string clear = "\r" + new string(' ', Math.Max(12, label.Length + 4)) + "\r";
        io.Write(clear);
        io.WriteLine($"{label} done");
    }
}

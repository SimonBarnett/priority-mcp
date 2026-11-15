namespace PriorityAgent;

/// <summary>FR #609: injectable console for PickList unit tests.</summary>
sealed class ScriptedPickIO : IConsolePickIO
{
    readonly Queue<ConsoleKeyInfo> _keys;
    readonly Queue<string> _lines;
    public List<string> Writes { get; } = new();
    public bool IsInputRedirected { get; }

    public ScriptedPickIO(bool redirected, ConsoleKeyInfo[]? keys = null, string[]? lines = null)
    {
        IsInputRedirected = redirected;
        _keys = new Queue<ConsoleKeyInfo>(keys ?? Array.Empty<ConsoleKeyInfo>());
        _lines = new Queue<string>(lines ?? Array.Empty<string>());
    }

    public void Write(string value) => Writes.Add(value);
    public void WriteLine(string value) => Writes.Add(value + "\n");
    public string? ReadLine() => _lines.Count > 0 ? _lines.Dequeue() : null;
    public ConsoleKeyInfo ReadKey(bool intercept) =>
        _keys.Count > 0 ? _keys.Dequeue() : throw new InvalidOperationException("no scripted keys left");
}

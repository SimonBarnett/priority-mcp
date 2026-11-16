namespace PriorityAgent;

/// <summary>FR #609: injectable console for PickList unit tests.</summary>
sealed class ScriptedPickIO : IConsolePickIO
{
    readonly Queue<ConsoleKeyInfo> _keys;
    readonly Queue<string> _lines;
    public List<string> Writes { get; } = new();
    public bool IsInputRedirected { get; }
    public bool CanPositionCursor { get; init; } = true;
    public int Width { get; init; } = 80;
    public int MenuTop { get; init; } = 4;
    public int CursorTop { get; private set; }
    public List<(int Top, string Text, bool Highlighted)> Rows { get; } = new();
    public List<int> RowCountsAtRead { get; } = new();
    public List<(int Left, int Top)> CursorMoves { get; } = new();

    public ScriptedPickIO(bool redirected, ConsoleKeyInfo[]? keys = null, string[]? lines = null)
    {
        IsInputRedirected = redirected;
        _keys = new Queue<ConsoleKeyInfo>(keys ?? Array.Empty<ConsoleKeyInfo>());
        _lines = new Queue<string>(lines ?? Array.Empty<string>());
    }

    public void Write(string value) => Writes.Add(value);
    public void WriteLine(string value) => Writes.Add(value + "\n");
    public string? ReadLine() => _lines.Count > 0 ? _lines.Dequeue() : null;
    public ConsoleKeyInfo ReadKey(bool intercept)
    {
        RowCountsAtRead.Add(Rows.Count);
        return _keys.Count > 0 ? _keys.Dequeue() : throw new InvalidOperationException("no scripted keys left");
    }

    public bool TryBeginMenu(int lineCount, out int top, out int width)
    {
        top = MenuTop;
        width = Width;
        CursorTop = top;
        return CanPositionCursor;
    }

    public void SetCursorPosition(int left, int top)
    {
        CursorMoves.Add((left, top));
        CursorTop = top;
    }

    public void WriteRow(string value, bool highlighted)
    {
        Rows.Add((CursorTop, value, highlighted));
        Write(value);
    }
}

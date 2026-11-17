using System.Text;

namespace PriorityAgent;

/// <summary>
/// FR #609: reusable console list picker (TTY arrows + highlight + Enter).
/// Esc/cancel policy: Esc (or Ctrl+C if surfaced as Escape) returns null — caller treats as cancel.
/// Redirected stdin: numbered Ask / ReadLine (1-based); blank or out-of-range returns null.
/// Do not call from --mcp-catalog JSON-RPC paths (keep MCP stdout clean; S5).
/// Call sites: customer picker FR #610; project folder picker FR #611; CLI chooser remains a later FR.
/// </summary>
public interface IConsolePickIO
{
    bool IsInputRedirected { get; }
    void Write(string value);
    void WriteLine(string value);
    string? ReadLine();
    ConsoleKeyInfo ReadKey(bool intercept);
    bool TryBeginMenu(int lineCount, out int top, out int width);
    void SetCursorPosition(int left, int top);
    void WriteRow(string value, bool highlighted);
    /// <summary>FR #687: clear the console before a new interactive question (TTY only).</summary>
    void ClearScreen();
}

/// <summary>Default console adapter for live TTY / redirected stdin.</summary>
public sealed class SystemConsolePickIO : IConsolePickIO
{
    public bool IsInputRedirected => Console.IsInputRedirected;
    public void Write(string value) => Console.Write(value);
    public void WriteLine(string value) => Console.WriteLine(value);
    public string? ReadLine() => Console.ReadLine();
    public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);

    public bool TryBeginMenu(int lineCount, out int top, out int width)
    {
        top = width = 0;
        if (Console.IsInputRedirected || Console.IsOutputRedirected ||
            (!OperatingSystem.IsWindows() &&
             string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TERM"))) ||
            Environment.GetEnvironmentVariable("TERM") == "dumb")
            return false;
        try
        {
            width = Console.WindowWidth;
            if (width < 8 || lineCount >= Console.WindowHeight)
                return false;
            var position = Console.GetCursorPosition();
            Console.SetCursorPosition(position.Left, position.Top);
            // Reserve the entire block before drawing so navigation never scrolls at the bottom.
            for (int i = 0; i < lineCount; i++)
                Console.WriteLine();
            top = Console.CursorTop - lineCount;
            Console.SetCursorPosition(0, top);
            return true;
        }
        catch (Exception ex) when (ex is IOException or PlatformNotSupportedException or ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

    public void WriteRow(string value, bool highlighted)
    {
        if (!highlighted)
        {
            Console.Write(value);
            return;
        }
        var foreground = Console.ForegroundColor;
        var background = Console.BackgroundColor;
        try
        {
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(value);
        }
        finally
        {
            Console.ResetColor();
            if ((int)foreground >= 0)
                Console.ForegroundColor = foreground;
            if ((int)background >= 0)
                Console.BackgroundColor = background;
        }
    }

    public void ClearScreen()
    {
        // FR #687: never clear when stdin/stdout is redirected (pipes, tests, MCP).
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
            return;
        try { Console.Clear(); }
        catch (Exception ex) when (ex is IOException or PlatformNotSupportedException or ArgumentOutOfRangeException)
        {
            // Headless / no buffer — leave prior output as-is.
        }
    }
}

public static class ConsolePick
{
    /// <summary>
    /// Pick one item. Returns 0-based index, or null on Esc / cancel / invalid redirected input.
    /// TTY: Up/Down/Home/End moves highlight in place; Enter confirms; Esc cancels.
    /// Without cursor positioning: prints numbered list and reads a 1-based index line.
    /// </summary>
    public static int? PickList(
        IReadOnlyList<string> items,
        string prompt,
        IConsolePickIO? io = null,
        int initialIndex = 0)
    {
        if (items is null || items.Count == 0)
            throw new ArgumentException("PickList requires at least one item.", nameof(items));

        io ??= new SystemConsolePickIO();
        // FR #687: clear between questions on interactive TTY so prior answers do not scroll.
        if (!io.IsInputRedirected)
            io.ClearScreen();
        if (io.IsInputRedirected || !io.TryBeginMenu(items.Count + 1, out int top, out int width))
            return PickRedirected(items, prompt, io);

        int index = Math.Clamp(initialIndex, 0, items.Count - 1);
        try
        {
            io.WriteRow(FitRow(prompt, width), false);
            for (int i = 0; i < items.Count; i++)
                DrawRow(i, i == index);
            while (true)
            {
                ConsoleKeyInfo key = io.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Escape)
                    return null;
                if (key.Key == ConsoleKey.Enter)
                    return index;
                int next = key.Key switch
                {
                    ConsoleKey.UpArrow => (index - 1 + items.Count) % items.Count,
                    ConsoleKey.DownArrow => (index + 1) % items.Count,
                    ConsoleKey.Home => 0,
                    ConsoleKey.End => items.Count - 1,
                    _ => index,
                };
                if (next != index)
                {
                    DrawRow(index, false);
                    DrawRow(next, true);
                    index = next;
                }
            }
        }
        finally
        {
            io.SetCursorPosition(0, top + items.Count + 1);
        }

        void DrawRow(int row, bool highlighted)
        {
            io.SetCursorPosition(0, top + row + 1);
            io.WriteRow(FitRow(FormatRow(row, items[row], highlighted), width), highlighted);
        }
    }

    static int? PickRedirected(IReadOnlyList<string> items, string prompt, IConsolePickIO io)
    {
        io.WriteLine(prompt);
        for (int i = 0; i < items.Count; i++)
            io.WriteLine($"  {i + 1,2}. {items[i]}");
        io.Write($"Pick [1-{items.Count}]: ");
        string raw = (io.ReadLine() ?? "").Trim();
        if (raw.Length == 0)
            return null;
        if (!int.TryParse(raw, out int oneBased))
            return null;
        if (oneBased < 1 || oneBased > items.Count)
            return null;
        return oneBased - 1;
    }

    static string FitRow(string value, int width)
    {
        var row = new StringBuilder();
        int columns = 0;
        foreach (var rune in value.EnumerateRunes())
        {
            // Conservatively budget two cells for non-ASCII and leave the last column unused.
            int cells = rune.IsAscii || Rune.IsControl(rune) ? 1 : 2;
            if (columns + cells >= width)
                break;
            row.Append(Rune.IsControl(rune) ? " " : rune.ToString());
            columns += cells;
        }
        return row.ToString();
    }

    internal static string FormatRow(int zeroBased, string label, bool highlighted) =>
        highlighted ? $"> {zeroBased + 1,2}. {label}" : $"  {zeroBased + 1,2}. {label}";
}

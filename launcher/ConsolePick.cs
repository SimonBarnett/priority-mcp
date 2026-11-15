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
}

/// <summary>Default console adapter for live TTY / redirected stdin.</summary>
public sealed class SystemConsolePickIO : IConsolePickIO
{
    public bool IsInputRedirected => Console.IsInputRedirected;
    public void Write(string value) => Console.Write(value);
    public void WriteLine(string value) => Console.WriteLine(value);
    public string? ReadLine() => Console.ReadLine();
    public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);
}

public static class ConsolePick
{
    /// <summary>
    /// Pick one item. Returns 0-based index, or null on Esc / cancel / invalid redirected input.
    /// TTY: Up/Down moves highlight; Enter confirms; Esc cancels.
    /// Redirected: prints numbered list and reads a 1-based index line (same spirit as Ask/ReadLine).
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
        if (io.IsInputRedirected)
            return PickRedirected(items, prompt, io);

        int index = Math.Clamp(initialIndex, 0, items.Count - 1);
        io.WriteLine(prompt);
        Draw(items, index, io);
        while (true)
        {
            ConsoleKeyInfo key = io.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Escape)
                return null;
            if (key.Key == ConsoleKey.Enter)
                return index;
            if (key.Key == ConsoleKey.UpArrow)
            {
                index = (index - 1 + items.Count) % items.Count;
                Redraw(items, index, io);
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                index = (index + 1) % items.Count;
                Redraw(items, index, io);
            }
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

    static void Draw(IReadOnlyList<string> items, int index, IConsolePickIO io)
    {
        for (int i = 0; i < items.Count; i++)
            io.WriteLine(FormatRow(i, items[i], i == index));
    }

    static void Redraw(IReadOnlyList<string> items, int index, IConsolePickIO io)
    {
        // Re-print the block (portable for tests / hosts without cursor APIs).
        io.WriteLine($"--- ({index + 1}/{items.Count}) ---");
        Draw(items, index, io);
    }

    internal static string FormatRow(int zeroBased, string label, bool highlighted) =>
        highlighted ? $"> {zeroBased + 1,2}. {label}" : $"  {zeroBased + 1,2}. {label}";
}

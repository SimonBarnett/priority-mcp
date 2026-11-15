namespace PriorityAgent;

/// <summary>Fake console for FR #613 WaitSpinner tests.</summary>
sealed class ScriptedWaitIO : IConsoleWaitIO
{
    public bool IsOutputRedirected { get; }
    public List<string> Writes { get; } = new();

    public ScriptedWaitIO(bool redirected) => IsOutputRedirected = redirected;

    public void Write(string value) => Writes.Add(value);
    public void WriteLine(string value) => Writes.Add(value + "\n");
}

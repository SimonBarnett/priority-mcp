namespace PriorityAgent;

/// <summary>
/// FR #794: format additive launcher log lines for timed startup phases.
/// Example: <c>phase ensureclone_fetch duration_ms=104000 path=C:\AI\priority-mcp</c>
/// </summary>
public static class PhaseTiming
{
    public static string Format(
        string name,
        long durationMs,
        string? path = null,
        string? root = null,
        string? extra = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("phase name required", nameof(name));
        var parts = new List<string>
        {
            "phase",
            name.Trim(),
            $"duration_ms={durationMs}",
        };
        if (!string.IsNullOrWhiteSpace(path))
            parts.Add("path=" + path.Trim());
        if (!string.IsNullOrWhiteSpace(root))
            parts.Add("root=" + root.Trim());
        if (!string.IsNullOrWhiteSpace(extra))
            parts.Add(extra.Trim());
        return string.Join(' ', parts);
    }
}

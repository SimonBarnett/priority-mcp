using System.Globalization;
using System.Text.Json;

namespace PriorityAgent;

/// <summary>
/// FR #791: skip blocking <c>git fetch</c> when a recent successful pull left HEAD==origin/branch.
/// Stamp lives at <c>.git/priority-agent-last-pull.json</c> under the clone.
/// </summary>
public static class ClonePullTtl
{
    public const string StampFileName = "priority-agent-last-pull.json";
    public static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(45);

    public static string StampPath(string cloneDir) =>
        Path.Combine(cloneDir ?? "", ".git", StampFileName);

    public static TimeSpan ResolveTtl(string? envMinutes)
    {
        if (string.IsNullOrWhiteSpace(envMinutes)) return DefaultTtl;
        if (!int.TryParse(envMinutes.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int mins))
            return DefaultTtl;
        if (mins < 5) mins = 5;
        if (mins > 240) mins = 240;
        return TimeSpan.FromMinutes(mins);
    }

    public static bool ShouldSkipFetch(
        string currentHead,
        string branch,
        bool forcePull,
        DateTime utcNow,
        TimeSpan ttl,
        string? stampJson)
    {
        if (forcePull) return false;
        if (string.IsNullOrWhiteSpace(currentHead) || string.IsNullOrWhiteSpace(branch)) return false;
        if (string.IsNullOrWhiteSpace(stampJson)) return false;
        if (!TryParseStamp(stampJson, out string head, out string origin, out string stampedBranch, out DateTime pulledAt))
            return false;
        if (!string.Equals(stampedBranch, branch, StringComparison.Ordinal)) return false;
        if (!string.Equals(head, currentHead, StringComparison.OrdinalIgnoreCase)) return false;
        // Only trust stamps written when HEAD already matched origin/branch.
        if (!string.Equals(head, origin, StringComparison.OrdinalIgnoreCase)) return false;
        if (utcNow < pulledAt) return false;
        return (utcNow - pulledAt) <= ttl;
    }

    public static string BuildStampJson(string head, string origin, string branch, DateTime utcNow)
    {
        // Compact JSON without depending on source-gen; ASCII keys for hand-edit.
        string h = JsonEscape(head ?? "");
        string o = JsonEscape(origin ?? "");
        string b = JsonEscape(branch ?? "");
        string t = utcNow.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
        return "{\"head\":\"" + h + "\",\"origin\":\"" + o + "\",\"branch\":\"" + b + "\",\"pulledAtUtc\":\"" + t + "\"}\n";
    }

    public static bool TryParseStamp(
        string json,
        out string head,
        out string origin,
        out string branch,
        out DateTime pulledAtUtc)
    {
        head = origin = branch = "";
        pulledAtUtc = default;
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            head = root.TryGetProperty("head", out var h) ? (h.GetString() ?? "") : "";
            origin = root.TryGetProperty("origin", out var o) ? (o.GetString() ?? "") : "";
            branch = root.TryGetProperty("branch", out var b) ? (b.GetString() ?? "") : "";
            string? ts = root.TryGetProperty("pulledAtUtc", out var t) ? t.GetString() : null;
            if (head.Length == 0 || origin.Length == 0 || branch.Length == 0 || string.IsNullOrWhiteSpace(ts))
                return false;
            if (!DateTime.TryParse(ts, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out pulledAtUtc))
                return false;
            if (pulledAtUtc.Kind == DateTimeKind.Unspecified)
                pulledAtUtc = DateTime.SpecifyKind(pulledAtUtc, DateTimeKind.Utc);
            else
                pulledAtUtc = pulledAtUtc.ToUniversalTime();
            return true;
        }
        catch
        {
            return false;
        }
    }

    static string JsonEscape(string s) =>
        s.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}

using System.Globalization;
using System.Text;
using System.Text.Json;

namespace PriorityAgent;

/// <summary>Disk row for FR #793 customer-topic cache (mirrors launcher Customer fields).</summary>
public sealed record CustomerCacheRow(string Name, string FullName, string DefaultBranch, string Description);

/// <summary>
/// FR #793: short-TTL disk cache for <c>gh api user/repos</c> topic-filtered customer list.
/// Default path: <c>%LOCALAPPDATA%\PriorityAgent\customers-topic-cache.json</c>.
/// </summary>
public static class CustomerListCache
{
    public const string DefaultFileName = "customers-topic-cache.json";
    public static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(30);

    public static string DefaultCachePath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PriorityAgent",
            DefaultFileName);

    public static TimeSpan ResolveTtl(string? envMinutes)
    {
        if (string.IsNullOrWhiteSpace(envMinutes)) return DefaultTtl;
        if (!int.TryParse(envMinutes.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int mins))
            return DefaultTtl;
        if (mins < 5) mins = 5;
        if (mins > 240) mins = 240;
        return TimeSpan.FromMinutes(mins);
    }

    public static bool TryLoad(
        string path,
        string topic,
        DateTime utcNow,
        TimeSpan ttl,
        out IReadOnlyList<CustomerCacheRow> rows,
        out TimeSpan age)
    {
        rows = Array.Empty<CustomerCacheRow>();
        age = default;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;
        string json;
        try { json = File.ReadAllText(path); }
        catch { return false; }
        if (!TryParse(json, out string stampedTopic, out DateTime fetchedAt, out var parsed))
            return false;
        if (!string.Equals(stampedTopic, topic, StringComparison.Ordinal)) return false;
        if (parsed.Count == 0) return false;
        if (utcNow < fetchedAt) return false;
        age = utcNow - fetchedAt;
        if (age > ttl) return false;
        rows = parsed;
        return true;
    }

    public static string BuildJson(string topic, DateTime utcNow, IReadOnlyList<CustomerCacheRow> rows)
    {
        var sb = new StringBuilder();
        sb.Append("{\"topic\":\"").Append(JsonEscape(topic ?? "")).Append("\",");
        sb.Append("\"fetchedAtUtc\":\"").Append(utcNow.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)).Append("\",");
        sb.Append("\"customers\":[");
        for (int i = 0; i < rows.Count; i++)
        {
            if (i > 0) sb.Append(',');
            var r = rows[i];
            sb.Append("{\"name\":\"").Append(JsonEscape(r.Name)).Append("\",");
            sb.Append("\"fullName\":\"").Append(JsonEscape(r.FullName)).Append("\",");
            sb.Append("\"defaultBranch\":\"").Append(JsonEscape(r.DefaultBranch)).Append("\",");
            sb.Append("\"description\":\"").Append(JsonEscape(r.Description)).Append("\"}");
        }
        sb.Append("]}\n");
        return sb.ToString();
    }

    public static bool TryParse(
        string json,
        out string topic,
        out DateTime fetchedAtUtc,
        out List<CustomerCacheRow> rows)
    {
        topic = "";
        fetchedAtUtc = default;
        rows = new List<CustomerCacheRow>();
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            topic = root.TryGetProperty("topic", out var t) ? (t.GetString() ?? "") : "";
            string? ts = root.TryGetProperty("fetchedAtUtc", out var f) ? f.GetString() : null;
            if (topic.Length == 0 || string.IsNullOrWhiteSpace(ts)) return false;
            if (!DateTime.TryParse(ts, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out fetchedAtUtc))
                return false;
            if (fetchedAtUtc.Kind == DateTimeKind.Unspecified)
                fetchedAtUtc = DateTime.SpecifyKind(fetchedAtUtc, DateTimeKind.Utc);
            else
                fetchedAtUtc = fetchedAtUtc.ToUniversalTime();
            if (!root.TryGetProperty("customers", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return false;
            foreach (var el in arr.EnumerateArray())
            {
                string name = el.TryGetProperty("name", out var n) ? (n.GetString() ?? "") : "";
                string full = el.TryGetProperty("fullName", out var fn) ? (fn.GetString() ?? "") : "";
                string branch = el.TryGetProperty("defaultBranch", out var b) ? (b.GetString() ?? "") : "";
                string desc = el.TryGetProperty("description", out var d) ? (d.GetString() ?? "") : "";
                if (name.Length == 0 || full.Length == 0 || branch.Length == 0) continue;
                rows.Add(new CustomerCacheRow(name, full, branch, desc));
            }
            return rows.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    static string JsonEscape(string s) =>
        s.Replace("\\", "\\\\", StringComparison.Ordinal)
         .Replace("\"", "\\\"", StringComparison.Ordinal)
         .Replace("\r", "\\r", StringComparison.Ordinal)
         .Replace("\n", "\\n", StringComparison.Ordinal);
}

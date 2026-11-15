using System.Text.Json;

namespace PriorityAgent;

/// <summary>
/// FR #635: load connected airc hosts. Live source is bob digest
/// <c>airc_console_details</c> (IRC roster Jeeves already maintains). Fixture env
/// <c>PRIORITY_AGENT_AIRC_DIGEST_JSON</c> supplies offline NAMES-style replies for tests.
/// </summary>
public static class AircHostFetch
{
    public const string Label = "airc hosts";
    public const string DefaultDigestUrl = "https://bob.ntsa.uk/bob/v1/digest";
    public const string FixtureEnv = "PRIORITY_AGENT_AIRC_DIGEST_JSON";
    public const string DigestUrlEnv = "PRIORITY_AGENT_DIGEST_URL";

    public static AircHostFetchResult Run(Func<AircHostFetchResult>? fetch = null, IConsoleWaitIO? io = null)
    {
        fetch ??= FetchOnce;
        try
        {
            return ConsoleWait.WithWait(Label, () =>
            {
                AircHostFetchResult result = fetch();
                if (!result.Ok)
                    throw new AircHostFetchFailed(result.Detail);
                return result;
            }, io);
        }
        catch (AircHostFetchFailed ex)
        {
            return new AircHostFetchResult(false, Array.Empty<AircHost>(), ex.Message);
        }
    }

    public static AircHostFetchResult FetchOnce()
    {
        try
        {
            string? fixture = Environment.GetEnvironmentVariable(FixtureEnv);
            if (!string.IsNullOrWhiteSpace(fixture))
                return ParseDigestJson(fixture);

            string? url = Environment.GetEnvironmentVariable(DigestUrlEnv);
            if (string.IsNullOrWhiteSpace(url))
                url = DefaultDigestUrl;
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
            string body = http.GetStringAsync(url).GetAwaiter().GetResult();
            return ParseDigestJson(body);
        }
        catch (Exception ex)
        {
            return new AircHostFetchResult(
                false,
                Array.Empty<AircHost>(),
                "IRC roster unreachable (" + ex.Message + "). Retry with /airc when bob digest or IRC is back.");
        }
    }

    public static AircHostFetchResult ParseDigestJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new AircHostFetchResult(false, Array.Empty<AircHost>(), "IRC roster empty response. Retry with /airc.");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var hosts = new List<AircHost>();

        if (root.TryGetProperty("airc_console_details", out var details) && details.ValueKind == JsonValueKind.Array)
        {
            foreach (var row in details.EnumerateArray())
            {
                if (TryReadHost(row, out AircHost host))
                    hosts.Add(host);
            }
        }
        else if (root.TryGetProperty("airc_consoles", out var nicks) && nicks.ValueKind == JsonValueKind.Array)
        {
            foreach (var nickEl in nicks.EnumerateArray())
            {
                string nick = nickEl.GetString() ?? "";
                if (nick.Length == 0) continue;
                string machine = nick.EndsWith("_console", StringComparison.OrdinalIgnoreCase)
                    ? nick[..^"_console".Length]
                    : nick;
                hosts.Add(new AircHost(machine, nick, "", null));
            }
        }

        hosts = hosts
            .OrderBy(h => h.Machine, StringComparer.OrdinalIgnoreCase)
            .ThenBy(h => h.Nick, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return new AircHostFetchResult(true, hosts, "");
    }

    static bool TryReadHost(JsonElement row, out AircHost host)
    {
        host = default;
        if (row.ValueKind != JsonValueKind.Object) return false;
        string nick = GetStr(row, "nick");
        string machine = GetStr(row, "machine");
        string channel = GetStr(row, "channel");
        string? domain = NullIfEmpty(GetStr(row, "domain"));
        string? workgroup = NullIfEmpty(GetStr(row, "workgroup"));
        string? domainWorkgroup = NullIfEmpty(GetStr(row, "domain_workgroup"));
        if (domainWorkgroup is null && domain is not null && workgroup is not null)
            domainWorkgroup = domain + "\\" + workgroup;
        else if (domainWorkgroup is null)
            domainWorkgroup = domain ?? workgroup;
        if (nick.Length == 0 && machine.Length == 0) return false;
        if (machine.Length == 0 && nick.EndsWith("_console", StringComparison.OrdinalIgnoreCase))
            machine = nick[..^"_console".Length];
        if (nick.Length == 0)
            nick = machine + "_console";
        host = new AircHost(machine, nick, channel, domainWorkgroup);
        return true;
    }

    static string GetStr(JsonElement row, string name) =>
        row.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String ? (el.GetString() ?? "") : "";

    static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    sealed class AircHostFetchFailed : Exception
    {
        public AircHostFetchFailed(string detail) : base(detail ?? "") { }
    }
}

using System.Text.Json;

namespace PriorityAgent;

/// <summary>
/// FR #635: persist the /airc PickList choice for this machine profile.
/// Session routing that consumes the selection is FR #636.
/// Never stores IRC passwords or tokens.
/// </summary>
public static class AircSelection
{
    public const string FileName = "airc-selection.json";

    public static string DefaultPath(string? localAppData = null)
    {
        string root = localAppData
            ?? Environment.GetEnvironmentVariable("LOCALAPPDATA")
            ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, "PriorityAgent", FileName);
    }

    public static void Save(AircHost host, string? path = null, DateTimeOffset? at = null)
    {
        path ??= DefaultPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var payload = new Dictionary<string, string?>
        {
            ["machine"] = host.Machine,
            ["nick"] = host.Nick,
            ["channel"] = host.Channel,
            ["domainWorkgroup"] = host.DomainWorkgroup,
            ["selectedAt"] = (at ?? DateTimeOffset.UtcNow).ToString("o"),
        };
        File.WriteAllText(path, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static bool TryLoad(out AircHost host, string? path = null)
    {
        host = default;
        path ??= DefaultPath();
        if (!File.Exists(path)) return false;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;
            string machine = root.TryGetProperty("machine", out var m) ? (m.GetString() ?? "") : "";
            string nick = root.TryGetProperty("nick", out var n) ? (n.GetString() ?? "") : "";
            string channel = root.TryGetProperty("channel", out var c) ? (c.GetString() ?? "") : "";
            string? dw = root.TryGetProperty("domainWorkgroup", out var d) ? d.GetString() : null;
            if (machine.Length == 0 && nick.Length == 0) return false;
            host = new AircHost(machine, nick, channel, string.IsNullOrWhiteSpace(dw) ? null : dw);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

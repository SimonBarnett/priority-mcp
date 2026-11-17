namespace PriorityAgent;

/// <summary>Shared launcher helpers (command line, connections status, session files).</summary>
public static class LauncherSupport
{
    public const int MaxWindowsCommandLineLength = 32_000;

    /// <summary>Default hosted Priority MCP (FR #669). Interactive start must preserve this URL.</summary>
    public const string HostedMcpUrl = "https://mcp.ntsa.uk/mcp";

    /// <summary>FR #669: true when PRIORITY_AGENT_MCP_LOCAL / --mcp-local opts into local stdio.</summary>
    public static bool PreferLocalMcpStdio(string? envOrFlag)
    {
        string v = (envOrFlag ?? "").Trim().ToLowerInvariant();
        return v is "1" or "true" or "yes" or "on" or "local" or "stdio";
    }

    /// <summary>FR #669: grok mcp list value looks like an HTTP(S) MCP registration.</summary>
    public static bool IsHostedMcpRegistration(string? current)
    {
        string t = (current ?? "").Trim();
        if (t.Length == 0) return false;
        if (t.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) return true;
        if (t.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return true;
        // grok mcp list may prefix transport, e.g. "http https://mcp.ntsa.uk/mcp"
        if (t.Contains("https://", StringComparison.OrdinalIgnoreCase)) return true;
        if (t.Contains("http://", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    /// <summary>
    /// FR #669: what EnsureGrokMcp should do. Hosted URLs are never replaced unless local stdio is opted in.
    /// </summary>
    public static string DecideGrokMcpAction(string? current, string localWant, bool preferLocal)
    {
        string cur = (current ?? "").Trim();
        string want = (localWant ?? "").Trim();
        if (preferLocal)
        {
            if (cur.Equals(want, StringComparison.OrdinalIgnoreCase)) return "keep-local";
            return "register-local";
        }
        if (IsHostedMcpRegistration(cur)) return "keep-hosted";
        if (cur.Length == 0) return "register-hosted";
        // Already local (or other) without opt-in: leave it; do not swap.
        return "keep-configured";
    }

    public static bool ConnectionFileExists(string path) => File.Exists(path);

    public static string ConnectionFileStatus(string customer, string path, bool exists) =>
        $"connections file for {customer} {(exists ? "exists" : "MISSING - ask Haitch for it")}: {path} (contents not read)";

    public static void WriteSessionMetadata(string sessionDirectory, string rules, object launchMetadata)
    {
        Directory.CreateDirectory(sessionDirectory);
        File.WriteAllText(Path.Combine(sessionDirectory, "rules.md"), rules);
        File.WriteAllText(Path.Combine(sessionDirectory, "launch.json"),
            System.Text.Json.JsonSerializer.Serialize(launchMetadata, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    public static List<string> BuildGrokArguments(string workDir, string sessionId, string rules, string brief, bool smoke)
    {
        var args = new List<string> { "--cwd", workDir, "--session-id", sessionId, "--trust", "--rules", rules };
        if (smoke) args.AddRange(new[] { "--max-turns", "4", "-p" });
        args.Add(brief);
        return args;
    }

    public static int WindowsCommandLineLength(string executable, IEnumerable<string> arguments) =>
        string.Join(' ', new[] { QuoteWindowsArgument(executable) }.Concat(arguments.Select(QuoteWindowsArgument))).Length + 1;

    public static int ValidateWindowsCommandLine(string executable, IEnumerable<string> arguments, string operation)
    {
        int length = WindowsCommandLineLength(executable, arguments);
        if (length > MaxWindowsCommandLineLength)
            throw new InvalidOperationException($"{operation} command line is {length} characters; Windows limit guard is {MaxWindowsCommandLineLength}. Reduce the prompt or rules content.");
        return length;
    }

    static string QuoteWindowsArgument(string value)
    {
        if (value.Length > 0 && !value.Any(char.IsWhiteSpace) && !value.Contains('"')) return value;
        var result = new System.Text.StringBuilder("\"");
        int slashes = 0;
        foreach (char c in value)
        {
            if (c == '\\') { slashes++; continue; }
            if (c == '"')
            {
                result.Append('\\', slashes * 2 + 1).Append('"');
                slashes = 0;
                continue;
            }
            result.Append('\\', slashes).Append(c);
            slashes = 0;
        }
        result.Append('\\', slashes * 2).Append('"');
        return result.ToString();
    }
}

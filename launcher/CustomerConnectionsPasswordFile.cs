using System.Text;
using System.Text.Json;

namespace PriorityAgent;

/// <summary>Write/update C:\passwords\&lt;client&gt;-connections.txt from OData + session discoveries (FR #813).</summary>
public sealed class CustomerConnectionsPasswordFile
{
    readonly string _passwordsDir;
    readonly string _clientKey;

    public CustomerConnectionsPasswordFile(string passwordsDir, string clientKey)
    {
        _passwordsDir = passwordsDir;
        _clientKey = clientKey.Trim().ToLowerInvariant();
    }

    public string FilePath => Path.Combine(_passwordsDir, _clientKey + "-connections.txt");

    public CustomerConnectionsWriteResult WriteFromOData(JsonElement customerPayload)
    {
        Directory.CreateDirectory(_passwordsDir);
        var active = ODataCustomerConnections.ActiveRowsFromCustomerPayload(customerPayload);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# priority-agent connections (active OData rows only)");
        sb.AppendLine("# source: ZEMG_CUSTCONECT_SUBFORM / ZEMG_CUSTCONECTD_SUBFORM");
        int n = 0;
        foreach (JsonElement row in active)
        {
            string section = row.TryGetProperty("CONNNAME", out JsonElement cn) ? cn.GetString() ?? ("conn" + n) : "conn" + n;
            sb.Append(ODataCustomerConnections.FormatRowForPasswordFile(section, row));
            n++;
        }
        File.WriteAllText(FilePath, sb.ToString());
        return new CustomerConnectionsWriteResult(FilePath, active.Count, redactedLog: "wrote " + active.Count + " active row(s) to " + FilePath);
    }

    /// <summary>Append session-surfaced route details (airc / WCF / OData / PAT) without logging secrets.</summary>
    public CustomerConnectionsWriteResult AppendSessionRoute(string routeKind, IReadOnlyDictionary<string, string> fields)
    {
        Directory.CreateDirectory(_passwordsDir);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine();
        sb.AppendLine("[session-" + routeKind + "]");
        foreach (var kv in fields)
            sb.AppendLine(kv.Key + "=" + kv.Value);
        File.AppendAllText(FilePath, sb.ToString());
        return new CustomerConnectionsWriteResult(FilePath, 1, redactedLog: "appended session " + routeKind + " block (" + fields.Count + " fields)");
    }
}

public sealed record CustomerConnectionsWriteResult(string Path, int RowCount, string redactedLog);
using System.Text.Json;

namespace PriorityAgent;

/// <summary>Parse ZEMG_CUSTCONECT* subform rows; keep only active connection rows (FR #813).</summary>
public static class ODataCustomerConnections
{
    public const string SubformConnect = "ZEMG_CUSTCONECT_SUBFORM";
    public const string SubformConnectD = "ZEMG_CUSTCONECTD_SUBFORM";

    public static bool IsActiveRow(JsonElement row)
    {
        foreach (string name in new[] { "ACTIVE", "BOOLACTIVE", "ISACTIVE", "CUSTACTIVE", "STAT" })
        {
            if (!row.TryGetProperty(name, out JsonElement v))
                continue;
            if (v.ValueKind == JsonValueKind.True) return true;
            if (v.ValueKind == JsonValueKind.False) return false;
            if (v.ValueKind == JsonValueKind.Number)
                return v.GetInt32() != 0;
            string s = v.GetString() ?? "";
            if (s.Equals("Y", StringComparison.OrdinalIgnoreCase)
                || s.Equals("YES", StringComparison.OrdinalIgnoreCase)
                || s.Equals("TRUE", StringComparison.OrdinalIgnoreCase)
                || s == "1")
                return true;
            if (s.Equals("N", StringComparison.OrdinalIgnoreCase)
                || s.Equals("NO", StringComparison.OrdinalIgnoreCase)
                || s.Equals("FALSE", StringComparison.OrdinalIgnoreCase)
                || s == "0")
                return false;
        }
        return true;
    }

    public static IReadOnlyList<JsonElement> ActiveRowsFromCustomerPayload(JsonElement customerPayload)
    {
        var list = new List<JsonElement>();
        CollectActive(customerPayload, SubformConnect, list);
        CollectActive(customerPayload, SubformConnectD, list);
        return list;
    }

    static void CollectActive(JsonElement root, string propertyName, List<JsonElement> sink)
    {
        if (!root.TryGetProperty(propertyName, out JsonElement sub) || sub.ValueKind != JsonValueKind.Array)
            return;
        foreach (JsonElement row in sub.EnumerateArray())
        {
            if (IsActiveRow(row))
                sink.Add(row);
        }
    }

    public static string FormatRowForPasswordFile(string sectionName, JsonElement row)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[" + sectionName + "]");
        foreach (var prop in row.EnumerateObject())
        {
            if (prop.Name.Equals("PASSWORD", StringComparison.OrdinalIgnoreCase)
                || prop.Name.Contains("SECRET", StringComparison.OrdinalIgnoreCase)
                || prop.Name.Contains("TOKEN", StringComparison.OrdinalIgnoreCase))
                sb.AppendLine(prop.Name + "=" + prop.Value.ToString());
            else
                sb.AppendLine(prop.Name + "=" + prop.Value.ToString());
        }
        return sb.ToString();
    }
}
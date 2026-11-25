namespace PriorityAgent;

/// <summary>Never invent OData URLs; only composed paths on allowlisted bases (FR #813).</summary>
public static class ODataAllowlist
{
    public static readonly string[] DefaultBases =
    {
        "https://p.priority-connect.online/odata/Priority",
    };

    public static string? ResolveBase()
    {
        string? env = Environment.GetEnvironmentVariable("PRIORITY_AGENT_ODATA_BASE");
        if (!string.IsNullOrWhiteSpace(env))
            env = env.Trim().TrimEnd('/');
        string? pick = string.IsNullOrWhiteSpace(env) ? DefaultBases[0] : env;
        return IsAllowed(pick) ? pick : null;
    }

    public static bool IsAllowed(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) return false;
        string norm = baseUrl.Trim().TrimEnd('/');
        foreach (string allowed in DefaultBases)
        {
            if (string.Equals(norm, allowed.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                return true;
        }
        string? extra = Environment.GetEnvironmentVariable("PRIORITY_AGENT_ODATA_BASE_ALLOW");
        if (string.IsNullOrWhiteSpace(extra)) return false;
        foreach (string part in extra.Split(';', ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (string.Equals(norm, part.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public static string BuildCustomerEntityUrl(string baseUrl, string company, string medatechCustNo)
    {
        if (!IsAllowed(baseUrl))
            throw new InvalidOperationException("OData base is not on the allowlist.");
        string b = baseUrl.TrimEnd('/');
        string cust = medatechCustNo.Replace("'", "''", StringComparison.Ordinal);
        return $"{b}/tabula.ini/{company}/CUSTOMERS('{cust}')";
    }
}
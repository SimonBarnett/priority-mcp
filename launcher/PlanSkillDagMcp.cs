namespace PriorityAgent;

/// <summary>Plan mode + Plan skill DAG/MCP at plan.ntsa.uk (FR #813).</summary>
public static class PlanSkillDagMcp
{
    public const string PlanMcpUrl = "https://plan.ntsa.uk/mcp";
    public const string CustomerSetupSkillId = "priority-customer-setup";

    public static bool UsesPlanMcp(string? configuredMcpUrl, bool planMode)
    {
        if (!planMode) return false;
        if (string.IsNullOrWhiteSpace(configuredMcpUrl)) return false;
        return configuredMcpUrl.Trim().Equals(PlanMcpUrl, StringComparison.OrdinalIgnoreCase)
               || configuredMcpUrl.Contains("plan.ntsa.uk", StringComparison.OrdinalIgnoreCase);
    }

    public static string ResolvePlanMcpFromEnvironment()
    {
        string? env = Environment.GetEnvironmentVariable("PRIORITY_AGENT_PLAN_MCP");
        return string.IsNullOrWhiteSpace(env) ? PlanMcpUrl : env.Trim();
    }

    public static bool PlanModeFromEnvironment()
    {
        string? v = Environment.GetEnvironmentVariable("PRIORITY_AGENT_PLAN_MODE");
        return string.Equals(v, "1", StringComparison.Ordinal)
               || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase)
               || string.Equals(v, "plan", StringComparison.OrdinalIgnoreCase);
    }
}
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PriorityAgent;

/// <summary>
/// FR #812 (FR-P2): a new subfolder starts Plan mode against the Plan MCP,
/// then POSTs the task to Bob /bob/v1/gitcust. It is not a generic worker seat.
/// </summary>
public static class NewSubfolderPlan
{
    public const string PlanMcpName = "plan";
    public const string PlanMcpUrl = "https://plan.ntsa.uk/mcp";
    public const string GitcustPath = "/bob/v1/gitcust";
    public const string DefaultBobOrigin = "https://bob.ntsa.uk";
    public const string Mode = "plan";

    public static readonly string[] Kinds = { "SC", "SO", "WBS" };

    public static bool TryParseKind(string? raw, out string kind)
    {
        kind = (raw ?? "").Trim().ToUpperInvariant();
        if (kind is "SC" or "SO" or "WBS")
            return true;
        kind = "";
        return false;
    }

    public static string BuildPayload(string customerRepo, string kind, string title)
    {
        if (!TryParseKind(kind, out string parsed))
            throw new ArgumentException("kind must be SC, SO, or WBS", nameof(kind));
        if (string.IsNullOrWhiteSpace(customerRepo))
            throw new ArgumentException("customer repo is required", nameof(customerRepo));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("title is required", nameof(title));
        return JsonSerializer.Serialize(new
        {
            action = "new_subfolder",
            mode = Mode,
            customer = customerRepo.Trim(),
            kind = parsed,
            title = title.Trim(),
            plan_mcp = PlanMcpUrl,
            plan_mcp_name = PlanMcpName,
            worker_seat = false,
        });
    }

    /// <summary>Operator-facing brief. Plan skill DAG via Plan MCP; never a worker seat.</summary>
    public static string BuildPlanBrief(string customerRepo, string kind, string title)
    {
        if (!TryParseKind(kind, out string parsed))
            throw new ArgumentException("kind must be SC, SO, or WBS", nameof(kind));
        return "PLAN MODE. Load the Plan skill DAG from MCP '" + PlanMcpName + "' at " + PlanMcpUrl
            + ". This is not a generic worker seat. New subfolder for " + customerRepo.Trim()
            + " kind " + parsed + " title \"" + title.Trim() + "\"."
            + " The task was POSTed to " + GitcustPath + ". Follow the Plan skill. Do not start a bob worker seat.";
    }

    public static string[] GrokAddPlanMcpArgs() =>
        new[] { "mcp", "add", "--transport", "http", PlanMcpName, PlanMcpUrl };

    public readonly record struct GitcustPostResult(bool Ok, int Status, string Body, string Url);

    public static GitcustPostResult Post(string origin, string payload, HttpMessageHandler? handler = null)
    {
        string url = origin.TrimEnd('/') + GitcustPath;
        bool own = handler == null;
        HttpMessageHandler used = handler ?? new HttpClientHandler();
        try
        {
            using var http = new HttpClient(used, disposeHandler: own) { Timeout = TimeSpan.FromSeconds(12) };
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };
            using HttpResponseMessage resp = http.Send(req);
            string body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return new GitcustPostResult(resp.IsSuccessStatusCode, (int)resp.StatusCode, body, url);
        }
        finally
        {
            if (!own)
            {
                // HttpClient does not dispose a caller-owned handler when disposeHandler is false.
            }
        }
    }
}

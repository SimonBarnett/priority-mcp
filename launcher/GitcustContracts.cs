using System.Text.Json;
namespace PriorityAgent;

/// <summary>FR #813 / bobiverse #9982: Bob /bob/v1/gitcust GET+POST shapes (client side).</summary>
public static class GitcustContracts
{
    public const string DefaultEndpoint = "https://bob.ntsa.uk/bob/v1/gitcust";

    public static string ResolveEndpoint()
    {
        string? env = Environment.GetEnvironmentVariable("PRIORITY_AGENT_GITCUST_URL");
        return string.IsNullOrWhiteSpace(env) ? DefaultEndpoint : env.Trim();
    }

    public static JsonDocument BuildPostCustomer(string customerKey, string repoFullName, string medatechCustNo, string? title = null)
    {
        var obj = new Dictionary<string, object?>
        {
            ["op"] = "register",
            ["kind"] = "customer",
            ["customer"] = customerKey,
            ["repo"] = repoFullName,
            ["medatechCust"] = medatechCustNo,
        };
        if (!string.IsNullOrWhiteSpace(title))
            obj["title"] = title;
        return JsonDocument.Parse(JsonSerializer.Serialize(obj));
    }

    public static JsonDocument BuildPostFirstSubfolder(string customerKey, string kind, string folderName, string title)
    {
        var obj = new Dictionary<string, object?>
        {
            ["op"] = "register",
            ["kind"] = kind,
            ["customer"] = customerKey,
            ["path"] = folderName,
            ["title"] = title,
        };
        return JsonDocument.Parse(JsonSerializer.Serialize(obj));
    }
}

public sealed record GitcustPostRecord(string Label, string BodyWithoutSecrets, int Order);
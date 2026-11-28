using System.Text.Json;

namespace PriorityAgent;

/// <summary>
/// Customer and task folders from Bob GET /bob/v1/gitcust.
/// Same rows the launcher used to take from the priority-customer topic and clone trees.
/// </summary>
public static class GitcustCatalog
{
    public const string DefaultUrl = "https://bob.ntsa.uk/bob/v1/gitcust";
    public const string UrlEnv = "PRIORITY_AGENT_GITCUST_URL";

    public sealed record Snapshot(IReadOnlyList<Customer> Customers, IReadOnlyList<TaskRow> Tasks);

    public sealed record TaskRow(string Customer, string Kind, string Title, string Path, string Folder);

    public static Snapshot? TryLoad()
    {
        try
        {
            string url = Environment.GetEnvironmentVariable(UrlEnv);
            if (string.IsNullOrWhiteSpace(url)) url = DefaultUrl;
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
            string body = http.GetStringAsync(url).GetAwaiter().GetResult();
            return Parse(body);
        }
        catch
        {
            return null;
        }
    }

    public static Snapshot Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var customers = new List<Customer>();
        if (root.TryGetProperty("customers", out var custs) && custs.ValueKind == JsonValueKind.Array)
        {
            foreach (var c in custs.EnumerateArray())
            {
                string name = c.TryGetProperty("name", out var n) ? (n.GetString() ?? "") : "";
                if (name.Length == 0) continue;
                customers.Add(new Customer(name, "SimonBarnett/" + name, "main", ""));
            }
        }
        var tasks = new List<TaskRow>();
        if (root.TryGetProperty("tasks", out var taskEl) && taskEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var t in taskEl.EnumerateArray())
            {
                string customer = t.TryGetProperty("customer", out var cn) ? (cn.GetString() ?? "") : "";
                string kind = t.TryGetProperty("kind", out var k) ? (k.GetString() ?? "") : "";
                string title = t.TryGetProperty("title", out var ti) ? (ti.GetString() ?? "") : "";
                string path = t.TryGetProperty("path", out var p) ? (p.GetString() ?? "") : "";
                string folder = path;
                int slash = path.LastIndexOf('/');
                if (slash >= 0 && slash < path.Length - 1) folder = path[(slash + 1)..];
                if (customer.Length == 0 || folder.Length == 0) continue;
                tasks.Add(new TaskRow(customer, kind, title.Length == 0 ? folder : title, path, folder));
            }
        }
        customers.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        return new Snapshot(customers, tasks);
    }

    public static List<string> Folders(Snapshot snap, string customer)
    {
        return snap.Tasks
            .Where(t => string.Equals(t.Customer, customer, StringComparison.OrdinalIgnoreCase))
            .Select(t => t.Folder)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PriorityAgent;

/// <summary>HTTP client for Bob GET/POST /bob/v1/gitcust.</summary>
public sealed class GitcustClient
{
    readonly HttpClient _http;
    readonly string _endpoint;
    readonly Action<string>? _log;

    public GitcustClient(HttpClient? http = null, string? endpoint = null, Action<string>? log = null)
    {
        _http = http ?? new HttpClient();
        _endpoint = endpoint ?? GitcustContracts.ResolveEndpoint();
        _log = log;
    }

    public string Endpoint => _endpoint;

    public async Task<string> GetCatalogJsonAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync(_endpoint, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        _log?.Invoke("gitcust GET ok (body length " + body.Length + ")");
        return body;
    }

    public async Task<HttpResponseMessage> PostAsync(JsonDocument body, CancellationToken cancellationToken = default)
    {
        string json = body.RootElement.GetRawText();
        string redacted = GitcustRedaction.RedactPostBodyForLog(json);
        _log?.Invoke("gitcust POST " + redacted);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _http.PostAsync(_endpoint, content, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>Strip secret-like keys from POST JSON before any log line (FR #813).</summary>
public static class GitcustRedaction
{
    static readonly string[] SecretKeys = { "password", "token", "secret", "pat", "access_token" };

    public static string RedactPostBodyForLog(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return RedactElement(doc.RootElement);
        }
        catch
        {
            return "(unparseable body)";
        }
    }

    static string RedactElement(JsonElement el)
    {
        if (el.ValueKind != JsonValueKind.Object)
            return el.GetRawText();
        var parts = new List<string>();
        foreach (var prop in el.EnumerateObject())
        {
            if (SecretKeys.Any(k => prop.Name.Contains(k, StringComparison.OrdinalIgnoreCase)))
                parts.Add($"\"{prop.Name}\":\"***\"");
            else
                parts.Add($"\"{prop.Name}\":{prop.Value.GetRawText()}");
        }
        return "{" + string.Join(",", parts) + "}";
    }
}
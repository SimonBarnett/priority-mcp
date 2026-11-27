using System.Net.Http.Headers;
using System.Text;

namespace PriorityAgent;

/// <summary>Fetch CUSTOMERS entity JSON from allowlisted OData base (FR #813).</summary>
public sealed class ODataCustomerFetcher
{
    readonly HttpClient _http;

    public ODataCustomerFetcher(HttpClient? http = null) => _http = http ?? new HttpClient();

    public async Task<string> FetchCustomerJsonAsync(string odataBase, string company, string medatechCustNo, string? basicUser, string? basicPassword, CancellationToken ct = default)
    {
        string url = ODataAllowlist.BuildCustomerEntityUrl(odataBase, company, medatechCustNo);
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(basicUser))
        {
            string cred = Convert.ToBase64String(Encoding.UTF8.GetBytes(basicUser + ":" + (basicPassword ?? "")));
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", cred);
        }
        using var resp = await _http.SendAsync(req, ct).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }
}
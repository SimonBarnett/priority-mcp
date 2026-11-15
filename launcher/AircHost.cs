namespace PriorityAgent;

/// <summary>FR #635: one airc console host row from the live IRC roster (via bob digest).</summary>
public readonly record struct AircHost(string Machine, string Nick, string Channel, string? DomainWorkgroup)
{
    public string DisplayRow
    {
        get
        {
            string host = string.IsNullOrWhiteSpace(Machine) ? Nick : Machine;
            string ch = string.IsNullOrWhiteSpace(Channel) ? "" : Channel.Trim();
            string dw = string.IsNullOrWhiteSpace(DomainWorkgroup) ? "" : DomainWorkgroup.Trim();
            if (dw.Length > 0 && ch.Length > 0)
                return $"{host}  {ch}  {dw}";
            if (ch.Length > 0)
                return $"{host}  {ch}";
            if (dw.Length > 0)
                return $"{host}  {dw}";
            return host;
        }
    }
}

/// <summary>FR #635: result of fetching the connected airc host roster.</summary>
public readonly record struct AircHostFetchResult(bool Ok, IReadOnlyList<AircHost> Hosts, string Detail);

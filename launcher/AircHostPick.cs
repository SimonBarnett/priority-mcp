namespace PriorityAgent;

/// <summary>
/// FR #635: map airc hosts to shared <see cref="ConsolePick.PickList"/> rows.
/// Choosing a host only records the choice (session use is FR #636).
/// </summary>
public static class AircHostPick
{
    public static List<string> BuildItems(IReadOnlyList<AircHost> hosts)
    {
        ArgumentNullException.ThrowIfNull(hosts);
        return hosts.Select(h => h.DisplayRow).ToList();
    }

    public static bool TryResolve(IReadOnlyList<AircHost> hosts, int? index, out AircHost host)
    {
        ArgumentNullException.ThrowIfNull(hosts);
        host = default;
        if (index is null) return false;
        int i = index.Value;
        if (i < 0 || i >= hosts.Count) return false;
        host = hosts[i];
        return true;
    }
}

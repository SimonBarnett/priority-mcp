namespace PriorityAgent;

/// <summary>
/// FR #635: <c>/airc</c> slash command — spinner while fetching, then shared PickList.
/// Selection is recorded only; session use is FR #636.
/// </summary>
public static class AircSlashCommand
{
    public const string EmptyMessage = "No airc hosts are connected on IRC right now.";
    public const string CancelMessage = "Cancelled — no airc host selected.";
    public const string PickPrompt = "Pick airc host:";

    public static int Run(
        Func<AircHostFetchResult>? fetch = null,
        IConsolePickIO? pickIo = null,
        IConsoleWaitIO? waitIo = null,
        Action<AircHost>? save = null,
        Action<string>? writeLine = null)
    {
        writeLine ??= Console.WriteLine;
        save ??= h => AircSelection.Save(h);

        AircHostFetchResult fetched = AircHostFetch.Run(fetch, waitIo);
        if (!fetched.Ok)
        {
            writeLine(fetched.Detail);
            return 2;
        }

        if (fetched.Hosts.Count == 0)
        {
            writeLine(EmptyMessage);
            return 0;
        }

        var items = AircHostPick.BuildItems(fetched.Hosts);
        int? idx = ConsolePick.PickList(items, PickPrompt, pickIo);
        if (!AircHostPick.TryResolve(fetched.Hosts, idx, out AircHost host))
        {
            writeLine(CancelMessage);
            return 0;
        }

        save(host);
        writeLine($"Selected airc host {host.Machine} ({host.Nick}) on {host.Channel}. Session use is a later FR.");
        return 0;
    }
}

namespace PriorityAgent;

/// <summary>
/// FR #611: project / service-call folder list for ConsolePick.PickList.
/// Rows: [0] customer level only, then folders, then New (Haitch).
/// --project and --customer non-interactive paths stay outside this helper.
/// </summary>
public static class ProjectFolderPick
{
    public const string CustomerLevelLabel = "Customer level only (no project)";
    public const string NewProjectLabel = "New project / service call...";

    public static List<string> BuildItems(IReadOnlyList<string> folders)
    {
        ArgumentNullException.ThrowIfNull(folders);
        var items = new List<string>(folders.Count + 2) { CustomerLevelLabel };
        items.AddRange(folders);
        items.Add(NewProjectLabel);
        return items;
    }

    /// <summary>
    /// Map a PickList 0-based index. Returns false on null/out-of-range (caller treats as cancel).
    /// When true: haitchNew means the New row; otherwise project is null (customer level) or a folder name.
    /// </summary>
    public static bool TryResolve(IReadOnlyList<string> folders, int? index, out string? project, out bool haitchNew)
    {
        ArgumentNullException.ThrowIfNull(folders);
        project = null;
        haitchNew = false;
        if (index is null)
            return false;
        int i = index.Value;
        int last = folders.Count + 1;
        if (i < 0 || i > last)
            return false;
        if (i == 0)
            return true;
        if (i == last)
        {
            haitchNew = true;
            return true;
        }
        project = folders[i - 1];
        return true;
    }
}

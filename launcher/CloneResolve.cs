namespace PriorityAgent;

public enum CloneKind
{
    Missing,
    Stub,
    WrongOrigin,
    Dirty,
    Clean,
    /// <summary>FR #792: git remote/status probe exceeded the per-root timeout.</summary>
    TimedOut,
}

public sealed record CloneSighting(string Path, CloneKind Kind);

public static class CloneResolve
{
    public static bool IsUsable(CloneKind kind) =>
        kind is CloneKind.Clean or CloneKind.Dirty;

    public static string Describe(CloneSighting sighting) => sighting.Kind switch
    {
        CloneKind.Missing => $"{sighting.Path}: missing",
        CloneKind.Stub => $"{sighting.Path}: empty stub (no .git)",
        CloneKind.WrongOrigin => $"{sighting.Path}: exists but origin is not this repo",
        CloneKind.Dirty => $"{sighting.Path}: valid clone with uncommitted changes",
        CloneKind.Clean => $"{sighting.Path}: valid clean clone",
        CloneKind.TimedOut => $"{sighting.Path}: git probe timed out",
        _ => sighting.Path,
    };

    public static string? SelectUsable(IReadOnlyList<CloneSighting> sightings, out string? chosenReason)
    {
        foreach (var s in sightings)
        {
            if (IsUsable(s.Kind))
            {
                chosenReason = s.Kind == CloneKind.Dirty
                    ? "using dirty clone (skip pull)"
                    : "using clean clone";
                return s.Path;
            }
        }
        chosenReason = null;
        return null;
    }

    /// <summary>
    /// FR #792: walk ordered roots, probe each path, stop after the preferred
    /// clone-root path yields Clean|Dirty (do not keep probing slower roots).
    /// </summary>
    public static List<CloneSighting> CollectSightings(
        IEnumerable<string> roots,
        string cloneRoot,
        string repoFolderName,
        Func<string, CloneSighting> probe,
        bool shortCircuitPreferredUsable = true)
    {
        string preferred = Path.GetFullPath(Path.Combine(cloneRoot, repoFolderName));
        var sightings = new List<CloneSighting>();
        foreach (var root in roots)
        {
            string d = Path.GetFullPath(Path.Combine(root, repoFolderName));
            CloneSighting hit = probe(d);
            sightings.Add(hit);
            if (shortCircuitPreferredUsable
                && IsUsable(hit.Kind)
                && string.Equals(hit.Path, preferred, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
        }
        return sightings;
    }


    public static string FatalUnavailable(string fullName, IReadOnlyList<CloneSighting> sightings)
    {
        string listed = sightings.Count == 0
            ? "(no candidate paths searched)"
            : string.Join("; ", sightings.Select(Describe));
        return $"priority-mcp clone unavailable for {fullName}. Candidates: {listed}. Rename/remove an empty stub, or use --root / PRIORITY_AGENT_ROOT pointing at a real clone.";
    }
}

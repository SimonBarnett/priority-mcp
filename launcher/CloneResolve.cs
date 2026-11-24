namespace PriorityAgent;

public enum CloneKind
{
    Missing,
    Stub,
    WrongOrigin,
    Dirty,
    Clean,
}

public sealed record CloneSighting(string Path, CloneKind Kind);

public static class CloneResolve
{
    public static string Describe(CloneSighting sighting) => sighting.Kind switch
    {
        CloneKind.Missing => $"{sighting.Path}: missing",
        CloneKind.Stub => $"{sighting.Path}: empty stub (no .git)",
        CloneKind.WrongOrigin => $"{sighting.Path}: exists but origin is not this repo",
        CloneKind.Dirty => $"{sighting.Path}: valid clone with uncommitted changes",
        CloneKind.Clean => $"{sighting.Path}: valid clean clone",
        _ => sighting.Path,
    };

    /// <summary>
    /// FR #790: UNC (\\server\share) or a mapped network drive letter.
    /// Inject <paramref name="getDriveType"/> in tests.
    /// </summary>
    public static bool IsRemotePath(string path, Func<char, DriveType>? getDriveType = null)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        string trimmed = path.Trim();
        if (trimmed.StartsWith(@"\\", StringComparison.Ordinal) || trimmed.StartsWith("//", StringComparison.Ordinal))
            return true;
        string? root;
        try { root = Path.GetPathRoot(Path.GetFullPath(trimmed)); }
        catch { return false; }
        if (string.IsNullOrEmpty(root) || root.Length < 2 || root[1] != ':') return false;
        char letter = char.ToUpperInvariant(root[0]);
        if (letter < 'A' || letter > 'Z') return false;
        try
        {
            getDriveType ??= static c =>
            {
                try { return new DriveInfo(c + @":\").DriveType; }
                catch { return DriveType.Unknown; }
            };
            return getDriveType(letter) == DriveType.Network;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// First Clean|Dirty sighting. FR #790: skip UNC/network paths unless
    /// <paramref name="allowRemote"/> is true (last-resort only).
    /// </summary>
    public static string? SelectUsable(
        IReadOnlyList<CloneSighting> sightings,
        out string? chosenReason,
        bool allowRemote = false,
        Func<string, bool>? isRemote = null)
    {
        isRemote ??= static p => IsRemotePath(p);
        foreach (var s in sightings)
        {
            if (!allowRemote && isRemote(s.Path)) continue;
            if (s.Kind is CloneKind.Clean or CloneKind.Dirty)
            {
                chosenReason = s.Kind == CloneKind.Dirty
                    ? "using dirty clone (skip pull)"
                    : "using clean clone";
                if (allowRemote && isRemote(s.Path))
                    chosenReason += " (remote/UNC last resort)";
                return s.Path;
            }
        }
        chosenReason = null;
        return null;
    }

    /// <summary>
    /// FR #790: when CloneRoot tip is vacant, clone there even if a UNC sighting
    /// was usable — never operate the session on the share when a local tip can be minted.
    /// </summary>
    public static bool ShouldCloneToPreferred(
        string? selectedLocalPath,
        bool preferredVacant,
        bool remoteUsableExists)
    {
        if (!preferredVacant) return false;
        if (selectedLocalPath != null) return false;
        // Vacant preferred + no local usable → clone (remote may or may not exist).
        return true;
    }

    public static string FatalUnavailable(string fullName, IReadOnlyList<CloneSighting> sightings)
    {
        string listed = sightings.Count == 0
            ? "(no candidate paths searched)"
            : string.Join("; ", sightings.Select(Describe));
        return $"priority-mcp clone unavailable for {fullName}. Candidates: {listed}. Rename/remove an empty stub, or use --root / PRIORITY_AGENT_ROOT pointing at a real clone. UNC/network sightings are not used when CloneRoot can receive a fresh clone (FR #790).";
    }
}

// FR #668: per-user Desktop shortcut for the shipped PriorityAgent.exe path.
using System.Runtime.InteropServices;

namespace PriorityAgent;

/// <summary>Creates, updates, and removes the per-user Desktop shortcut.</summary>
static class DesktopShortcut
{
    public const string ShortcutFileName = "Priority Agent.lnk";
    public const string Description = "Priority ERP coding agent (Grok) with priority-mcp skills";

    /// <summary>Write a Windows .lnk. Tests inject a fake; production uses WScript.Shell.</summary>
    public interface IWriter
    {
        void Write(string linkPath, string targetExe, string workingDirectory, string iconLocation, string description);
    }

    public sealed record Result(string LinkPath, string TargetExe, string WorkingDirectory);

    public static string ResolveDesktopDirectory(string? desktopDir = null)
    {
        if (!string.IsNullOrWhiteSpace(desktopDir))
            return Path.GetFullPath(desktopDir);
        string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("Desktop directory is unavailable.");
        return path;
    }

    public static string LinkPath(string? desktopDir = null)
        => Path.Combine(ResolveDesktopDirectory(desktopDir), ShortcutFileName);

    /// <summary>
    /// Create or overwrite the single Desktop shortcut so a second install is idempotent.
    /// </summary>
    public static Result Ensure(string targetExe, string? desktopDir = null, IWriter? writer = null)
    {
        if (string.IsNullOrWhiteSpace(targetExe))
            throw new ArgumentException("target exe is required", nameof(targetExe));
        string full = Path.GetFullPath(targetExe);
        string work = Path.GetDirectoryName(full) ?? full;
        string link = LinkPath(desktopDir);
        Directory.CreateDirectory(Path.GetDirectoryName(link)!);
        (writer ?? ComWriter.Instance).Write(link, full, work, full + ",0", Description);
        return new Result(link, full, work);
    }

    /// <summary>Remove the Desktop shortcut if present. Returns true when a file was deleted.</summary>
    public static bool Remove(string? desktopDir = null)
    {
        string link = LinkPath(desktopDir);
        if (!File.Exists(link))
            return false;
        File.Delete(link);
        return true;
    }

    sealed class ComWriter : IWriter
    {
        public static readonly ComWriter Instance = new();

        public void Write(string linkPath, string targetExe, string workingDirectory, string iconLocation, string description)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("Desktop shortcuts require Windows.");
            Type? shellType = Type.GetTypeFromProgID("WScript.Shell")
                ?? throw new InvalidOperationException("WScript.Shell COM progid is unavailable.");
            object shell = Activator.CreateInstance(shellType)
                ?? throw new InvalidOperationException("WScript.Shell could not be created.");
            try
            {
                object shortcut = shellType.InvokeMember(
                    "CreateShortcut",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    shell,
                    new object[] { linkPath })
                    ?? throw new InvalidOperationException("CreateShortcut returned null.");
                Type shortcutType = shortcut.GetType();
                void Set(string name, object value) =>
                    shortcutType.InvokeMember(name, System.Reflection.BindingFlags.SetProperty, null, shortcut, new[] { value });
                Set("TargetPath", targetExe);
                Set("WorkingDirectory", workingDirectory);
                Set("IconLocation", iconLocation);
                Set("Description", description);
                shortcutType.InvokeMember(
                    "Save",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    shortcut,
                    null);
            }
            finally
            {
                if (Marshal.IsComObject(shell))
                    Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}

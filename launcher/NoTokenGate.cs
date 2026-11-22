using System;
using System.Runtime.InteropServices;

namespace PriorityAgent
{
    /// <summary>PRI-02: fuel-empty means dialog and quit. The launcher must not start.</summary>
    internal static class NoTokenGate
    {
        internal const string Title = "Priority Agent";
        internal const string Body = "No tokens left on this box. Priority Agent will quit.";

        internal static bool ShouldQuit(int remaining)
        {
            return remaining <= 0;
        }

        /// <summary>Returns 2 when the box must quit, otherwise 0. show replaces the dialog in tests.</summary>
        internal static int Apply(int remaining, Action<string, string> show)
        {
            if (!ShouldQuit(remaining)) return 0;
            if (show != null) show(Title, Body);
            else ShowDialog();
            return 2;
        }

        internal static void ShowDialog()
        {
            MessageBox(IntPtr.Zero, Body, Title, 0x30);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
    }
}

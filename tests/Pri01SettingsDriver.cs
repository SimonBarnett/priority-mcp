using System;

namespace PriorityAgent
{
    static class Pri01SettingsDriver
    {
        static int Main()
        {
            string err;
            if (!PriorityStartSettings.TrySet("model", "gpt-5", out err)) return 2;
            if (!PriorityStartSettings.TrySet("context", "200000", out err)) return 2;
            if (!PriorityStartSettings.TrySet("effort", "low", out err)) return 2;
            string line = PriorityStartSettings.DryRunLine();
            if (line != "settings model=gpt-5 context=200000 effort=low") return 3;
            if (PriorityStartSettings.TrySet("model", "bad value", out err)) return 4;
            if (PriorityStartSettings.HelpText.IndexOf("--model", StringComparison.Ordinal) < 0) return 5;
            Console.WriteLine(line);
            return 0;
        }
    }
}

using System;

internal static class Pri02NoTokenDriver
{
    static int Main()
    {
        bool shown = false;
        int quit = PriorityAgent.NoTokenGate.Apply(0, delegate(string title, string body)
        {
            shown = title == PriorityAgent.NoTokenGate.Title && body.Length > 0;
        });
        int go = PriorityAgent.NoTokenGate.Apply(12, null);
        if (quit != 2 || !shown || go != 0) return 1;
        Console.WriteLine("dialog-quit no-start");
        return 0;
    }
}

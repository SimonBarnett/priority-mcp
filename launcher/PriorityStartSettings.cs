using System;

namespace PriorityAgent
{
    /// <summary>PRI-01: model, context, and effort flags for the Priority exe.</summary>
    static class PriorityStartSettings
    {
        internal static string Model = "";
        internal static string Context = "";
        internal static string Effort = "";

        internal static bool TrySet(string which, string value, out string error)
        {
            error = "";
            if (string.IsNullOrWhiteSpace(value) || value.IndexOfAny(new[] { ' ', '\t', '"', '\'', ';', '|', '&', '<', '>' }) >= 0)
            {
                error = which + " value is empty or has a shell metacharacter";
                return false;
            }
            if (which == "model") Model = value;
            else if (which == "context") Context = value;
            else if (which == "effort") Effort = value;
            else
            {
                error = "unknown setting " + which;
                return false;
            }
            return true;
        }

        internal static string DryRunLine()
        {
            return "settings model=" + Model + " context=" + Context + " effort=" + Effort;
        }

        internal const string HelpText =
            "  --model <id>          models-webhook model (PRI-01)\r\n" +
            "  --context <n>         models-webhook context (PRI-01)\r\n" +
            "  --effort <level>      models-webhook effort (PRI-01)\r\n";
    }
}

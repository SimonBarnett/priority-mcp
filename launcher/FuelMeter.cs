using System.Text.Json;

namespace PriorityAgent;

/// <summary>
/// FR #677: read Grok fuel remaining from the Bob digest machine row.
/// xAI weekly remaining is published as <c>pcent.grok-build</c> and <c>weekly</c>
/// (hosts without Cursor login have no Cursor Sand <c>grok-chat</c> bar).
/// Legacy <c>pcent.grok-chat</c> is still accepted as a fallback.
/// </summary>
public static class FuelMeter
{
    public const string KeyGrokBuild = "grok-build";
    public const string KeyWeekly = "weekly";
    public const string KeyGrokChatLegacy = "grok-chat";

    public readonly record struct Reading(double Percent, string Key);

    /// <summary>
    /// Prefer <c>pcent.grok-build</c>, then machine <c>weekly</c>, then legacy <c>pcent.grok-chat</c>.
    /// </summary>
    public static bool TryRead(JsonElement machine, out Reading reading)
    {
        reading = default;
        if (machine.ValueKind != JsonValueKind.Object)
            return false;

        if (machine.TryGetProperty("pcent", out var pc) && pc.ValueKind == JsonValueKind.Object)
        {
            if (TryNumber(pc, KeyGrokBuild, out double build))
            {
                reading = new Reading(build, KeyGrokBuild);
                return true;
            }
        }

        if (TryNumber(machine, KeyWeekly, out double weekly))
        {
            reading = new Reading(weekly, KeyWeekly);
            return true;
        }

        if (machine.TryGetProperty("pcent", out pc) && pc.ValueKind == JsonValueKind.Object
            && TryNumber(pc, KeyGrokChatLegacy, out double chat))
        {
            reading = new Reading(chat, KeyGrokChatLegacy);
            return true;
        }

        return false;
    }

    /// <summary>Locate <paramref name="machineName"/> under <c>machines</c> (case-insensitive).</summary>
    public static bool TryGetMachine(JsonElement root, string machineName, out JsonElement machine)
    {
        machine = default;
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("machines", out var ms)
            || ms.ValueKind != JsonValueKind.Object)
            return false;
        string want = (machineName ?? "").Trim().ToLowerInvariant();
        if (want.Length == 0)
            return false;
        if (ms.TryGetProperty(want, out machine) && machine.ValueKind == JsonValueKind.Object)
            return true;
        foreach (var prop in ms.EnumerateObject())
        {
            if (prop.Name.Equals(want, StringComparison.OrdinalIgnoreCase)
                && prop.Value.ValueKind == JsonValueKind.Object)
            {
                machine = prop.Value;
                return true;
            }
        }
        return false;
    }

    static bool TryNumber(JsonElement obj, string name, out double value)
    {
        value = 0;
        if (!obj.TryGetProperty(name, out var el))
            return false;
        if (el.ValueKind == JsonValueKind.Number)
        {
            value = el.GetDouble();
            return true;
        }
        if (el.ValueKind == JsonValueKind.String
            && double.TryParse(el.GetString(), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out value))
            return true;
        return false;
    }
}

namespace PriorityAgent;

/// <summary>
/// FR #610: customer list for ConsolePick.PickList.
/// Rows: [0] General, then customer names, then New (Haitch).
/// --customer and --general stay outside this helper.
/// </summary>
public static class CustomerPick
{
    public const string GeneralLabel = "General Priority only (no customer)";
    public const string NewCustomerLabel = "New customer...";

    public static List<string> BuildItems(IReadOnlyList<string> customerNames)
    {
        ArgumentNullException.ThrowIfNull(customerNames);
        var items = new List<string>(customerNames.Count + 2) { GeneralLabel };
        items.AddRange(customerNames);
        items.Add(NewCustomerLabel);
        return items;
    }

    /// <summary>
    /// Map a PickList 0-based index. Returns false on null/out-of-range (caller treats as cancel).
    /// When true: haitchNew means the New row; otherwise customerIndex is -1 (General) or a name index.
    /// </summary>
    public static bool TryResolve(IReadOnlyList<string> customerNames, int? index, out int customerIndex, out bool haitchNew)
    {
        ArgumentNullException.ThrowIfNull(customerNames);
        customerIndex = -1;
        haitchNew = false;
        if (index is null)
            return false;
        int i = index.Value;
        int last = customerNames.Count + 1;
        if (i < 0 || i > last)
            return false;
        if (i == 0)
            return true;
        if (i == last)
        {
            haitchNew = true;
            return true;
        }
        customerIndex = i - 1;
        return true;
    }
}

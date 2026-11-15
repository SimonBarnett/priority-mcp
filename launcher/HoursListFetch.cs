namespace PriorityAgent;

/// <summary>FR #619: hours list GET result. Failure is not an exception at the call site.</summary>
public readonly record struct HoursFetchResult(bool Ok, string Json, string Detail);

/// <summary>
/// FR #619: wrap the blocking hours list GET in ConsoleWait.WithWait.
/// A failed GET does not settle to "hours fetch done". Redirected output stays quiet.
/// </summary>
public static class HoursListFetch
{
    public const string Label = "hours fetch";

    public static HoursFetchResult Run(Func<HoursFetchResult> fetch, IConsoleWaitIO? io = null)
    {
        ArgumentNullException.ThrowIfNull(fetch);
        try
        {
            return ConsoleWait.WithWait(Label, () =>
            {
                HoursFetchResult result = fetch();
                if (!result.Ok)
                    throw new HoursFetchFailed(result.Detail);
                return result;
            }, io);
        }
        catch (HoursFetchFailed ex)
        {
            return new HoursFetchResult(false, "", ex.Message);
        }
    }

    sealed class HoursFetchFailed : Exception
    {
        public HoursFetchFailed(string detail) : base(detail ?? "") { }
    }
}

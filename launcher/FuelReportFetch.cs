namespace PriorityAgent;

/// <summary>FR #616: fuel report GET result. Failure is not an exception at the call site.</summary>
public readonly record struct FuelFetchResult(bool Ok, string Json, string Detail);

/// <summary>
/// FR #616: wrap the blocking Bob fuel report GET in ConsoleWait.WithWait.
/// A failed GET does not settle to "fuel report done". Redirected output stays quiet.
/// Continue-anyway prompt stays in the caller outside the spinner.
/// </summary>
public static class FuelReportFetch
{
    public const string Label = "fuel report";

    public static FuelFetchResult Run(Func<FuelFetchResult> fetch, IConsoleWaitIO? io = null)
    {
        ArgumentNullException.ThrowIfNull(fetch);
        try
        {
            return ConsoleWait.WithWait(Label, () =>
            {
                FuelFetchResult result = fetch();
                if (!result.Ok)
                    throw new FuelFetchFailed(result.Detail);
                return result;
            }, io);
        }
        catch (FuelFetchFailed ex)
        {
            return new FuelFetchResult(false, "", ex.Message);
        }
    }

    sealed class FuelFetchFailed : Exception
    {
        public FuelFetchFailed(string detail) : base(detail ?? "") { }
    }
}

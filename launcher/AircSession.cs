namespace PriorityAgent;

/// <summary>
/// FR #636: in-memory /airc host for this Priority Agent process only.
/// Not written to config. A new instance is a new session with no selection.
/// The PickList that fills this is FR #635.
/// </summary>
public sealed class AircSession
{
    public string? SelectedHost { get; private set; }

    public string StatusSuffix =>
        string.IsNullOrEmpty(SelectedHost) ? "" : " airc:" + SelectedHost;

    public void Select(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("host is required", nameof(host));
        string trimmed = host.Trim();
        if (trimmed.IndexOfAny(new[] { ' ', '\r', '\n', '\t' }) >= 0)
            throw new ArgumentException("host must be a single token", nameof(host));
        SelectedHost = trimmed;
    }

    /// <summary>Channel for a host. Generic: no estate names.</summary>
    public static string ChannelFor(string host) => "#" + host;

    public string Relay(IAircTransport transport, string operation)
    {
        ArgumentNullException.ThrowIfNull(transport);
        if (string.IsNullOrEmpty(SelectedHost))
            return transport.RelayUnselected(operation);

        if (!transport.IsConnected(SelectedHost))
        {
            throw new AircHostUnavailableException(
                SelectedHost + " is not on IRC. Run /airc to re-pick.");
        }
        return transport.Send(ChannelFor(SelectedHost), operation);
    }
}

public interface IAircTransport
{
    bool IsConnected(string host);
    string Send(string channel, string operation);
    string RelayUnselected(string operation);
}

public sealed class AircHostUnavailableException : Exception
{
    public AircHostUnavailableException(string message) : base(message) { }
}

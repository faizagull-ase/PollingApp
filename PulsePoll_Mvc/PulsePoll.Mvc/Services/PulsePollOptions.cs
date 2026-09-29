namespace PulsePoll.Mvc.Services;

public class PulsePollOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string HubPath { get; set; } = string.Empty;
    public string SignalRClientVersion { get; set; } = string.Empty;

    public string HubUrl => BaseUrl.TrimEnd('/') + HubPath;
}

namespace Kerko.Analytics;

public class RequestLog
{
    public long Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string? Emri { get; set; }
    public string? Mbiemri { get; set; }
    public string? NumriTarges { get; set; }
    public string? NumriTelefonit { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string ClientIp { get; set; } = string.Empty;
    public string UserAgentRaw { get; set; } = string.Empty;
    public string UserAgentSimplified { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int DurationMs { get; set; }
    public int? ResultCount { get; set; }
    public string RequestId { get; set; } = string.Empty;
}

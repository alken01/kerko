using System;

namespace Kerko.Models;

public class SearchLog
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string SearchType { get; set; } = string.Empty;
    public string SearchParams { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool WasSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
} 
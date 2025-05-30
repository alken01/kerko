namespace Kerko.Models;

public class KestrelConfig
{
    public EndpointConfig Endpoints { get; set; } = new();
}

public class EndpointConfig
{
    public HttpEndpointConfig Http { get; set; } = new();
}

public class HttpEndpointConfig
{
    public string Url { get; set; } = string.Empty;
}
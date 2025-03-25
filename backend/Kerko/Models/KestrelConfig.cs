namespace Kerko.Models;

public class KestrelConfig
{
    public EndpointConfig Endpoints { get; set; } = new();
}

public class EndpointConfig
{
    public HttpEndpointConfig Http { get; set; } = new();
    public HttpsEndpointConfig Https { get; set; } = new();
}

public class HttpEndpointConfig
{
    public string Url { get; set; } = string.Empty;
}

public class HttpsEndpointConfig
{
    public string Url { get; set; } = string.Empty;
    public CertificateConfig Certificate { get; set; } = new();
}

public class CertificateConfig
{
    public string Path { get; set; } = string.Empty;
    public string KeyPath { get; set; } = string.Empty;
} 
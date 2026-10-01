using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mkx.Templates.Application.Abstractions;

namespace Mkx.Templates.Application.Services;

// An optional provider-neutral webhook adapter. A 2xx response means accepted for delivery.
public sealed class SmsSender(HttpClient client, IConfiguration configuration, ILogger<SmsSender> logger) : ISmsSender
{
    public async Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(configuration["Sms:Endpoint"], UriKind.Absolute, out var endpoint) || endpoint.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("Configure an HTTPS Sms:Endpoint before enabling SMS workflows.");
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(new { phoneNumber, message })
        };
        var apiKey = configuration["Sms:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey)) request.Headers.Add("X-API-Key", apiKey);
        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            logger.LogWarning("SMS provider rejected a delivery request with status {StatusCode}.", (int)response.StatusCode);
        return response.IsSuccessStatusCode;
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mkx.Templates.Sdk.Shared.Exceptions;

public class HttpRequestFailedException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public HttpRequestFailedException(HttpStatusCode statusCode) : this(statusCode, "Request failed with status " + statusCode + ".") { }

    public static async Task<Exception> GetExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new HttpRequestAuthenticationFailedException(response.StatusCode);
        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new HttpRequestAuthorizationFailedException(response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var errors = new Dictionary<string, string[]>();
        string detail = null;
        try
        {
            using var json = JsonDocument.Parse(content);
            if (json.RootElement.ValueKind == JsonValueKind.Object && json.RootElement.TryGetProperty("detail", out var d)) detail = d.GetString();
            if (json.RootElement.ValueKind == JsonValueKind.Object && json.RootElement.TryGetProperty("errors", out var e))
                errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(e.GetRawText()) ?? errors;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException) { /* Non-problem responses use a safe generic message. */ }
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity)
            return new HttpRequestValidationException(response.StatusCode, errors, detail);
        return new HttpRequestFailedException(response.StatusCode, detail ?? "The request could not be completed.");
    }
}

public class HttpRequestAuthenticationFailedException(HttpStatusCode statusCode) : HttpRequestFailedException(statusCode);
public class HttpRequestAuthorizationFailedException(HttpStatusCode statusCode) : HttpRequestFailedException(statusCode);
public class HttpRequestValidationException(HttpStatusCode statusCode, Dictionary<string, string[]> errors, string detail = null) : HttpRequestFailedException(statusCode, detail ?? "The supplied information is invalid.")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}

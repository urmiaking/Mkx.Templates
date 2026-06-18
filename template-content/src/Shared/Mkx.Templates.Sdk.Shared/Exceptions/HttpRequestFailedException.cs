using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mkx.Templates.Sdk.Shared.Exceptions;

public class HttpRequestFailedException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; private set; } = statusCode;

    public HttpRequestFailedException(HttpStatusCode statusCode)
        : this(statusCode, "Request failed with status " + statusCode + ".")
    {
    }

    public static Exception GetException(HttpStatusCode statusCode, HttpResponseMessage response)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => new HttpRequestAuthenticationFailedException(statusCode),
            HttpStatusCode.Forbidden => new HttpRequestAuthorizationFailedException(statusCode),
            HttpStatusCode.BadRequest => new HttpRequestValidationException(statusCode, response),
            _ => new HttpRequestFailedException(statusCode)
        };
    }
}

public class HttpRequestAuthenticationFailedException(HttpStatusCode statusCode)
    : HttpRequestFailedException(statusCode);

public class HttpRequestAuthorizationFailedException(HttpStatusCode statusCode) : HttpRequestFailedException(statusCode);

public class HttpRequestValidationException : HttpRequestFailedException
{
    public Dictionary<string, string[]> Errors { get; private set; }

    public HttpRequestValidationException(HttpStatusCode statusCode, HttpResponseMessage response)
        : base(statusCode)
    {
        Errors = new Dictionary<string, string[]>();

        if (response != null)
            TryReadValidationProblems(response);
    }

    private void TryReadValidationProblems(HttpResponseMessage response)
    {
        string content = null;

        try
        {
            using (var stream = response.Content.ReadAsStreamAsync().Result)
            using (var reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var problems = JsonSerializer.Deserialize<ValidationProblemDetails>(content, jsonOptions);


            if (problems is { Errors: not null } && problems.Errors.Any())
            {
                Errors = problems.Errors;
                return;
            }
        }
        catch
        {
            // ignore, fallback below
        }

        // fallback - plain text
        if (!string.IsNullOrWhiteSpace(content))
        {
            Errors = new Dictionary<string, string[]>
            {
                { string.Empty, [content] }
            };
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Mkx.Templates.Sdk.Server.Api.Middlewares;

/// <summary>
/// Abstract handler for all exceptions.
/// </summary>
public abstract class ExceptionHandlerMiddlewareBase(ILogger logger, RequestDelegate next)
{
    /// <summary>
    /// This key should be used to store the exception in the <see cref="IDictionary{TKey,TValue}"/> of the exception data,
    /// to be localized in the abstract handler.
    /// </summary>
    public static string LocalizationKey => "LocalizationKey";

    /// <summary>
    /// Gets HTTP status code response and message to be returned to the caller.
    /// Use the ".Data" property to set the key of the messages if it's localized.
    /// </summary>
    /// <param name="exception">The actual exception</param>
    /// <returns>Tuple of HTTP status code and a message</returns>
    public abstract (HttpStatusCode code, string message) GetResponse(Exception exception);

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // mute
        }
        catch (Exception exception)
        {
            // log the error
            logger.LogError(exception, "An error occured during executing '{Context}'", context.Request.Path.Value);
            if (context.Response.HasStarted) throw;
            var response = context.Response;
            response.Clear();
            response.ContentType = "application/problem+json";
            response.Headers.CacheControl = "no-store";

            // get the response code and message
            var (status, message) = GetResponse(exception);
            response.StatusCode = (int)status;
            var problem = System.Text.Json.Nodes.JsonNode.Parse(message)!;
            problem["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.TraceIdentifier;
            problem["instance"] = context.Request.Path.Value;
            await response.WriteAsync(problem.ToJsonString(), context.RequestAborted);
        }
    }
}

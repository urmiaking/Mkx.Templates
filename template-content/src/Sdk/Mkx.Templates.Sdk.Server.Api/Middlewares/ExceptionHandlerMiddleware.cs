using FluentValidation;
using Mkx.Templates.Sdk.Server.Application.Exceptions;
using Mkx.Templates.Sdk.Server.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Mkx.Templates.Sdk.Server.Api.Middlewares;

public class ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger, RequestDelegate next)
    : ExceptionHandlerMiddlewareBase(logger, next)
{
    public override (HttpStatusCode code, string message) GetResponse(Exception exception)
    {
        var code = exception switch
        {
            NotFoundException or FileNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedException or UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            ForbiddenException => HttpStatusCode.Forbidden,
            BadRequestException or ArgumentException or ValidationException => HttpStatusCode.BadRequest,
            DbUpdateConcurrencyException => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };
        ProblemDetails problem = exception is ValidationException validation
            ? new ValidationProblemDetails(validation.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            : new ProblemDetails();
        problem.Status = (int)code;
        problem.Title = code.ToString();
        problem.Detail = code switch
        {
            HttpStatusCode.InternalServerError => "An unexpected error occurred. Please try again or contact support.",
            HttpStatusCode.Conflict => "This record was changed by another user. Reload it before saving.",
            _ => exception.Message
        };
        return (code, JsonSerializer.Serialize(problem, problem.GetType(), new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}

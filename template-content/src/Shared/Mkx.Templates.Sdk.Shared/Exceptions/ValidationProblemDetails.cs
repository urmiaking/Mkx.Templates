using System.Collections.Generic;

namespace Mkx.Templates.Sdk.Shared.Exceptions;

internal class ValidationProblemDetails
{
    public Dictionary<string, string[]> Errors { get; set; } = new();
}

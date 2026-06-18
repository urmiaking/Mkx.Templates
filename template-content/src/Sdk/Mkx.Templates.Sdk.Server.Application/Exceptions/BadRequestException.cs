using Mkx.Templates.Sdk.Server.Shared.Exceptions;

namespace Mkx.Templates.Sdk.Server.Application.Exceptions;

public class BadRequestException(string message) : ApplicationOperationException(message)
{
    public BadRequestException() : this("Bad request.")
    {

    }
}


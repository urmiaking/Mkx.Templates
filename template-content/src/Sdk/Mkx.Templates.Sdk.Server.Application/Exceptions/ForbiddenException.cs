using Mkx.Templates.Sdk.Server.Shared.Exceptions;

namespace Mkx.Templates.Sdk.Server.Application.Exceptions;

public class ForbiddenException(string message) : ApplicationOperationException(message)
{
    public ForbiddenException() : this("Forbidden")
    {
        
    }
}


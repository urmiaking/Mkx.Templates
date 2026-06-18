using Mkx.Templates.Sdk.Server.Shared.Exceptions;

namespace Mkx.Templates.Sdk.Server.Application.Exceptions;

public class UnauthorizedException(string message) : ApplicationOperationException(message)
{
    public UnauthorizedException() : this("Unauthorized.")
    {
        
    }
}


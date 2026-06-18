namespace Mkx.Templates.Sdk.Server.Shared.Exceptions;

public class NotFoundException(string message) : ApplicationOperationException(message)
{
    public NotFoundException() : this("Not found.")
    {
        
    }
}

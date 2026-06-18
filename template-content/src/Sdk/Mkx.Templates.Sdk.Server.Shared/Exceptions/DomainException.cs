namespace Mkx.Templates.Sdk.Server.Shared.Exceptions;

public class DomainException(string message) : ApplicationOperationException(message)
{
    public DomainException() : this("Domain exception occured.")
    {

    }
}

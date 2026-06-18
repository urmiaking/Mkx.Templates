namespace Mkx.Templates.Sdk.Server.Domain.Abstractions;

public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    void RemoveEvent(IDomainEvent e);
}


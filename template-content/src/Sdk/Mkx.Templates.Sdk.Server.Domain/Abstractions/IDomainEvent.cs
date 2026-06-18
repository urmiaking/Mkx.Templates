namespace Mkx.Templates.Sdk.Server.Domain.Abstractions;

public interface IDomainEvent;

public interface IPrePersistenceDomainEvent : IDomainEvent;
public interface IPostPersistenceDomainEvent : IDomainEvent;

using Mkx.Templates.Sdk.Server.Domain;

namespace Mkx.Templates.Sdk.Server.Infrastructure.Repositories;

public interface ICreateRepository<in TEntity> where TEntity : EntityBase
{
    void Create(TEntity entity);
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    void CreateRange(IEnumerable<TEntity> entities);
    Task CreateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}


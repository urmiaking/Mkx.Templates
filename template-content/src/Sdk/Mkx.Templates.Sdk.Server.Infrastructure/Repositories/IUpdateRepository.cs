using Mkx.Templates.Sdk.Server.Domain;

namespace Mkx.Templates.Sdk.Server.Infrastructure.Repositories;

public interface IUpdateRepository<in TEntity> where TEntity : EntityBase
{
    void Update(TEntity entity);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    void UpdateRange(IEnumerable<TEntity> entities);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}

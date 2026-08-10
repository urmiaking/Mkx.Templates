using Ardalis.Specification;
using Mkx.Templates.Sdk.Server.Domain;

namespace Mkx.Templates.Sdk.Server.Infrastructure.Repositories;

public interface IDeleteRepository<TEntity> where TEntity : EntityBase
{
    void Delete(TEntity entity);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    void DeleteRange(IEnumerable<TEntity> entities);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<int> ExecuteDeleteAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
}

using System.Data;
using System.Data.Common;
using Ardalis.Specification;
using Microsoft.EntityFrameworkCore.Storage;
using Mkx.Templates.Sdk.Server.Domain;

namespace Mkx.Templates.Sdk.Server.Infrastructure.Repositories;

public interface IRepository<TEntity> : IReadRepositoryBase<TEntity> where TEntity : EntityBase
{
    void AsNoTracking();

    void AsTracking();

    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);

    IDbContextTransaction? GetCurrentTransaction();

    Task<int> SaveAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction?> UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = default);

    IQueryable<TEntity> Get(ISpecification<TEntity> specification);

    Task<bool> ExistsAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
}

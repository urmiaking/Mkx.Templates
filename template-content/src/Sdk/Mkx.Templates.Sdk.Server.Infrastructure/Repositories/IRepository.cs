using System.Data;
using System.Data.Common;
using Mkx.Templates.Sdk.Server.Domain;
using Mkx.Templates.Sdk.Server.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mkx.Templates.Sdk.Server.Infrastructure.Repositories;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    void AsNoTracking();

    void AsTracking();

    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default(CancellationToken));

    IDbContextTransaction? GetCurrentTransaction();

    Task<int> SaveAsync(CancellationToken cancellationToken = default(CancellationToken));

    Task<IDbContextTransaction?> UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = default(CancellationToken));

    IQueryable<TEntity> Get(ISpecification<TEntity> specification);

    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default(CancellationToken));

    Task<bool> ExistsAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default(CancellationToken));
}

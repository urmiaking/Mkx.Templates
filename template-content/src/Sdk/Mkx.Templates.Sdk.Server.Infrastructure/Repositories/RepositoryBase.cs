using System.Data;
using System.Data.Common;
using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mkx.Templates.Sdk.Server.Domain;
using Mkx.Templates.Sdk.Server.Infrastructure.Extensions;

namespace Mkx.Templates.Sdk.Server.Infrastructure.Repositories;

public abstract class RepositoryBase<TEntity> :
    Ardalis.Specification.EntityFrameworkCore.RepositoryBase<TEntity>,
    IRepository<TEntity>,
    ICreateRepository<TEntity>,
    IUpdateRepository<TEntity>,
    IDeleteRepository<TEntity>
    where TEntity : EntityBase
{
    private readonly DbContext _context;

    protected RepositoryBase(DbContext context) : base(context)
    {
        _context = context;
    }

    protected bool Tracking { get; private set; } = true;

    public virtual IQueryable<TEntity> Query => _context.Set<TEntity>().SetTracking(Tracking);

    public virtual void AsNoTracking() => Tracking = false;

    public virtual void AsTracking() => Tracking = true;

    public virtual void Create(TEntity entity) => _context.Add(entity);

    public virtual async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Create(entity);
        await SaveAsync(cancellationToken);
    }

    public virtual void CreateRange(IEnumerable<TEntity> entities) => _context.AddRange(entities);

    public virtual async Task CreateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        CreateRange(entities);
        await SaveAsync(cancellationToken);
    }

    public virtual void Update(TEntity entity) => _context.Update(entity);

    public new virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Update(entity);
        await SaveAsync(cancellationToken);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities) => _context.UpdateRange(entities);

    public new virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        UpdateRange(entities);
        await SaveAsync(cancellationToken);
    }

    public virtual void Delete(TEntity entity) => _context.Remove(entity);

    public new virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Delete(entity);
        await SaveAsync(cancellationToken);
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities) => _context.RemoveRange(entities);

    public new virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        DeleteRange(entities);
        await SaveAsync(cancellationToken);
    }

    public virtual Task<int> SaveAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public virtual Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
      CancellationToken cancellationToken = default)
    {
        return _context.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public virtual Task<IDbContextTransaction?> UseTransactionAsync(
      DbTransaction transaction,
      CancellationToken cancellationToken = default)
    {
        return _context.Database.UseTransactionAsync(transaction, cancellationToken);
    }

    public virtual IDbContextTransaction? GetCurrentTransaction()
    {
        return _context.Database.CurrentTransaction;
    }

    public IQueryable<TEntity> Get(ISpecification<TEntity> specification)
    {
        return ApplySpecification(specification);
    }

    public Task<bool> ExistsAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return AnyAsync(specification, cancellationToken);
    }

    public virtual Task<int> ExecuteDeleteAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification, evaluateCriteriaOnly: true);
        return query.ExecuteDeleteAsync(cancellationToken);
    }
}

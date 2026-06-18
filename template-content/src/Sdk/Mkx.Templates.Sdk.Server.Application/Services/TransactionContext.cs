using Mkx.Templates.Sdk.Server.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mkx.Templates.Sdk.Server.Application.Services;

public class TransactionContext<T>(T dbContext) : ITransactionContext where T : DbContext
{
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Database.BeginTransactionAsync(cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore.Storage;

namespace Mkx.Templates.Sdk.Server.Application.Abstractions;

public interface ITransactionContext
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

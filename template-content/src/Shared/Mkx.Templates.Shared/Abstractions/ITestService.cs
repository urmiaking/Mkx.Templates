using Mkx.Templates.Shared.DTOs.Tests;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mkx.Templates.Shared.Abstractions;

public interface ITestService
{
    Task<List<GetTestResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GetTestResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
}

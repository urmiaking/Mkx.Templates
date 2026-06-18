using Mkx.Templates.Domain.TestAggregate;
using Mkx.Templates.Infrastructure.Repositories.Abstractions;
using Mkx.Templates.Sdk.Server.Infrastructure.Repositories;
using Mkx.Templates.Sdk.Shared.Attributes;

namespace Mkx.Templates.Infrastructure.Repositories;

[ScopedService]
internal sealed class TestRepository(AppDbContext dbContext) : RepositoryBase<Test>(dbContext), ITestRepository;

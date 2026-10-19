using Mkx.Templates.Domain.SampleItemAggregate;
using Mkx.Templates.Infrastructure.Repositories.Abstractions;
using Mkx.Templates.Sdk.Server.Infrastructure.Repositories;
using Mkx.Templates.Sdk.Shared.Attributes;

namespace Mkx.Templates.Infrastructure.Repositories;

[ScopedService]
internal sealed class SampleItemRepository(AppDbContext dbContext) : RepositoryBase<SampleItem>(dbContext), ISampleItemRepository;

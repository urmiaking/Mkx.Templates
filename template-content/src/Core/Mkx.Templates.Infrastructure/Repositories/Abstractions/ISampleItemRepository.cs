using Mkx.Templates.Domain.SampleItemAggregate;
using Mkx.Templates.Sdk.Server.Infrastructure.Repositories;

namespace Mkx.Templates.Infrastructure.Repositories.Abstractions;

public interface ISampleItemRepository : IRepository<SampleItem>,
    ICreateRepository<SampleItem>,
    IUpdateRepository<SampleItem>,
    IDeleteRepository<SampleItem>;

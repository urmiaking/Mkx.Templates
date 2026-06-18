using Mkx.Templates.Domain.TestAggregate;
using Mkx.Templates.Sdk.Server.Infrastructure.Repositories;

namespace Mkx.Templates.Infrastructure.Repositories.Abstractions;

public interface ITestRepository : IRepository<Test>,
    ICreateRepository<Test>,
    IUpdateRepository<Test>,
    IDeleteRepository<Test>;

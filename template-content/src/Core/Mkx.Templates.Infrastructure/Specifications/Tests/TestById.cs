using Mkx.Templates.Domain.TestAggregate;
using Mkx.Templates.Sdk.Server.Infrastructure.Specifications;

namespace Mkx.Templates.Infrastructure.Specifications.Tests;

public sealed class TestById(TestId id) : SpecificationBase<Test>(x => x.Id == id);

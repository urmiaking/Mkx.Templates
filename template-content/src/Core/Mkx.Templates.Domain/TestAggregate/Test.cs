using Mkx.Templates.Sdk.Server.Domain;

namespace Mkx.Templates.Domain.TestAggregate;

public readonly record struct TestId(Guid Value);

public class Test : EntityBase<TestId>
{
#pragma warning disable CS8618
    private Test() { }
#pragma warning restore CS8618

    private Test(string name, string? description)
    {
        Id = new TestId(Guid.CreateVersion7());
        Name = name;
        Description = description;
    }

    public static Test Create(string name, string? description) =>
        new(name, description);

    public string Name { get; private set; }
    public string? Description { get; private set; }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}

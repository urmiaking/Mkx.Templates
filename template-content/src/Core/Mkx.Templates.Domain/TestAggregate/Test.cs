using Mkx.Templates.Sdk.Server.Domain;

namespace Mkx.Templates.Domain.TestAggregate;

public readonly record struct TestId(Guid Value);

public class Test : EntityBase<TestId>
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 500;
    private Test() { }
    private Test(string name, string? description)
    {
        Id = new TestId(Guid.CreateVersion7());
        SetContent(name, description);
    }
    public static Test Create(string name, string? description) => new(name, description);
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid Version { get; private set; } = Guid.CreateVersion7();
    public void Update(string name, string? description)
    {
        SetContent(name, description);
        Version = Guid.CreateVersion7();
    }
    private void SetContent(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > MaxNameLength)
            throw new ArgumentException("A test name must contain 1 to 200 characters.", nameof(name));
        if (description?.Length > MaxDescriptionLength)
            throw new ArgumentException("A test description cannot exceed 500 characters.", nameof(description));
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}

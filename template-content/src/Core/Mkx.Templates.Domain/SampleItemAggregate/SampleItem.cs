using Mkx.Templates.Sdk.Server.Domain;

namespace Mkx.Templates.Domain.SampleItemAggregate;

public readonly record struct SampleItemId(Guid Value);

public class SampleItem : EntityBase<SampleItemId>
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 500;
    private SampleItem() { }
    private SampleItem(string name, string? description)
    {
        Id = new SampleItemId(Guid.CreateVersion7());
        SetContent(name, description);
    }
    public static SampleItem Create(string name, string? description) => new(name, description);
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
            throw new ArgumentException("A sample item name must contain 1 to 200 characters.", nameof(name));
        if (description?.Length > MaxDescriptionLength)
            throw new ArgumentException("A sample item description cannot exceed 500 characters.", nameof(description));
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}

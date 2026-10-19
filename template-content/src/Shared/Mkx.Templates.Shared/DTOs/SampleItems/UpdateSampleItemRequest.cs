namespace Mkx.Templates.Shared.DTOs.SampleItems;

public sealed record UpdateSampleItemRequest(string Name, string? Description, Guid Version);

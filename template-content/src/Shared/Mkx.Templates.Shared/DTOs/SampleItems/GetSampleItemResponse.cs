namespace Mkx.Templates.Shared.DTOs.SampleItems;

public sealed record GetSampleItemResponse(Guid Id, string Name, string? Description, Guid Version);

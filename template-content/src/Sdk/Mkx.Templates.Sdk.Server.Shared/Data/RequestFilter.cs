using Mkx.Templates.Sdk.Server.Shared.Enums;

namespace Mkx.Templates.Sdk.Server.Shared.Data;

public record RequestFilter(int? Skip = null, int? Take = null, string? Search = null, string? SortLabel = null, SortDirection? SortDirection = null)
{
    public int? Skip { get; set; } = Skip;
    public RequestFilter Normalize() => this with
    {
        Skip = Math.Max(0, Skip ?? 0),
        Take = Math.Clamp(Take ?? 25, 1, 100),
        Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim()[..Math.Min(Search.Trim().Length, 200)]
    };
}


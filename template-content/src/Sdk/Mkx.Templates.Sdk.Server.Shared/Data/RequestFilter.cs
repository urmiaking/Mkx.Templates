using Mkx.Templates.Sdk.Server.Shared.Enums;

namespace Mkx.Templates.Sdk.Server.Shared.Data;

public record RequestFilter(int? Skip = null, int? Take = null, string? Search = null, string? SortLabel = null, SortDirection? SortDirection = null)
{
    public int? Skip { get; set; } = Skip;
}


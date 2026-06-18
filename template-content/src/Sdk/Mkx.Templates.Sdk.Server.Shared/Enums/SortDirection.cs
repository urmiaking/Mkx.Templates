using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Mkx.Templates.Sdk.Server.Shared.Enums;

/// <summary>
/// Indicates the direction that search results are sorted by.
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// No sort direction.
    /// </summary>
    [Description("none")]
    [Display(Name = "بدون مرتب سازی")]
    None,

    /// <summary>
    /// Results are sorted in ascending order (A-Z).
    /// </summary>
    [Description("ascending")]
    [Display(Name = "صعودی")]
    Ascending,

    /// <summary>
    /// Results are sorted in descending order (Z-A).
    /// </summary>
    [Description("descending")]
    [Display(Name = "نزولی")]
    Descending,
}

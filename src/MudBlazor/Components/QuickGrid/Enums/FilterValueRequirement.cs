namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Specifies whether a <see cref="FilterOperatorDescriptor"/> requires a value
/// in <see cref="MudBlazor.Components.QuickGrid.GridFilter.Value"/> to produce a
/// meaningful predicate.
/// </summary>
internal enum FilterValueRequirement
{
    /// <summary>
    /// A non-null, non-empty <see cref="MudBlazor.Components.QuickGrid.GridFilter.Value"/>
    /// is required.  Filters with a missing value are skipped.
    /// </summary>
    Required,

    /// <summary>
    /// No value is needed.  The operator tests a condition that is inherent to the
    /// member itself (e.g. <c>IsNull</c>, <c>BooleanTrue</c>, <c>StringEmpty</c>).
    /// </summary>
    None,
}

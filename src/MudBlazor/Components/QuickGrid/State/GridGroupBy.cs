namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Describes a single grouping level within a <see cref="GridQuery"/>.
/// </summary>
/// <remarks>
/// When <see cref="GridQuery.Groups"/> is non-empty, <see cref="IGridDataSource{T}"/>
/// implementations must pre-sort results by the group keys in
/// <see cref="Priority"/> order before applying user-specified <see cref="GridSort"/>
/// instructions.  This guarantees that items belonging to the same group are always
/// consecutive within a paginated result, which is required by
/// <c>IGridGroupingService&lt;T&gt;.BuildGroupTree</c>.
/// </remarks>
public sealed record GridGroupBy
{
    /// <summary>
    /// Gets or initialises the stable column key to group by.
    /// Must match <c>ColumnDescriptor&lt;T&gt;.Key</c>.
    /// </summary>
    public required string ColumnKey { get; init; }

    /// <summary>
    /// Gets or initialises the zero-based nesting priority of this grouping level
    /// relative to other grouped columns.  Lower values produce the outermost groups.
    /// </summary>
    public int Priority { get; init; }
}

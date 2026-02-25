namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Describes a single column sort instruction within a <see cref="GridQuery"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GridSort"/> is intentionally non-generic and contains no expression
/// trees or compiled delegates.  It is a pure data record that can be:
/// <list type="bullet">
///   <item><description>JSON-serialised and stored as part of a <see cref="GridSnapshot"/>.</description></item>
///   <item><description>Sent over the wire to a remote API.</description></item>
///   <item><description>Translated to OData, GraphQL, or SQL by an <see cref="IGridDataSource{T}"/> adapter.</description></item>
/// </list>
/// </para>
/// <para>
/// Expression compilation — when needed — is the responsibility of the
/// <see cref="IGridDataSource{T}"/> implementation, not the grid core.
/// </para>
/// </remarks>
public sealed record GridSort
{
    /// <summary>
    /// Gets or initialises the stable column key this sort targets.
    /// Must match <c>ColumnDescriptor&lt;T&gt;.Key</c>.
    /// </summary>
    public required string ColumnKey { get; init; }

    /// <summary>Gets or initialises the sort direction.</summary>
    public required SortDirection Direction { get; init; }

    /// <summary>
    /// Gets or initialises the zero-based priority of this sort within a multi-column sort.
    /// Lower values are applied first (primary sort before secondary sort).
    /// </summary>
    public int Priority { get; init; }
}

using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Represents a single column sort instruction.  Multiple <see cref="SortDefinition{T}"/>
/// entries can coexist in <c>QuickGridState&lt;T&gt;.SortDefinitions</c> when
/// <c>SortMode.Multiple</c> is active.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed record SortDefinition<T>
{
    /// <summary>
    /// Gets or initialises the stable column identifier being sorted.
    /// Must match <c>QuickGridColumnBase&lt;T&gt;.Id</c>.
    /// </summary>
    public string ColumnId { get; init; } = "";

    /// <summary>
    /// Gets or initialises whether the sort is descending.
    /// <see langword="false"/> means ascending (the default).
    /// </summary>
    public bool Descending { get; init; }

    /// <summary>
    /// Gets or initialises the zero-based priority of this sort within a multi-column sort.
    /// Lower values are applied first (primary sort before secondary sort).
    /// </summary>
    public int SortIndex { get; init; }

    /// <summary>
    /// Gets or initialises the sort expression used by <c>IGridSortService&lt;T&gt;</c> to build
    /// an <c>IOrderedQueryable&lt;T&gt;</c>. Not serialized — reconstructed from column metadata
    /// when a <c>GridSnapshot&lt;T&gt;</c> is applied.
    /// </summary>
    public Expression<Func<T, object>>? SortExpression { get; init; }

    /// <summary>
    /// Gets or initialises an optional custom comparer for in-memory sorts. Takes precedence over
    /// <see cref="SortExpression"/> when both are provided for client-side enumerable sorts.
    /// </summary>
    public IComparer<object>? Comparer { get; init; }
}

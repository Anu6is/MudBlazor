namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Describes a single grouping column passed to
/// <c>IGridGroupingService&lt;T&gt;</c> methods.
/// Produced by <c>MudQuickGrid&lt;T&gt;</c> by inspecting the registered columns at refresh time.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed record GroupColumnDefinition<T>
{
    /// <summary>Gets or initialises the stable column identifier.</summary>
    public string ColumnId { get; init; } = "";

    /// <summary>
    /// Gets or initialises the key selector that extracts the grouping value from a row item.
    /// Maps to <c>PropertyColumn&lt;T, TProp&gt;.GroupBy</c>.
    /// </summary>
    public Func<T, object> GroupBy { get; init; } = _ => default!;

    /// <summary>
    /// Gets or initialises the sort priority of this grouping level relative to other grouped
    /// columns.  Lower values are the outermost (primary) group.
    /// Maps to <c>PropertyColumn&lt;T, TProp&gt;.GroupByOrder</c>.
    /// </summary>
    public int GroupByOrder { get; init; }
}

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// The canonical, non-generic query descriptor produced by <c>MudQuickGrid&lt;T&gt;</c>
/// and passed to <see cref="IGridDataSource{T}.QueryAsync"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GridQuery"/> describes <em>what</em> the grid wants; the
/// <see cref="IGridDataSource{T}"/> implementation decides <em>how</em> to retrieve it.
/// The grid core never executes sorts, filters, or expressions directly.
/// </para>
/// <para>
/// All fields are pure data — no generics, no expression trees, no compiled delegates.
/// <see cref="GridQuery"/> is fully AOT-safe and JSON-serialisable.
/// </para>
/// </remarks>
public sealed record GridQuery
{
    /// <summary>
    /// Gets or initialises the active sort instructions, ordered by
    /// <see cref="GridSort.Priority"/> ascending.
    /// An empty list means no explicit sort is applied (data source default order).
    /// </summary>
    public required IReadOnlyList<GridSort> Sorts { get; init; }

    /// <summary>
    /// Gets or initialises the active filter definitions.
    /// All filters are AND-combined; an empty list means no filtering.
    /// </summary>
    public required IReadOnlyList<GridFilter> Filters { get; init; }

    /// <summary>
    /// Gets or initialises the active grouping levels, ordered by
    /// <see cref="GridGroupBy.Priority"/> ascending.
    /// Data sources must pre-sort by group keys before any user sort when this is non-empty.
    /// </summary>
    public IReadOnlyList<GridGroupBy> Groups { get; init; } = [];

    /// <summary>
    /// Gets or initialises the raw quick-filter text entered by the user, or
    /// <see langword="null"/> when no quick filter is active.
    /// Data sources decide whether to apply this as full-text search, multi-field
    /// substring matching, or any other strategy appropriate for their backend.
    /// </summary>
    public string? QuickFilterText { get; init; }

    /// <summary>
    /// Gets or initialises the zero-based index of the first item to return
    /// (i.e. <c>PageIndex * PageSize</c>).
    /// </summary>
    public int Offset { get; init; }

    /// <summary>
    /// Gets or initialises the maximum number of items to return (i.e. <c>PageSize</c>).
    /// </summary>
    public int Limit { get; init; }

    /// <summary>
    /// A shared, pre-constructed instance representing a query with no sorts, filters,
    /// groups, or quick filter, for the first page of 20 items.
    /// </summary>
    public static readonly GridQuery Empty = new()
    {
        Sorts = [],
        Filters = [],
        Limit = 20,
    };
}

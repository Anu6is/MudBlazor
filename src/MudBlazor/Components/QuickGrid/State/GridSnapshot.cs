namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// A complete, serialisable snapshot of all mutable <c>MudQuickGrid&lt;T&gt;</c> state:
/// sort, filter, quick-filter text, pagination, grouping, and column layout.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GridSnapshot"/> is non-generic because <see cref="GridSort"/> and
/// <see cref="GridFilter"/> are pure data records — they contain no expression trees
/// or compiled delegates.  This makes a <see cref="GridSnapshot"/> natively
/// JSON-serialisable with <c>System.Text.Json</c> without any custom converters.
/// </para>
/// <para>
/// Obtain via <c>MudQuickGrid&lt;T&gt;.GetSnapshot()</c>.
/// Restore via <c>MudQuickGrid&lt;T&gt;.ApplySnapshotAsync(snapshot)</c>.
/// Applying a snapshot in <c>OnInitializedAsync</c> is fully supported — the first
/// server round-trip carries all restored state without multiple reloads.
/// </para>
/// <example>
/// Persist and restore:
/// <code>
/// // Save
/// var json = JsonSerializer.Serialize(grid.GetSnapshot());
/// localStorage.SetItem("grid-state", json);
///
/// // Restore in OnInitializedAsync
/// var snapshot = JsonSerializer.Deserialize&lt;GridSnapshot&gt;(json);
/// if (snapshot is not null)
///     await grid.ApplySnapshotAsync(snapshot);
/// </code>
/// </example>
/// </remarks>
public sealed record GridSnapshot
{
    /// <summary>
    /// Gets or initialises the active sort instructions at snapshot time.
    /// Ordered by <see cref="GridSort.Priority"/> ascending.
    /// </summary>
    public IReadOnlyList<GridSort> Sorts { get; init; } = [];

    /// <summary>Gets or initialises the active filter definitions at snapshot time.</summary>
    public IReadOnlyList<GridFilter> Filters { get; init; } = [];

    /// <summary>
    /// Gets or initialises the quick-filter text at snapshot time,
    /// or <see langword="null"/> when no quick filter was active.
    /// </summary>
    public string? QuickFilterText { get; init; }

    /// <summary>Gets or initialises the zero-based current page index at snapshot time.</summary>
    public int PageIndex { get; init; }

    /// <summary>Gets or initialises the page size at snapshot time.</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Gets or initialises the column layout (order, visibility, widths) at snapshot time.
    /// </summary>
    public ColumnLayout ColumnLayout { get; init; } = new();
}

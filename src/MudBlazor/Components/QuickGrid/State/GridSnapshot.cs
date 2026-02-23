namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// A serialisable snapshot of column presentation state: order, visibility, and widths.
/// Stored as part of <see cref="GridSnapshot{T}"/> and also persisted independently via
/// <c>IColumnStatePersistenceProvider</c>.
/// </summary>
public sealed record ColumnLayout
{
    /// <summary>
    /// Gets or initialises the ordered list of column IDs representing the user-configured
    /// display order.  Columns not present are appended at the end in markup order.
    /// </summary>
    public IReadOnlyList<string> ColumnOrder { get; init; } = [];

    /// <summary>
    /// Gets or initialises the set of column IDs that are currently hidden.
    /// Columns not present in this set are visible.
    /// </summary>
    public IReadOnlySet<string> HiddenColumns { get; init; } = new HashSet<string>();

    /// <summary>
    /// Gets or initialises the map of column IDs to their widths in pixels.
    /// Columns not present use their CSS <c>Width</c> parameter or browser default.
    /// </summary>
    public IReadOnlyDictionary<string, double> ColumnWidths { get; init; } =
        new Dictionary<string, double>();
}

/// <summary>
/// A complete, serialisable snapshot of all mutable <c>MudQuickGrid&lt;T&gt;</c> state:
/// sort, filter, quick-filter, pagination, and column layout.
/// </summary>
/// <remarks>
/// <para>
/// Obtain via <c>MudQuickGrid&lt;T&gt;.GetSnapshot()</c>.
/// Restore via <c>MudQuickGrid&lt;T&gt;.ApplySnapshotAsync(snapshot)</c>.
/// </para>
/// <para>
/// Applying a snapshot in <c>OnInitializedAsync</c> is fully supported — the first server
/// round-trip will carry all restored state without multiple reloads.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed record GridSnapshot<T>
{
    /// <summary>Gets or initialises the active sort definitions at snapshot time.</summary>
    public IReadOnlyList<SortDefinition<T>> SortDefinitions { get; init; } = [];

    /// <summary>
    /// Gets or initialises the active filter definitions in their serialisable form.
    /// <c>QuickGridState&lt;T&gt;</c> reconstructs typed <see cref="IFilterDefinition{T}"/>
    /// instances from these during <c>ApplySnapshotAsync</c>.
    /// </summary>
    public IReadOnlyList<SerializableFilterDefinition> FilterDefinitions { get; init; } = [];

    /// <summary>Gets or initialises the quick-filter string at snapshot time, or <see langword="null"/>.</summary>
    public string? QuickFilterValue { get; init; }

    /// <summary>Gets or initialises the zero-based current page index at snapshot time.</summary>
    public int CurrentPage { get; init; }

    /// <summary>Gets or initialises the rows-per-page value at snapshot time.</summary>
    public int RowsPerPage { get; init; }

    /// <summary>Gets or initialises the column layout (order, visibility, widths) at snapshot time.</summary>
    public ColumnLayout ColumnLayout { get; init; } = new();
}

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// A point-in-time snapshot of the grid's data and metadata used by
/// </summary>
/// <remarks>
/// The <see cref="Items"/> collection contains the <em>full</em> filtered dataset —
/// not just the current page — so that exports are never truncated by pagination.
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed record GridExportSnapshot<T>
{
    /// <summary>
    /// Gets or initialises the ordered list of visible columns to include in the export.
    /// </summary>
    public IReadOnlyList<ExportColumn<T>> Columns { get; init; } = [];

    /// <summary>
    /// Gets or initialises the full filtered item set (unpaginated) at the time the export
    /// was triggered.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>Gets or initialises the sort definitions active at export time.</summary>
    public IReadOnlyList<SortDefinition<T>> SortDefinitions { get; init; } = [];

    /// <summary>Gets or initialises the filter definitions active at export time.</summary>
    public IReadOnlyList<IFilterDefinition<T>> FilterDefinitions { get; init; } = [];

    /// <summary>Gets or initialises the UTC timestamp when this snapshot was created.</summary>
    public DateTimeOffset SnapshotTime { get; init; } = DateTimeOffset.UtcNow;
}

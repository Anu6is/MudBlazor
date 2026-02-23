namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Provides data to a column's <c>HeaderTemplate</c> render fragment, allowing consumers
/// to fully customise the column header cell's content.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class HeaderContext<T>
{
    /// <summary>Gets the default title inferred or configured for the column.</summary>
    public string? Title { get; init; }

    /// <summary>Gets whether this column currently has an active sort.</summary>
    public bool IsSorted { get; init; }

    /// <summary>Gets whether the current sort for this column is descending.</summary>
    public bool SortDescending { get; init; }

    /// <summary>
    /// Gets a delegate that toggles the sort direction for this column when invoked.
    /// Honours <c>SortMode</c> (single vs. multi-column).
    /// </summary>
    public Func<Task> ToggleSortAsync { get; init; } = () => Task.CompletedTask;
}

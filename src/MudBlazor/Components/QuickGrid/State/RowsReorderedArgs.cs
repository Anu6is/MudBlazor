namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Provides data for the <c>MudQuickGrid&lt;T&gt;.OnRowsReordered</c> event,
/// which fires after a successful drag-and-drop row reorder operation.
/// </summary>
/// <remarks>
/// The grid does <em>not</em> mutate the <c>Items</c> source.  The consumer is
/// responsible for applying the reorder to their data source using the information
/// provided in this record.
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed record RowsReorderedArgs<T>
{
    /// <summary>Gets or initialises the item that was dragged.</summary>
    public T Item { get; init; } = default!;

    /// <summary>
    /// Gets or initialises the zero-based index of the item in the current page's
    /// display order <em>before</em> the drag.
    /// </summary>
    public int OldIndex { get; init; }

    /// <summary>
    /// Gets or initialises the zero-based index of the item in the current page's
    /// display order <em>after</em> the drop.
    /// </summary>
    public int NewIndex { get; init; }
}

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Provides data to a column's <c>CellTemplate</c> or <c>EditTemplate</c> render fragment.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class CellContext<T>
{
    /// <summary>Gets the row item for the cell being rendered.</summary>
    public T Item { get; init; } = default!;

    /// <summary>Gets the zero-based display index of the row on the current page.</summary>
    public int RowIndex { get; init; }

    /// <summary>
    /// Gets whether this row is currently in edit mode.
    /// Consumers can use this to switch between read and edit templates in a single
    /// <c>CellTemplate</c> render fragment.
    /// </summary>
    public bool IsEditing { get; init; }

    /// <summary>
    /// Gets a delegate that begins editing this row programmatically.
    /// No-op when the row is not editable per <c>RowEditableFunc</c>.
    /// </summary>
    public Func<Task> BeginEditAsync { get; init; } = () => Task.CompletedTask;
}

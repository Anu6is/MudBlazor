namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Provides the row item and action delegates to the <c>CommandColumn&lt;T&gt;.CellTemplate</c>
/// render fragment.  Enables fully custom command cell content while still having access
/// to the built-in edit and delete triggers.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class CommandColumnContext<T>
{
    /// <summary>Gets the row item for the cell being rendered.</summary>
    public T Item { get; init; } = default!;

    /// <summary>Gets the zero-based display index of the row on the current page.</summary>
    public int RowIndex { get; init; }

    /// <summary>
    /// Gets a delegate that initiates editing for this row using the configured
    /// <c>EditMode</c> and <c>EditTrigger</c>.
    /// No-op when <c>RowEditableFunc</c> returns <see langword="false"/> for this item.
    /// </summary>
    public Func<Task> BeginEditAsync { get; init; } = () => Task.CompletedTask;

    /// <summary>
    /// Gets a delegate that invokes the grid's <c>OnDelete</c> event for this row.
    /// No-op when <c>CommandColumn.ShowDeleteButton</c> is <see langword="false"/>.
    /// </summary>
    public Func<Task> DeleteAsync { get; init; } = () => Task.CompletedTask;
}

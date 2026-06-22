namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Determines the column-resize strategy used by <c>IColumnResizerService</c>.
/// </summary>
public enum ResizeMode
{
    /// <summary>Column resizing is disabled. Resize handles are not rendered.</summary>
    None,

    /// <summary>
    /// Dragging a column's resize handle changes that column's width only. Adjacent columns are
    /// not affected; the total table width changes as a result.
    /// </summary>
    Column,

    /// <summary>
    /// Dragging a column's resize handle widens one column while narrowing its right neighbour,
    /// keeping the total table width constant — like a traditional spreadsheet resize.
    /// </summary>
    Container
}

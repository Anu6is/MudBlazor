namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Controls how column filter UI is presented in <c>MudQuickGrid&lt;T&gt;</c>.
/// </summary>
public enum QuickGridFilterMode
{
    /// <summary>
    /// Filter chips are displayed in the toolbar. Clicking a chip — or the filter icon on a column
    /// header — opens a popover with the filter input for that column.
    /// </summary>
    Simple,

    /// <summary>
    /// An additional header row is rendered below the column header row. Each cell in the filter row
    /// renders the column's <c>FilterTemplate</c>, or the default input for the column's data type.
    /// Compatible with <c>FixedHeader=true</c>.
    /// </summary>
    ColumnFilterRow
}

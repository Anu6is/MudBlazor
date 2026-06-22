namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Controls how column header action icons (sort, filter, resize, pin) are presented.
/// </summary>
public enum HeaderActionsMode
{
    /// <summary>
    /// Sort icon, filter icon, and drag handle are shown as individual inline icons.
    /// This is the default and preserves the familiar MudDataGrid appearance.
    /// </summary>
    Inline,

    /// <summary>
    /// All header actions (sort ascending, sort descending, clear sort, filter, pin left,
    /// pin right, hide column, resize) are collapsed into a single ellipsis (⋯) icon that
    /// opens a <c>MudMenu</c>. Eliminates the icon-overflow problem in narrow columns.
    /// </summary>
    Menu
}

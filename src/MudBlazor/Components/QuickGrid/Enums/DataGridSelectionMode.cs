namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Controls row selection behaviour in <c>MudQuickGrid&lt;T&gt;</c>.
/// </summary>
public enum DataGridSelectionMode
{
    /// <summary>Row selection is disabled.</summary>
    None,

    /// <summary>
    /// At most one row may be selected at a time. Clicking a row selects it and deselects the
    /// previously selected row.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple rows may be selected simultaneously via checkboxes (via <c>SelectColumn&lt;T&gt;</c>)
    /// or Shift-click range extension. "Select all on current page" and "Select all across all pages"
    /// are distinct toolbar actions.
    /// </summary>
    MultiSelection
}

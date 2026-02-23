namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Specifies what user action initiates row editing in <c>MudQuickGrid&lt;T&gt;</c>.
/// </summary>
public enum DataGridEditTrigger
{
    /// <summary>
    /// Edit mode is only entered programmatically via <c>BeginEditRowAsync(item)</c>.
    /// No automatic trigger on click or double-click.
    /// </summary>
    Manual,

    /// <summary>
    /// A single click anywhere on the row enters edit mode.
    /// Equivalent to <c>OnRowClick</c> wired to <c>BeginEditRowAsync</c>.
    /// </summary>
    OnRowClick,

    /// <summary>
    /// A single click on any data cell enters edit mode for that cell (in <c>Cell</c> edit mode)
    /// or for the entire row (in <c>Row</c> edit mode).
    /// </summary>
    OnCellClick,

    /// <summary>
    /// A double-click anywhere on the row enters edit mode.
    /// </summary>
    OnRowDoubleClick
}

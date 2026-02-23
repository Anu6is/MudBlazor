namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Determines how rows enter edit mode in <c>MudQuickGrid&lt;T&gt;</c>.
/// </summary>
public enum DataGridEditMode
{
    /// <summary>Editing is disabled. The grid is read-only.</summary>
    None,

    /// <summary>
    /// Clicking a cell activates that single cell for inline editing.
    /// Other cells in the row remain read-only.
    /// Tab moves to the next editable cell; Enter commits; Escape cancels.
    /// </summary>
    Cell,

    /// <summary>
    /// The edit trigger switches the entire row into edit mode simultaneously.
    /// All editable cells become input fields. Tab navigates between cells within the row.
    /// Commit/cancel controls appear at the end of the row.
    /// </summary>
    Row,

    /// <summary>
    /// The row data is cloned into a <c>MudDialog</c> using <c>EditFormContent</c>.
    /// The dialog auto-focuses the first editable field. Enter submits (unless a multi-line
    /// field has focus). Standard <c>EditContext</c> validation is supported.
    /// </summary>
    Form
}

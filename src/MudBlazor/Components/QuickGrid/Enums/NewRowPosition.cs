namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Specifies where the transient new-row appears when <c>BeginNewRowAsync()</c> is called.
/// </summary>
public enum NewRowPosition
{
    /// <summary>The new row is inserted at the top of the data rows, above existing items.</summary>
    Top,

    /// <summary>The new row is appended at the bottom of the data rows, below existing items.</summary>
    Bottom
}

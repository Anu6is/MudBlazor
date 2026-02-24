using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Handles all column resize pointer tracking, width calculation, and delta accumulation.
/// Communicates results through events so that no rendering component holds resize logic.
/// </summary>
/// <remarks>
/// <para>
/// <c>GridHeaderCell&lt;T&gt;</c> wires a single <c>@onpointerdown</c> handler on its
/// resize handle element that calls <see cref="BeginResizeAsync"/>.  All subsequent
/// <c>pointermove</c> and <c>pointerup</c> events are also forwarded to this service.
/// The component itself has no knowledge of widths, deltas, RTL, or min/max clamping.
/// </para>
/// <para>
/// <c>QuickGridState&lt;T&gt;</c> subscribes to <see cref="ColumnWidthChanged"/> and
/// calls <c>SetColumnWidthAsync</c> to update column widths in state, which in turn
/// triggers sticky offset recomputation.
/// </para>
/// </remarks>
public interface IColumnResizerService
{
    /// <summary>
    /// Gets whether a resize drag operation is currently in progress.
    /// </summary>
    bool IsResizing { get; }

    /// <summary>
    /// Begins a resize operation for the specified column, capturing the initial
    /// pointer position from <paramref name="args"/>.
    /// </summary>
    /// <param name="columnId">The stable ID of the column whose handle was grabbed.</param>
    /// <param name="args">The <c>pointerdown</c> event arguments providing the initial position.</param>
    /// <param name="headerElement">
    /// A reference to the header cell element, used to read the current rendered width
    /// as the baseline for delta calculation.
    /// </param>
    Task BeginResizeAsync(string columnId, PointerEventArgs args, ElementReference headerElement);

    /// <summary>
    /// Updates the in-progress resize with the latest pointer position.
    /// Fires <see cref="ColumnWidthChanged"/> with the new provisional width.
    /// </summary>
    /// <param name="args">The <c>pointermove</c> event arguments.</param>
    Task HandleResizeMoveAsync(PointerEventArgs args);

    /// <summary>
    /// Ends the resize operation, fires a final <see cref="ColumnWidthChanged"/> with
    /// the committed width, then fires <see cref="ResizeStateChanged"/> with
    /// <see langword="false"/>.
    /// </summary>
    /// <param name="args">The <c>pointerup</c> event arguments.</param>
    Task EndResizeAsync(PointerEventArgs args);

    /// <summary>
    /// Raised whenever the width of a column changes during (or at the end of) a resize
    /// operation.  Arguments are the column ID and the new width in pixels.
    /// </summary>
    /// <remarks>
    /// <c>QuickGridState&lt;T&gt;</c> subscribes to this event and calls
    /// <c>SetColumnWidthAsync(columnId, newWidthPx)</c> in response.
    /// </remarks>
    event Action<string, double>? ColumnWidthChanged;

    /// <summary>
    /// Raised when the <see cref="IsResizing"/> state changes.
    /// The boolean argument is <see langword="true"/> when a drag begins and
    /// <see langword="false"/> when it ends.
    /// </summary>
    event Action<bool>? ResizeStateChanged;
}

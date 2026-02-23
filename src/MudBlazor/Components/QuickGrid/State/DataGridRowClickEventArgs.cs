using Microsoft.AspNetCore.Components.Web;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Provides data for the <c>MudQuickGrid&lt;T&gt;.RowClick</c> event.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed record DataGridRowClickEventArgs<T>
{
    /// <summary>Gets or initialises the row item that was clicked.</summary>
    public T Item { get; init; } = default!;

    /// <summary>
    /// Gets or initialises the zero-based display index of the clicked row on the current page.
    /// </summary>
    public int RowIndex { get; init; }

    /// <summary>
    /// Gets or initialises the underlying <see cref="MouseEventArgs"/> from the browser event.
    /// </summary>
    public MouseEventArgs MouseEventArgs { get; init; } = new();
}

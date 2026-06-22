using Microsoft.AspNetCore.Components.Web;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Provides data and control delegates to the <c>MudQuickGrid&lt;T&gt;.RowContextMenu</c>
/// render fragment, displayed when the user right-clicks a row or presses the context-menu key.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class RowContextMenuContext<T>
{
    /// <summary>Gets the row item for which the context menu was triggered.</summary>
    public T Item { get; init; } = default!;

    /// <summary>Gets the browser mouse event that triggered the context menu.</summary>
    public MouseEventArgs MouseEventArgs { get; init; } = new();

    /// <summary>
    /// Gets a delegate that closes the context menu popover.
    /// Consumers should call this after a menu action has been selected.
    /// </summary>
    public Func<Task> CloseAsync { get; init; } = () => Task.CompletedTask;
}

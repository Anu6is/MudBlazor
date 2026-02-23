namespace MudBlazor.QuickGrid.Registration;

/// <summary>
/// Cascaded from <c>MudQuickGrid&lt;T&gt;</c> to all descendant column components,
/// allowing them to register and unregister themselves with the grid's ordered column list.
/// </summary>
/// <remarks>
/// <para>
/// Column components call <see cref="RegisterColumn"/> in <c>OnInitialized</c> and
/// <see cref="UnregisterColumn"/> in <c>Dispose</c>.  The grid maintains an ordered
/// <c>List&lt;QuickGridColumnBase&lt;T&gt;&gt; RenderedColumns</c> that respects
/// markup declaration order.
/// </para>
/// <para>
/// This interface is non-generic intentionally: it is cascaded once from the grid and
/// received by column base components that are themselves generic.  The non-generic
/// contract avoids an open-generic cascade value and keeps column registration type-safe
/// through the <c>QuickGridColumnBase&lt;T&gt;</c> base class constraint enforced in the
/// implementing grid.
/// </para>
/// </remarks>
public interface IQuickGridColumnRegistrar
{
    /// <summary>
    /// Registers <paramref name="column"/> with the grid, appending it to the end of
    /// the ordered column list.  Called from <c>QuickGridColumnBase&lt;T&gt;.OnInitialized</c>.
    /// </summary>
    /// <param name="column">
    /// The column component instance being registered.  Must not be <see langword="null"/>.
    /// </param>
    void RegisterColumn(object column);

    /// <summary>
    /// Removes <paramref name="column"/> from the ordered column list.
    /// Called from <c>QuickGridColumnBase&lt;T&gt;.Dispose</c>.
    /// </summary>
    /// <param name="column">
    /// The column component instance being unregistered.  No-op if the column is
    /// not currently registered.
    /// </param>
    void UnregisterColumn(object column);
}

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Provides consumer-defined storage for column layout state (visibility, order, and widths),
/// enabling columns to persist across navigation or browser sessions.
/// </summary>
/// <remarks>
/// <para>
/// No built-in implementation ships with the core package to avoid a hard dependency on
/// <c>localStorage</c>, a server API, or any other storage mechanism.
/// </para>
/// <para>
/// When <c>MudQuickGrid&lt;T&gt;.PersistenceKey</c> is set and an implementation of this
/// interface is registered in DI, the grid will:
/// <list type="number">
///   <item><description>Call <see cref="LoadAsync"/> in <c>OnInitializedAsync</c> before the first data load.</description></item>
///   <item><description>Apply the returned <see cref="ColumnLayout"/> as the initial column state when non-null.</description></item>
///   <item><description>Call <see cref="SaveAsync"/> whenever column visibility, order, or widths change.</description></item>
/// </list>
/// </para>
/// <para>
/// Reference implementations are documented in the companion docs:
/// <c>LocalStorageColumnStatePersistenceProvider</c> (uses <c>IJSRuntime</c>) and
/// <c>HttpApiColumnStatePersistenceProvider</c> (uses <c>HttpClient</c>).
/// </para>
/// </remarks>
public interface IColumnStatePersistenceProvider
{
    /// <summary>
    /// Loads the persisted <see cref="ColumnLayout"/> for the grid identified by
    /// <paramref name="persistenceKey"/>.
    /// </summary>
    /// <param name="persistenceKey">
    /// The unique key set on <c>MudQuickGrid&lt;T&gt;.PersistenceKey</c>. Consumers
    /// are responsible for ensuring uniqueness across grids in the same application.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The stored <see cref="ColumnLayout"/>, or <see langword="null"/> if no layout
    /// has been persisted yet for this key (grid uses its default column state).
    /// </returns>
    Task<ColumnLayout?> LoadAsync(string persistenceKey, CancellationToken ct = default);

    /// <summary>
    /// Persists the current <see cref="ColumnLayout"/> for the grid identified by
    /// <paramref name="persistenceKey"/>.  Called by the grid after every column layout change.
    /// </summary>
    /// <param name="persistenceKey">
    /// The unique key set on <c>MudQuickGrid&lt;T&gt;.PersistenceKey</c>.
    /// </param>
    /// <param name="layout">The column layout state to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveAsync(string persistenceKey, ColumnLayout layout, CancellationToken ct = default);
}

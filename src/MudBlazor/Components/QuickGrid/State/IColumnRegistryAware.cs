namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Implemented by <see cref="IGridDataSource{T}"/> adapters that need access to the
/// grid's <see cref="ColumnRegistry{T}"/> to resolve column metadata at query time.
/// </summary>
/// <remarks>
/// <para>
/// <c>MudQuickGrid&lt;T&gt;</c> checks whether its <c>DataSource</c> implements this
/// interface during <c>OnInitializedAsync</c>, after all child column components have
/// registered.  If so, it calls <see cref="Initialize"/> once with the fully populated
/// registry before the first <c>QueryAsync</c> is invoked.
/// </para>
/// <para>
/// This interface is <c>internal</c> — it is not visible to consumers.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
internal interface IColumnRegistryAware<T>
{
    /// <summary>
    /// Called once by the grid after all columns have registered.
    /// Implementations should cache the registry or derive compiled structures
    /// (delegate caches, expression caches) from it here rather than at query time.
    /// </summary>
    /// <param name="registry">The fully populated column registry.</param>
    void Initialize(ColumnRegistry<T> registry);
}

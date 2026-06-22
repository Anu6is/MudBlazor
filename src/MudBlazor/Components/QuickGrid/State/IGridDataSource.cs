namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// The single data binding surface for <c>MudQuickGrid&lt;T&gt;</c>.
/// </summary>
/// <remarks>
/// <para>
/// <c>MudQuickGrid&lt;T&gt;</c> accepts a single <c>DataSource</c> parameter of this type,
/// replacing the previous <c>Items</c> / <c>ItemsProvider</c> parameter fork.
/// The grid itself performs no sorting, filtering, or expression compilation —
/// it builds a <see cref="GridQuery"/> from its current <c>GridState</c> and
/// delegates all execution to the implementation.
/// </para>
/// <para>
/// Three built-in implementations are provided:
/// <list type="bullet">
///   <item><description><c>InMemoryDataSource&lt;T&gt;</c> — wraps <c>IEnumerable&lt;T&gt;</c>, compiles delegates once at initialisation.</description></item>
///   <item><description><c>QueryableDataSource&lt;T&gt;</c> — wraps <c>IQueryable&lt;T&gt;</c>, builds EF Core-compatible expression trees.</description></item>
///   <item><description><c>DelegateDataSource&lt;T&gt;</c> — wraps a <c>Func&lt;GridQuery, CancellationToken, ValueTask&lt;GridDataPage&lt;T&gt;&gt;&gt;</c> for remote APIs.</description></item>
/// </list>
/// </para>
/// <para>
/// Implement this interface directly to integrate with OData, GraphQL, gRPC, CQRS
/// command buses, or any other data retrieval mechanism.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public interface IGridDataSource<T>
{
    /// <summary>
    /// Executes the <paramref name="query"/> and returns the matching page of items.
    /// </summary>
    /// <param name="query">
    /// The complete query descriptor built from the grid's current state.
    /// Includes sorts, filters, grouping intent, quick-filter text, offset, and limit.
    /// </param>
    /// <param name="cancellationToken">
    /// Token supplied by the grid's debounce mechanism.  Implementations should
    /// respect this to avoid redundant work on rapid state changes.
    /// </param>
    /// <returns>
    /// A <see cref="GridDataPage{T}"/> containing the page items and the total
    /// pre-pagination item count.
    /// </returns>
    ValueTask<GridDataPage<T>> QueryAsync(GridQuery query, CancellationToken cancellationToken = default);
}

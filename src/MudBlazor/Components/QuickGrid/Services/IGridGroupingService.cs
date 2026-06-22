namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Builds the recursive <see cref="GroupNode{T}"/> display tree from paginated items
/// returned by the data source.
/// </summary>
/// <remarks>
/// <para>
/// This service is a <em>rendering</em> concern.  It receives already-retrieved,
/// already-group-sorted page items and organises them into the nested tree that
/// <c>GridBody&lt;T&gt;</c> renders.
/// </para>
/// <para>
/// Group pre-sorting (ensuring same-group items are consecutive within a page) is now
/// the responsibility of the <see cref="IGridDataSource{T}"/> implementation via the
/// <see cref="GridGroupBy"/> entries in <see cref="GridQuery.Groups"/>.  The previous
/// <c>ApplyGroupSort</c> method has been removed from this interface and is implemented
/// internally by <c>QueryableSortApplicator&lt;T&gt;</c> and the in-memory sort path
/// of <c>InMemoryDataSource&lt;T&gt;</c>.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public interface IGridGroupingService<T>
{
    /// <summary>
    /// Builds a recursive <see cref="GroupNode{T}"/> tree from the
    /// <paramref name="items"/> that belong to the current page.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is pure — given the same inputs it always returns the same tree
    /// and has no side effects on <c>QuickGridState&lt;T&gt;</c>.
    /// </para>
    /// <para>
    /// Each node's <see cref="GroupNode{T}.KeyPath"/> is a pipe-delimited concatenation
    /// of ancestor key strings down to the current node, providing a unique stable
    /// <c>@key</c> value for Blazor's diffing algorithm.
    /// </para>
    /// </remarks>
    /// <param name="items">
    /// The materialised current-page items, already group-sorted by the data source.
    /// </param>
    /// <param name="groupColumns">
    /// The ordered list of columns that have grouping active.  Index 0 is the outermost
    /// group level.
    /// </param>
    /// <param name="expansionState">
    /// A map of <see cref="GroupKey"/> → expanded flag.
    /// Nodes not present use <paramref name="defaultExpanded"/>.
    /// </param>
    /// <param name="defaultExpanded">
    /// The initial expansion state for nodes absent from <paramref name="expansionState"/>.
    /// </param>
    /// <param name="aggregates">
    /// Column-ID → <see cref="AggregateDefinition{T}"/> for columns that declare
    /// footer aggregates.  Used to pre-compute <see cref="GroupNode{T}.Aggregates"/>.
    /// </param>
    /// <returns>
    /// Root-level group nodes, or an empty list when <paramref name="groupColumns"/>
    /// or <paramref name="items"/> is empty.
    /// </returns>
    IReadOnlyList<GroupNode<T>> BuildGroupTree(IReadOnlyList<T> items,
                                               IReadOnlyList<GroupColumnDefinition<T>> groupColumns,
                                               IReadOnlyDictionary<GroupKey, bool> expansionState,
                                               bool defaultExpanded,
                                               IReadOnlyDictionary<string, AggregateDefinition<T>> aggregates);
}

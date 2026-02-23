using MudBlazor.Components.QuickGrid;

namespace MudBlazor.QuickGrid.Services;

/// <summary>
/// Provides the two grouping operations required by the data pipeline:
/// pre-sorting the source by group key(s) to guarantee consecutive groups within pages,
/// and building the recursive <see cref="GroupNode{T}"/> display tree from paginated items.
/// </summary>
/// <remarks>
/// <para>
/// Both methods are pure with respect to grid state: they receive all inputs as parameters
/// and return new data structures without side effects on <c>QuickGridState&lt;T&gt;</c>.
/// </para>
/// <para>
/// <see cref="ApplyGroupSort"/> runs before <c>Skip</c>/<c>Take</c> in the client-side
/// pipeline, ensuring that group rows are always consecutive within a page.
/// </para>
/// <para>
/// <see cref="BuildGroupTree"/> runs after pagination on the materialised page items,
/// constructing the nested <see cref="GroupNode{T}"/> tree that <c>GridBody&lt;T&gt;</c>
/// renders recursively.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public interface IGridGroupingService<T>
{
    /// <summary>
    /// Pre-sorts <paramref name="source"/> by the group key(s) of all
    /// <paramref name="groupColumns"/> in <see cref="GroupColumnDefinition{T}.GroupByOrder"/>
    /// ascending order, prepending these sort clauses before any user-specified sort.
    /// </summary>
    /// <remarks>
    /// This method must be called before <c>Skip</c>/<c>Take</c> to guarantee that groups
    /// are always consecutive within a paginated result.  When
    /// <paramref name="groupColumns"/> is empty, <paramref name="source"/> is returned
    /// unchanged.
    /// </remarks>
    /// <param name="source">The queryable source, already filtered but not yet paginated.</param>
    /// <param name="groupColumns">
    /// The ordered list of columns that have grouping active.
    /// </param>
    /// <returns>
    /// An <see cref="IOrderedQueryable{T}"/> ordered by the group keys, or the original
    /// <paramref name="source"/> when no grouping columns are present.
    /// </returns>
    IQueryable<T> ApplyGroupSort(
        IQueryable<T> source,
        IReadOnlyList<GroupColumnDefinition<T>> groupColumns);

    /// <summary>
    /// Builds a recursive <see cref="GroupNode{T}"/> tree from the
    /// <paramref name="items"/> that belong to the current page.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is pure: given the same inputs it always returns the same tree.
    /// It has no side effects on <c>QuickGridState&lt;T&gt;</c>.
    /// </para>
    /// <para>
    /// Each node's <see cref="GroupNode{T}.KeyPath"/> is a pipe-delimited concatenation of
    /// ancestor key strings down to the current node, providing a unique
    /// <c>@key</c> value for Blazor's diffing algorithm.
    /// </para>
    /// <para>
    /// Aggregate values are computed for every node when the corresponding column
    /// has an <c>AggregateDefinition</c> entry in <paramref name="aggregates"/>.
    /// </para>
    /// </remarks>
    /// <param name="items">
    /// The materialised current-page items, already group-sorted by <see cref="ApplyGroupSort"/>.
    /// </param>
    /// <param name="groupColumns">
    /// The ordered list of columns that have grouping active.  The list order defines
    /// the nesting depth: index 0 is the outermost group level.
    /// </param>
    /// <param name="expansionState">
    /// A map of <see cref="GroupKey"/> → expanded flag from
    /// <c>QuickGridState&lt;T&gt;.GroupExpansionState</c>.
    /// Nodes not present in this map use <paramref name="defaultExpanded"/>.
    /// </param>
    /// <param name="defaultExpanded">
    /// The initial expansion state applied to nodes not found in
    /// <paramref name="expansionState"/>.
    /// </param>
    /// <param name="aggregates">
    /// A map of column ID → <see cref="AggregateDefinition{T}"/> for columns that declare
    /// footer aggregates.  Used to pre-compute <see cref="GroupNode{T}.Aggregates"/>.
    /// </param>
    /// <returns>
    /// The root-level group nodes.  An empty list is returned when
    /// <paramref name="groupColumns"/> is empty or <paramref name="items"/> is empty.
    /// </returns>
    IReadOnlyList<GroupNode<T>> BuildGroupTree(
        IReadOnlyList<T> items,
        IReadOnlyList<GroupColumnDefinition<T>> groupColumns,
        IReadOnlyDictionary<GroupKey, bool> expansionState,
        bool defaultExpanded,
        IReadOnlyDictionary<string, AggregateDefinition<T>> aggregates);
}

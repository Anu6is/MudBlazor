// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Default implementation of <see cref="IGridGroupingService{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ApplyGroupSort"/> is called before <c>Skip</c>/<c>Take</c> to guarantee
/// consecutive groups within paginated results.
/// </para>
/// <para>
/// <see cref="BuildGroupTree"/> is called after pagination on the materialised page items.
/// It is a pure function — same inputs always produce the same tree.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class GridGroupingService<T> : IGridGroupingService<T>
{
    /// <inheritdoc/>
    public IQueryable<T> ApplyGroupSort(
        IQueryable<T> source,
        IReadOnlyList<GroupColumnDefinition<T>> groupColumns)
    {
        if (groupColumns.Count == 0)
            return source;

        var ordered = groupColumns
            .OrderBy(gc => gc.GroupByOrder)
            .ToList();

        IOrderedQueryable<T>? orderedQuery = null;

        foreach (var column in ordered)
        {
            // Build a lambda x => column.GroupBy(x) with the correct return type (object)
            // so it can be passed to OrderBy / ThenBy.
            var keySelector = BuildObjectKeySelectorExpression(column.GroupBy);

            orderedQuery = orderedQuery is null
                ? source.OrderBy(keySelector)
                : orderedQuery.ThenBy(keySelector);
        }

        return orderedQuery ?? source;
    }

    /// <inheritdoc/>
    public IReadOnlyList<GroupNode<T>> BuildGroupTree(
        IReadOnlyList<T> items,
        IReadOnlyList<GroupColumnDefinition<T>> groupColumns,
        IReadOnlyDictionary<GroupKey, bool> expansionState,
        bool defaultExpanded,
        IReadOnlyDictionary<string, AggregateDefinition<T>> aggregates)
    {
        if (groupColumns.Count == 0 || items.Count == 0)
            return [];

        // Sort group columns by GroupByOrder so nesting depth matches the sort order
        // applied by ApplyGroupSort.
        var sortedGroupColumns = groupColumns
            .OrderBy(gc => gc.GroupByOrder)
            .ToList();

        return BuildLevel(
            items,
            sortedGroupColumns,
            depth: 0,
            parentKeyPath: string.Empty,
            expansionState,
            defaultExpanded,
            aggregates);
    }

    // ── Recursive tree builder ────────────────────────────────────────────────

    private static IReadOnlyList<GroupNode<T>> BuildLevel(
        IReadOnlyList<T> items,
        IReadOnlyList<GroupColumnDefinition<T>> groupColumns,
        int depth,
        string parentKeyPath,
        IReadOnlyDictionary<GroupKey, bool> expansionState,
        bool defaultExpanded,
        IReadOnlyDictionary<string, AggregateDefinition<T>> aggregates)
    {
        var column = groupColumns[depth];
        var nodes = new List<GroupNode<T>>();

        // Group items by their key at this depth, preserving encounter order.
        // We cannot use LINQ GroupBy with IQueryable here (already materialised).
        var groups = GroupConsecutive(items, column.GroupBy);

        foreach (var (key, groupItems) in groups)
        {
            var keyString = key?.ToString() ?? string.Empty;
            var keyPath = parentKeyPath.Length == 0
                ? keyString
                : $"{parentKeyPath}|{keyString}";

            var groupKey = new GroupKey(keyPath);
            var isExpanded = expansionState.TryGetValue(groupKey, out var expanded)
                ? expanded
                : defaultExpanded;

            bool isLeaf = depth == groupColumns.Count - 1;

            IReadOnlyList<GroupNode<T>> children = isLeaf
                ? []
                : BuildLevel(
                    groupItems,
                    groupColumns,
                    depth + 1,
                    keyPath,
                    expansionState,
                    defaultExpanded,
                    aggregates);

            var nodeAggregates = ComputeAggregates(
                groupItems,
                aggregates,
                groupColumns[depth].ColumnId);

            nodes.Add(new GroupNode<T>
            {
                Key = key ?? string.Empty,
                KeyPath = keyPath,
                Depth = depth,
                IsExpanded = isExpanded,
                Children = children,
                Items = isLeaf ? groupItems : [],
                Aggregates = nodeAggregates
            });
        }

        return nodes;
    }

    // ── Aggregate computation ─────────────────────────────────────────────────

    private static IReadOnlyDictionary<string, object?> ComputeAggregates(
        IReadOnlyList<T> items,
        IReadOnlyDictionary<string, AggregateDefinition<T>> aggregates,
        string currentColumnId)
    {
        if (aggregates.Count == 0)
            return new Dictionary<string, object?>();

        var result = new Dictionary<string, object?>(aggregates.Count);

        foreach (var (columnId, definition) in aggregates)
        {
            // GetValue with a null property expression falls back to Count or CustomAggregate.
            // For group nodes we store the formatted string value directly.
            result[columnId] = definition.GetValue<object>(propertyExpression: null, items);
        }

        return result;
    }

    // ── Consecutive grouping ──────────────────────────────────────────────────

    /// <summary>
    /// Groups <paramref name="items"/> by consecutive runs of the same key value,
    /// returning a list of (key, items) pairs that preserves the encounter order.
    /// This matches the semantics of <see cref="ApplyGroupSort"/> which ensures items
    /// with the same group key are adjacent after pre-sorting.
    /// </summary>
    private static List<(object? Key, IReadOnlyList<T> Items)> GroupConsecutive(
        IReadOnlyList<T> items,
        Func<T, object> keySelector)
    {
        var groups = new List<(object? Key, IReadOnlyList<T> Items)>();

        if (items.Count == 0)
            return groups;

        object? currentKey = keySelector(items[0]);
        var currentGroup = new List<T> { items[0] };

        for (int i = 1; i < items.Count; i++)
        {
            var itemKey = keySelector(items[i]);
            if (Equals(itemKey, currentKey))
            {
                currentGroup.Add(items[i]);
            }
            else
            {
                groups.Add((currentKey, currentGroup));
                currentKey = itemKey;
                currentGroup = [items[i]];
            }
        }

        groups.Add((currentKey, currentGroup));
        return groups;
    }

    // ── Expression utilities ──────────────────────────────────────────────────

    /// <summary>
    /// Wraps a <c>Func&lt;T, object&gt;</c> delegate in an
    /// <c>Expression&lt;Func&lt;T, object&gt;&gt;</c> so that <c>IQueryable.OrderBy</c>
    /// can accept it.  Note: for EF Core server-side translation the consumer must
    /// supply a true expression on <see cref="GroupColumnDefinition{T}.GroupBy"/>;
    /// this wrapper serves the client-side LINQ path.
    /// </summary>
    private static Expression<Func<T, object>> BuildObjectKeySelectorExpression(
        Func<T, object> groupByFunc)
    {
        // For in-memory IQueryable (client-side), wrap the compiled delegate in a lambda.
        // This does not translate to SQL — EF Core consumers must override via their own
        // IGridGroupingService<T> implementation that uses real Expression trees.
        var param = Expression.Parameter(typeof(T), "x");
        var funcExpr = Expression.Constant(groupByFunc);
        var invoke = Expression.Invoke(funcExpr, param);
        return Expression.Lambda<Func<T, object>>(invoke, param);
    }
}

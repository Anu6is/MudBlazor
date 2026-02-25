namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Default implementation of <see cref="IGridGroupingService{T}"/>.
/// </summary>
/// <remarks>
/// Implements only <see cref="BuildGroupTree"/> — group pre-sorting is now handled
/// inside <c>InMemoryDataSource&lt;T&gt;</c> and <c>QueryableDataSource&lt;T&gt;</c>
/// via <see cref="GridGroupBy"/> entries in <see cref="GridQuery.Groups"/>.
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class GridGroupingService<T> : IGridGroupingService<T>
{
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

        foreach (var (key, groupItems) in GroupConsecutive(items, column.GroupBy))
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

            var children = isLeaf
                ? (IReadOnlyList<GroupNode<T>>)[]
                : BuildLevel(groupItems, groupColumns, depth + 1, keyPath,
                             expansionState, defaultExpanded, aggregates);

            var nodeAggregates = ComputeAggregates(groupItems, aggregates);

            nodes.Add(new GroupNode<T>
            {
                Key = key ?? string.Empty,
                KeyPath = keyPath,
                Depth = depth,
                IsExpanded = isExpanded,
                Children = children,
                Items = isLeaf ? groupItems : [],
                Aggregates = nodeAggregates,
            });
        }

        return nodes;
    }

    private static IReadOnlyDictionary<string, object?> ComputeAggregates(
        IReadOnlyList<T> items,
        IReadOnlyDictionary<string, AggregateDefinition<T>> aggregates)
    {
        if (aggregates.Count == 0)
            return new Dictionary<string, object?>();

        var result = new Dictionary<string, object?>(aggregates.Count);
        foreach (var (columnId, definition) in aggregates)
        {
            // TODO Phase 5B: Pass the column's ValueSelector here once GroupColumnDefinition<T>
            // carries ColumnDescriptor<T> metadata. Sum/Average/Min/Max return "" until then.
            result[columnId] = definition.GetValue<object>(propertyExpression: null, items);
        }

        return result;
    }

    /// <summary>
    /// Groups items by consecutive runs of equal keys, preserving encounter order.
    /// This is O(n) and relies on the data source having pre-sorted items by group key.
    /// </summary>
    private static List<(object? Key, IReadOnlyList<T> Items)> GroupConsecutive(
        IReadOnlyList<T> items,
        Func<T, object> keySelector)
    {
        var groups = new List<(object? Key, IReadOnlyList<T> Items)>();
        if (items.Count == 0) return groups;

        var currentKey = keySelector(items[0]);
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
}

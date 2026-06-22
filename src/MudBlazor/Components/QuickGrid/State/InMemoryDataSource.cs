namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// An <see cref="IGridDataSource{T}"/> adapter that operates on an in-memory
/// <see cref="IEnumerable{T}"/> collection.
/// </summary>
/// <remarks>
/// <para>
/// <b>AOT safety:</b> All expression compilation happens once at
/// <see cref="IColumnRegistryAware{T}"/> initialization time (or at construction when
/// descriptors are provided directly), never inside <see cref="QueryAsync"/>.
/// The hot query path uses only pre-compiled <see cref="Func{T, TResult}"/> delegates.
/// </para>
/// <para>
/// <b>Quick filter:</b> When <see cref="GridQuery.QuickFilterText"/> is non-null,
/// a default implementation searches across all string-typed column selectors using
/// <see cref="StringComparison.OrdinalIgnoreCase"/>.  Provide
/// <see cref="QuickFilterPredicate"/> to override this behaviour.
/// </para>
/// <para>
/// <b>Data source binding:</b>
/// <code>
/// &lt;MudQuickGrid DataSource="new InMemoryDataSource&lt;Person&gt;(people)"&gt;
/// </code>
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class InMemoryDataSource<T> : IGridDataSource<T>, IColumnRegistryAware<T>
{
    private readonly IReadOnlyList<T> _items;
    private Dictionary<string, ColumnDescriptor<T>> _descriptors = new(StringComparer.Ordinal);
    private List<ColumnDescriptor<T>> _stringDescriptors = [];
    private FilterOptions _filterOptions = FilterOptions.Default;

    /// <summary>
    /// Gets or sets an optional predicate applied when
    /// <see cref="GridQuery.QuickFilterText"/> is non-null.
    /// When <see langword="null"/>, the default multi-column string search is used.
    /// </summary>
    public Func<T, string, bool>? QuickFilterPredicate { get; set; }

    /// <summary>
    /// Gets or sets the filter options (case sensitivity) used for string comparisons.
    /// Defaults to <see cref="FilterOptions.Default"/> (case-insensitive ordinal).
    /// </summary>
    public FilterOptions FilterOptions
    {
        get => _filterOptions;
        set => _filterOptions = value ?? FilterOptions.Default;
    }

    // ── Constructors ──────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises the source with a collection.  Column descriptors will be provided
    /// by the grid via <see cref="IColumnRegistryAware{T}"/> after all columns register.
    /// </summary>
    /// <param name="items">The data collection to wrap. Materialised once at construction.</param>
    public InMemoryDataSource(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        _items = items.ToList();
    }

    /// <summary>
    /// Initialises the source with a collection and an explicit descriptor list.
    /// Use this constructor in unit tests or when a <c>MudQuickGrid</c> is not driving
    /// the lifecycle.
    /// </summary>
    /// <param name="items">The data collection to wrap.</param>
    /// <param name="descriptors">Column descriptors providing value selectors.</param>
    public InMemoryDataSource(IEnumerable<T> items, IReadOnlyList<ColumnDescriptor<T>> descriptors)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(descriptors);

        _items = items.ToList();
        IndexDescriptors(descriptors);
    }

    // ── IColumnRegistryAware ──────────────────────────────────────────────────

    void IColumnRegistryAware<T>.Initialize(ColumnRegistry<T> registry)
    {
        IndexDescriptors(registry.All.ToList());
    }

    // ── IGridDataSource ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public ValueTask<GridDataPage<T>> QueryAsync(
        GridQuery query,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<T> seq = _items;

        // 1. Column filters.
        if (query.Filters.Count > 0)
            seq = InMemoryFilterEvaluator<T>.Apply(seq, query.Filters, _descriptors, _filterOptions);

        // 2. Quick filter.
        if (!string.IsNullOrEmpty(query.QuickFilterText))
            seq = ApplyQuickFilter(seq, query.QuickFilterText);

        // 3. Count before pagination (must materialise after filter).
        var filtered = seq.ToList();
        int totalCount = filtered.Count;

        // 4. Group pre-sort + user sort.
        IEnumerable<T> sorted = ApplySort(filtered, query.Groups, query.Sorts);

        // 5. Pagination.
        var page = sorted
            .Skip(query.Offset)
            .Take(query.Limit)
            .ToList();

        return ValueTask.FromResult(new GridDataPage<T>
        {
            Items = page,
            TotalItemCount = totalCount,
        });
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void IndexDescriptors(IReadOnlyList<ColumnDescriptor<T>> descriptors)
    {
        _descriptors = descriptors.ToDictionary(d => d.Key, StringComparer.Ordinal);
        _stringDescriptors = descriptors
            .Where(d => d.ValueSelector is not null && d.PropertyType == typeof(string))
            .ToList();
    }

    private IEnumerable<T> ApplyQuickFilter(IEnumerable<T> source, string text)
    {
        if (QuickFilterPredicate is not null)
            return source.Where(item => QuickFilterPredicate(item, text));

        // Default: case-insensitive substring match across all string columns.
        if (_stringDescriptors.Count == 0)
            return source;

        return source.Where(item => _stringDescriptors.Any(desc =>
        {
            var value = desc.ValueSelector!(item) as string;
            return value is not null &&
                   value.Contains(text, StringComparison.OrdinalIgnoreCase);
        }));
    }

    private IEnumerable<T> ApplySort(
        List<T> source,
        IReadOnlyList<GridGroupBy> groups,
        IReadOnlyList<GridSort> sorts)
    {
        if (groups.Count == 0 && sorts.Count == 0)
            return source;

        // Combine group pre-sorts and user sorts into a single ordered sequence.
        // Group sorts (by Priority) are applied first as they are prepended.
        var allSorts = groups
            .OrderBy(g => g.Priority)
            .Select(g => new GridSort { ColumnKey = g.ColumnKey, Direction = SortDirection.Ascending, Priority = -1 })
            .Concat(sorts.OrderBy(s => s.Priority))
            .ToList();

        IOrderedEnumerable<T>? ordered = null;

        foreach (var sort in allSorts)
        {
            if (!_descriptors.TryGetValue(sort.ColumnKey, out var descriptor))
                continue;
            if (descriptor.ValueSelector is null)
                continue;

            var selector = descriptor.ValueSelector;
            bool descending = sort.Direction == SortDirection.Descending;

            ordered = ordered is null
                ? (descending ? source.OrderByDescending(selector) : source.OrderBy(selector))
                : (descending ? ordered.ThenByDescending(selector) : ordered.ThenBy(selector));
        }

        return (IEnumerable<T>?)ordered ?? source;
    }
}

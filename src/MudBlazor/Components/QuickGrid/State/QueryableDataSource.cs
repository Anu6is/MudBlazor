namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// An <see cref="IGridDataSource{T}"/> adapter that wraps an <see cref="IQueryable{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Expression building:</b> filter predicates are built by an
/// <see cref="IFilterExpressionBuilder{T}"/> and cached in a
/// <see cref="FilterExpressionCache{T}"/> — identical filters within the lifetime of this
/// instance never rebuild their expression trees.  The default builder uses
/// <see cref="TypeStrategyRegistry"/> and the per-type <see cref="ITypeFilterStrategy"/>
/// implementations; inject a custom builder via the three-argument constructor to replace
/// or extend this behaviour (e.g. a custom LINQ provider rewriter).
/// </para>
/// <para>
/// <b>Async counting:</b> total item count uses synchronous <c>Count()</c> by default.
/// Override <see cref="CountAsync"/> to inject EF Core's async variant:
/// <code>
/// protected override Task&lt;int&gt; CountAsync(IQueryable&lt;T&gt; query, CancellationToken ct)
///     => query.CountAsync(ct);
/// </code>
/// </para>
/// <para>
/// <b>Data source binding:</b>
/// <code>
/// &lt;MudQuickGrid DataSource="new QueryableDataSource&lt;Person&gt;(dbContext.People)"&gt;
/// </code>
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public class QueryableDataSource<T> : IGridDataSource<T>, IColumnRegistryAware<T>
{
    private readonly IQueryable<T> _source;
    private readonly FilterExpressionCache<T> _cache = new();
    private readonly IFilterExpressionBuilder<T> _filterBuilder;
    private Dictionary<string, ColumnDescriptor<T>> _descriptors = new(StringComparer.Ordinal);
    private FilterOptions _filterOptions = FilterOptions.Default;

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
    /// Initialises the adapter wrapping <paramref name="source"/>.
    /// Column descriptors are provided by the grid via <see cref="IColumnRegistryAware{T}"/>
    /// after all columns register.
    /// </summary>
    public QueryableDataSource(IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
        _filterBuilder = new DefaultFilterExpressionBuilder<T>(_cache);
    }

    /// <summary>
    /// Initialises the adapter with a custom <see cref="IFilterExpressionBuilder{T}"/>.
    /// Use when the default <see cref="TypeStrategyRegistry"/>-based builder needs to be
    /// replaced (e.g. a custom LINQ provider with different expression translation rules).
    /// </summary>
    public QueryableDataSource(IQueryable<T> source, IFilterExpressionBuilder<T> filterBuilder)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filterBuilder);
        _source = source;
        _filterBuilder = filterBuilder;
    }

    /// <summary>
    /// Initialises the adapter with an explicit descriptor list.
    /// Use in unit tests or when a grid is not driving the lifecycle.
    /// </summary>
    public QueryableDataSource(IQueryable<T> source, IReadOnlyList<ColumnDescriptor<T>> descriptors)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(descriptors);
        _source = source;
        _filterBuilder = new DefaultFilterExpressionBuilder<T>(_cache);
        _descriptors = descriptors.ToDictionary(d => d.Key, StringComparer.Ordinal);
    }

    // ── IColumnRegistryAware ──────────────────────────────────────────────────

    void IColumnRegistryAware<T>.Initialize(ColumnRegistry<T> registry)
    {
        _descriptors = registry.All.ToDictionary(d => d.Key, StringComparer.Ordinal);
    }

    // ── IGridDataSource ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async ValueTask<GridDataPage<T>> QueryAsync(
        GridQuery query,
        CancellationToken cancellationToken = default)
    {
        var filtered = ApplyFilters(_source, query.Filters);
        var sorted = ApplySorts(filtered, query.Groups, query.Sorts);

        int totalCount = await CountAsync(sorted, cancellationToken).ConfigureAwait(false);

        var page = await MaterialisePageAsync(sorted, query.Offset, query.Limit, cancellationToken)
            .ConfigureAwait(false);

        return new GridDataPage<T>
        {
            Items = page,
            TotalItemCount = totalCount,
        };
    }

    // ── Overridable execution hooks ───────────────────────────────────────────

    /// <summary>
    /// Returns the total count of <paramref name="query"/> results.
    /// Override to use EF Core's async variant:
    /// <code>
    /// protected override Task&lt;int&gt; CountAsync(IQueryable&lt;T&gt; query, CancellationToken ct)
    ///     => query.CountAsync(ct);
    /// </code>
    /// </summary>
    protected virtual Task<int> CountAsync(IQueryable<T> query, CancellationToken ct)
        => Task.FromResult(query.Count());

    /// <summary>
    /// Materialises the page items after <c>Skip</c>/<c>Take</c>.
    /// Override to use EF Core's async variant:
    /// <code>
    /// protected override Task&lt;IReadOnlyList&lt;T&gt;&gt; MaterialisePageAsync(
    ///     IQueryable&lt;T&gt; query, int offset, int limit, CancellationToken ct)
    ///     => query.Skip(offset).Take(limit).ToListAsync(ct)
    ///             .ContinueWith(t => (IReadOnlyList&lt;T&gt;)t.Result);
    /// </code>
    /// </summary>
    protected virtual Task<IReadOnlyList<T>> MaterialisePageAsync(
        IQueryable<T> query, int offset, int limit, CancellationToken ct)
    {
        IReadOnlyList<T> result = query.Skip(offset).Take(limit).ToList();
        return Task.FromResult(result);
    }

    // ── Private pipeline steps ────────────────────────────────────────────────

    private IQueryable<T> ApplyFilters(IQueryable<T> source, IReadOnlyList<GridFilter> filters)
    {
        if (filters.Count == 0) return source;
        return QueryableFilterApplicator<T>.Apply(source, filters, _descriptors, _filterOptions, _filterBuilder);
    }

    private IQueryable<T> ApplySorts(
        IQueryable<T> source,
        IReadOnlyList<GridGroupBy> groups,
        IReadOnlyList<GridSort> sorts)
    {
        if (groups.Count == 0 && sorts.Count == 0) return source;
        return QueryableSortApplicator<T>.Apply(source, groups, sorts, _descriptors);
    }
}

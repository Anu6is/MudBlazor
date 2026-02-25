using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

// ─────────────────────────────────────────────────────────────────────────────
// IFilterExpressionBuilder<T>
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Builds a <c>Where</c> predicate expression for a single <see cref="GridFilter"/> against
/// an <see cref="IQueryable{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is the extension point for replacing or augmenting the expression-building strategy
/// used by <see cref="QueryableDataSource{T}"/>.
/// </para>
/// <para>
/// The default implementation, <see cref="DefaultFilterExpressionBuilder{T}"/>, uses
/// <see cref="TypeStrategyRegistry"/> and <see cref="FilterExpressionCache{T}"/>.
/// Custom implementations can be injected via the
/// <see cref="QueryableDataSource{T}(IQueryable{T}, IFilterExpressionBuilder{T})"/> constructor,
/// for example to produce a custom provider-specific expression that translates differently
/// (e.g. a custom LINQ provider with different translation rules, or a full
/// <c>ExpressionVisitor</c>-based rewriter).
/// </para>
/// <para>
/// <b>Scope:</b> this interface is for <i>LINQ expression generation</i> only — it targets the
/// <see cref="IQueryable{T}"/> execution path.  For remote (non-LINQ) providers, use
/// <see cref="DelegateDataSource{T}"/> and translate the raw <see cref="GridQuery"/> in the
/// delegate.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row / entity type.</typeparam>
internal interface IFilterExpressionBuilder<T>
{
    /// <summary>
    /// Builds a predicate for a single <paramref name="filter"/>.
    /// </summary>
    /// <returns>
    /// A <c>Where</c> predicate, or <see langword="null"/> to skip this filter
    /// (e.g. unsupported type, missing value, or constant-true predicate).
    /// </returns>
    Expression<Func<T, bool>>? Build(GridFilter filter, ColumnDescriptor<T> descriptor, FilterOptions options);
}

// ─────────────────────────────────────────────────────────────────────────────
// DefaultFilterExpressionBuilder<T>
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// The default <see cref="IFilterExpressionBuilder{T}"/> implementation used by
/// <see cref="QueryableDataSource{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Pipeline per filter:</b>
/// <list type="number">
///   <item>
///     <description>
///       Type-agnostic operators (<c>IsNull</c>, <c>IsNotNull</c>) are handled directly
///       without consulting <see cref="TypeStrategyRegistry"/>.
///     </description>
///   </item>
///   <item>
///     <description>
///       The operator is resolved to a <see cref="FilterOperatorDescriptor"/> via
///       <see cref="BuiltInOperators.Get(FilterOperator)"/>.
///     </description>
///   </item>
///   <item>
///     <description>
///       A <see cref="FilterCacheKey"/> is constructed from the column key, operator key,
///       raw value string, and string comparison setting.
///     </description>
///   </item>
///   <item>
///     <description>
///       The <see cref="FilterExpressionCache{T}"/> is checked; on miss, the filter value
///       is parsed via <see cref="FilterValueParser"/>, the appropriate
///       <see cref="ITypeFilterStrategy"/> is resolved from <see cref="TypeStrategyRegistry"/>,
///       and <see cref="ITypeFilterStrategy.BuildExpression"/> is invoked.
///     </description>
///   </item>
/// </list>
/// </para>
/// </remarks>
internal sealed class DefaultFilterExpressionBuilder<T> : IFilterExpressionBuilder<T>
{
    private readonly FilterExpressionCache<T> _cache;

    internal DefaultFilterExpressionBuilder(FilterExpressionCache<T> cache)
    {
        _cache = cache;
    }

    public Expression<Func<T, bool>>? Build(GridFilter filter, ColumnDescriptor<T> descriptor, FilterOptions options)
    {
        if (descriptor.PropertyExpression is null) return null;

        var param = Expression.Parameter(typeof(T), "x");
        var memberAccess = new FilterExpressionHelpers.ParameterReplacer(
                descriptor.PropertyExpression.Parameters[0], param)
            .Visit(descriptor.PropertyExpression.Body);

        // ── Type-agnostic operators ────────────────────────────────────────────
        if (filter.Operator == FilterOperator.IsNull)
            return Expression.Lambda<Func<T, bool>>(
                FilterExpressionHelpers.BuildIsNull(memberAccess), param);

        if (filter.Operator == FilterOperator.IsNotNull)
            return Expression.Lambda<Func<T, bool>>(
                Expression.Not(FilterExpressionHelpers.BuildIsNull(memberAccess)), param);

        // ── Strategy-based operators ───────────────────────────────────────────
        var opDescriptor = BuiltInOperators.Get(filter.Operator);
        var propertyType = descriptor.PropertyType ?? descriptor.PropertyExpression.ReturnType;
        var strategy = TypeStrategyRegistry.ResolveByType(propertyType);
        if (strategy is null) return null;

        // Value-required operators with a missing value: skip.
        if (opDescriptor.ValueRequirement == FilterValueRequirement.Required
            && string.IsNullOrEmpty(filter.Value))
            return null;

        // Cache key uses the raw string value so the key is stable across parses.
        var cacheKey = new FilterCacheKey(
            filter.ColumnKey,
            opDescriptor.Key,
            filter.Value,
            options.StringComparison);

        return _cache.GetOrAdd(cacheKey, () =>
        {
            var parsedValue = FilterValueParser.Parse(filter.Value, propertyType);
            var body = strategy.BuildExpression(memberAccess, opDescriptor, parsedValue, options);

            if (body is null) return null!; // sentinel; GetOrAdd caller will check and skip
            // Suppress constant-true (spurious WHERE 1=1).
            if (body is ConstantExpression { Value: true }) return null!;

            return Expression.Lambda<Func<T, bool>>(body, param);
        });
    }
}

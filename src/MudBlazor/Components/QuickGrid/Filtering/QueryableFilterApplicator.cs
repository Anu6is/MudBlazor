using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Applies a list of <see cref="GridFilter"/> records to an <see cref="IQueryable{T}"/>
/// as <c>Where</c> clauses, delegating all predicate construction to an
/// <see cref="IFilterExpressionBuilder{T}"/>.
/// </summary>
/// <remarks>
/// This class is a thin orchestration loop — it contains no operator dispatch, no
/// expression building logic, and no type-branching.  All of that lives in
/// <see cref="DefaultFilterExpressionBuilder{T}"/> (via <see cref="TypeStrategyRegistry"/>
/// and the per-type <see cref="ITypeFilterStrategy"/> implementations) or in a custom
/// <see cref="IFilterExpressionBuilder{T}"/> injected via
/// <see cref="QueryableDataSource{T}(IQueryable{T}, IFilterExpressionBuilder{T})"/>.
/// </remarks>
internal static class QueryableFilterApplicator<T>
{
    /// <summary>
    /// Applies all <paramref name="filters"/> to <paramref name="source"/>.
    /// Filters targeting unknown columns, columns without a
    /// <see cref="ColumnDescriptor{T}.PropertyExpression"/>, or filters for which
    /// <paramref name="builder"/> returns <see langword="null"/>, are silently skipped.
    /// </summary>
    public static IQueryable<T> Apply(IQueryable<T> source,
                                      IReadOnlyList<GridFilter> filters,
                                      IReadOnlyDictionary<string, ColumnDescriptor<T>> descriptors,
                                      FilterOptions options,
                                      IFilterExpressionBuilder<T> builder)
    {
        foreach (var filter in filters)
        {
            if (!descriptors.TryGetValue(filter.ColumnKey, out var descriptor))
                continue;

            var predicate = builder.Build(filter, descriptor, options);
            if (predicate is null) continue;

            source = source.Where(predicate);
        }

        return source;
    }

    // ── AND combiner (used by tests and export snapshot) ─────────────────────

    /// <summary>
    /// AND-combines two predicate lambda expressions by sharing the outer parameter.
    /// </summary>
    internal static Expression<Func<T, bool>> CombineAnd(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];
        var rightBody = new FilterExpressionHelpers.ParameterReplacer(
            right.Parameters[0], param).Visit(right.Body);
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, rightBody), param);
    }
}

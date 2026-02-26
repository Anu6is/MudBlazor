using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Applies <see cref="GridSort"/> and <see cref="GridGroupBy"/> instructions to an
/// <see cref="IQueryable{T}"/> by building <c>OrderBy</c> / <c>ThenBy</c> chains
/// from column property expressions in the <see cref="ColumnRegistry{T}"/>.
/// </summary>
/// <remarks>
/// Group pre-sorts (from <see cref="GridQuery.Groups"/>) are prepended before user
/// sorts so that same-group items are always consecutive within a paginated result.
/// This replaces <c>IGridGroupingService&lt;T&gt;.ApplyGroupSort</c> — that method
/// was an adapter concern, not a grid rendering concern.
/// </remarks>
internal static class QueryableSortApplicator<T>
{
    /// <summary>
    /// Applies group pre-sorts (from <paramref name="groups"/>) followed by user
    /// sorts (from <paramref name="sorts"/>) to <paramref name="source"/>.
    /// Columns with no <see cref="ColumnDescriptor{T}.PropertyExpression"/> are skipped.
    /// </summary>
    public static IQueryable<T> Apply(
        IQueryable<T> source,
        IReadOnlyList<GridGroupBy> groups,
        IReadOnlyList<GridSort> sorts,
        IReadOnlyDictionary<string, ColumnDescriptor<T>> descriptors)
    {
        IOrderedQueryable<T>? ordered = null;

        // 1. Group pre-sorts — lower Priority = outermost group = applied first.
        foreach (var group in groups.OrderBy(g => g.Priority))
        {
            if (!descriptors.TryGetValue(group.ColumnKey, out var descriptor))
                continue;
            if (descriptor.PropertyExpression is null)
                continue;

            var keyLambda = BuildObjectLambda(descriptor.PropertyExpression);
            ordered = ordered is null
                ? source.OrderBy(keyLambda)
                : ordered.ThenBy(keyLambda);
        }

        // 2. User sorts — lower Priority = primary sort.
        foreach (var sort in sorts.OrderBy(s => s.Priority))
        {
            if (!descriptors.TryGetValue(sort.ColumnKey, out var descriptor))
                continue;
            if (descriptor.PropertyExpression is null)
                continue;

            var keyLambda = BuildObjectLambda(descriptor.PropertyExpression);
            bool descending = sort.Direction == SortDirection.Descending;

            ordered = ordered is null
                ? (descending ? source.OrderByDescending(keyLambda) : source.OrderBy(keyLambda))
                : (descending ? ordered.ThenByDescending(keyLambda) : ordered.ThenBy(keyLambda));
        }

        return ordered ?? source;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Converts a <see cref="LambdaExpression"/> whose return type is <c>TValue</c>
    /// into <c>Expression&lt;Func&lt;T, object&gt;&gt;</c> for use with the non-generic
    /// <c>IQueryable.OrderBy(Expression&lt;Func&lt;T, TKey&gt;&gt;)</c> overload.
    /// </summary>
    private static Expression<Func<T, object>> BuildObjectLambda(LambdaExpression lambdaExpr)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var body = new FilterExpressionHelpers.ParameterReplacer(lambdaExpr.Parameters[0], param).Visit(lambdaExpr.Body);

        // Box value types so the lambda return type is object (required for the
        // non-generic OrderBy overload). EF Core's expression translator unwraps
        // this Convert node correctly.
        var boxed = lambdaExpr.ReturnType.IsValueType
            ? Expression.Convert(body, typeof(object))
            : body;

        return Expression.Lambda<Func<T, object>>(boxed, param);
    }
}

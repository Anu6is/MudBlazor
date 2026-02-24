// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Default implementation of <see cref="IGridFilterService{T}"/>.
/// Delegates predicate generation to <see cref="FilterExpressionGenerator{T}"/>
/// and applies the composed <c>Where</c> clause to an <see cref="IQueryable{T}"/>.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class GridFilterService<T> : IGridFilterService<T>
{
    private readonly FilterOptions _defaultOptions;

    /// <summary>
    /// Initialises a new instance using the provided <paramref name="defaultOptions"/>,
    /// or <see cref="FilterOptions.Default"/> when <see langword="null"/>.
    /// </summary>
    public GridFilterService(FilterOptions? defaultOptions = null)
    {
        _defaultOptions = defaultOptions ?? FilterOptions.Default;
    }

    /// <inheritdoc/>
    public IQueryable<T> ApplyFilters(
        IQueryable<T> source,
        IReadOnlyList<IFilterDefinition<T>> filters)
    {
        if (filters.Count == 0)
            return source;

        var combinedExpression = BuildFilterExpression(filters);

        // Constant true means nothing is actually filtered — skip the Where call
        // so EF Core does not emit a spurious WHERE 1=1.
        if (combinedExpression.Body is ConstantExpression { Value: true })
            return source;

        return source.Where(combinedExpression);
    }

    /// <inheritdoc/>
    public IEnumerable<T> ApplyQuickFilter(
        IEnumerable<T> source,
        Func<T, bool>? quickFilter)
    {
        return quickFilter is null ? source : source.Where(quickFilter);
    }

    /// <inheritdoc/>
    public Expression<Func<T, bool>> BuildFilterExpression(
        IReadOnlyList<IFilterDefinition<T>> filters)
    {
        if (filters.Count == 0)
            return _ => true;

        Expression<Func<T, bool>>? combined = null;

        foreach (var filter in filters)
        {
            var predicate = filter.GenerateExpression(_defaultOptions);

            // Skip constant-true predicates (filters with no value set).
            if (predicate.Body is ConstantExpression { Value: true })
                continue;

            combined = combined is null
                ? predicate
                : CombineAnd(combined, predicate);
        }

        return combined ?? (_ => true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// AND-combines two predicate expressions by sharing their parameter and
    /// composing their bodies.
    /// </summary>
    private static Expression<Func<T, bool>> CombineAnd(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = left.Parameters[0];
        var rightBody = new ParameterReplacer(right.Parameters[0], param).Visit(right.Body);
        var body = Expression.AndAlso(left.Body, rightBody);
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    private sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}

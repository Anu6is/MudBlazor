using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Applies filter definitions to an <see cref="IQueryable{T}"/> data source and provides
/// expression-building utilities for server-side consumers.
/// </summary>
/// <remarks>
/// <para>
/// The default implementation wraps <c>FilterExpressionGenerator&lt;T&gt;</c> (Phase 2B)
/// to produce composed LINQ predicate expressions, benefiting from EF Core deferred execution
/// when the source is an <c>IQueryable</c> backed by a database context.
/// </para>
/// <para>
/// Replace via DI to delegate predicate generation to an OData expression builder or any
/// other server-side filtering mechanism.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public interface IGridFilterService<T>
{
    /// <summary>
    /// Composes all active <paramref name="filters"/> into a single <c>Where</c> clause
    /// and applies it to <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The queryable source to filter.</param>
    /// <param name="filters">
    /// The active filter definitions. Each definition produces a predicate expression;
    /// all predicates are AND-combined. An empty list returns <paramref name="source"/> unmodified.
    /// </param>
    /// <returns>
    /// A filtered <see cref="IQueryable{T}"/>. When <paramref name="filters"/> is empty,
    /// returns <paramref name="source"/> unchanged.
    /// </returns>
    IQueryable<T> ApplyFilters(
        IQueryable<T> source,
        IReadOnlyList<IFilterDefinition<T>> filters);

    /// <summary>
    /// Applies an optional quick-filter predicate to an already-materialised sequence.
    /// </summary>
    /// <param name="source">The sequence to filter (typically already filtered by <see cref="ApplyFilters"/>).</param>
    /// <param name="quickFilter">
    /// An in-memory predicate corresponding to the user's quick-filter string.
    /// <see langword="null"/> returns <paramref name="source"/> unmodified.
    /// </param>
    /// <returns>
    /// A filtered <see cref="IEnumerable{T}"/>. When <paramref name="quickFilter"/> is
    /// <see langword="null"/>, returns <paramref name="source"/> unchanged.
    /// </returns>
    IEnumerable<T> ApplyQuickFilter(
        IEnumerable<T> source,
        Func<T, bool>? quickFilter);

    /// <summary>
    /// Builds a single composed predicate expression from all active filters without
    /// applying it to any source. Useful for server-side consumers that need the raw
    /// expression tree (e.g. to translate to an OData <c>$filter</c> clause).
    /// </summary>
    /// <param name="filters">
    /// The active filter definitions. Each definition's result is AND-combined.
    /// An empty list returns a constant-true expression.
    /// </param>
    Expression<Func<T, bool>> BuildFilterExpression(
        IReadOnlyList<IFilterDefinition<T>> filters);
}

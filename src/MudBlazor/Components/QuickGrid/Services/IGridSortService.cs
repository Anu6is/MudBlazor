using System.Diagnostics.CodeAnalysis;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Applies sort instructions to an <see cref="IQueryable{T}"/> data source.
/// </summary>
/// <remarks>
/// <para>
/// Replace via DI to delegate ordering to an OData or server-side query builder.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public interface IGridSortService<T>
{
    /// <summary>
    /// Applies one or more sort definitions to <paramref name="source"/>, returning an ordered
    /// <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="source">The (optionally pre-filtered) queryable source to sort.</param>
    /// <param name="sortDefinitions">
    /// The active sort definitions, ordered by SortDefinition.SortIndex ascending.
    /// The first entry becomes the primary <c>OrderBy</c>; subsequent entries become <c>ThenBy</c>.
    /// An empty list returns <paramref name="source"/> unmodified.
    /// </param>
    /// <returns>
    /// An <see cref="IOrderedQueryable{T}"/> if any sort definitions were applied;
    /// otherwise the original <paramref name="source"/> unchanged.
    /// </returns>
    [RequiresUnreferencedCode("May use expression tree compilation for in-memory sorting.")]
    IQueryable<T> ApplySort(IQueryable<T> source, IReadOnlyList<SortDefinition<T>> sortDefinitions);
}

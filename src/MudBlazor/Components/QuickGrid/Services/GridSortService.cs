// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System.Diagnostics.CodeAnalysis;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Default implementation of <see cref="IGridSortService{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// When all active <see cref="SortDefinition{T}"/> entries use
/// <see cref="SortDefinition{T}.SortExpression"/> with no custom
/// <see cref="SortDefinition{T}.Comparer"/>, the sort is applied entirely through
/// <see cref="IQueryable{T}"/> chaining — preserving EF Core deferred execution.
/// </para>
/// <para>
/// When any entry specifies a <see cref="SortDefinition{T}.Comparer"/>, the full
/// sequence is materialised via <c>AsEnumerable()</c> and sorted in-memory.
/// The result is re-projected via <c>AsQueryable()</c> so the return type
/// contract is maintained.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class GridSortService<T> : IGridSortService<T>
{
    /// <inheritdoc/>
    [RequiresUnreferencedCode("May use expression tree compilation for in-memory sorting.")]
    public IQueryable<T> ApplySort(IQueryable<T> source, IReadOnlyList<SortDefinition<T>> sortDefinitions)
    {
        if (sortDefinitions.Count == 0)
            return source;

        var definitions = sortDefinitions
            .OrderBy(sd => sd.SortIndex)
            .Where(sd => sd.SortExpression is not null || sd.Comparer is not null)
            .ToList();

        if (definitions.Count == 0)
            return source;

        return definitions.Any(sd => sd.Comparer is not null)
            ? ApplySortInMemory(source, definitions)
            : ApplySortQueryable(source, definitions);
    }

    // ── IQueryable path (EF Core compatible) ─────────────────────────────────
    private static IQueryable<T> ApplySortQueryable(IQueryable<T> source, List<SortDefinition<T>> definitions)
    {
        IOrderedQueryable<T>? ordered = null;

        foreach (var definition in definitions)
        {
            var expr = definition.SortExpression!;

            ordered = ordered is null
                ? (definition.Descending ? source.OrderByDescending(expr) : source.OrderBy(expr))
                : (definition.Descending ? ordered.ThenByDescending(expr) : ordered.ThenBy(expr));
        }

        return ordered ?? source;
    }

    // ── In-memory path (custom IComparer<object>) ────────────────────────────

    [RequiresUnreferencedCode("Compiling expression trees for in-memory sorting is not trimming-safe. " +
                              "Use IQueryable provider-based sorting when trimming or AOT is enabled.")]
    private static IQueryable<T> ApplySortInMemory(IQueryable<T> source, List<SortDefinition<T>> definitions)
    {
        var seq = source.AsEnumerable();
        IOrderedEnumerable<T>? ordered = null;

        foreach (var definition in definitions)
        {
            var keySelector = definition.SortExpression is not null
                ? definition.SortExpression.Compile()
                : item => item!;

            var comparer = definition.Comparer;

            if (ordered is null)
            {
                ordered = comparer is not null
                    ? (definition.Descending
                        ? seq.OrderByDescending(keySelector, comparer)
                        : seq.OrderBy(keySelector, comparer))
                    : (definition.Descending
                        ? seq.OrderByDescending(keySelector)
                        : seq.OrderBy(keySelector));
            }
            else
            {
                ordered = comparer is not null
                    ? (definition.Descending
                        ? ordered.ThenByDescending(keySelector, comparer)
                        : ordered.ThenBy(keySelector, comparer))
                    : (definition.Descending
                        ? ordered.ThenByDescending(keySelector)
                        : ordered.ThenBy(keySelector));
            }
        }

        return ((IEnumerable<T>?)ordered ?? seq).AsQueryable();
    }
}

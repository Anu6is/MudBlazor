using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

// ─────────────────────────────────────────────────────────────────────────────
// FilterCacheKey
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A value-type cache key that uniquely identifies a compiled filter predicate.
/// </summary>
/// <remarks>
/// <para>
/// The key is composed of four independent dimensions:
/// <list type="bullet">
///   <item>
///     <description>
///       <b><see cref="ColumnKey"/>:</b> the stable column identifier from
///       <see cref="GridFilter.ColumnKey"/>.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b><see cref="OperatorKey"/>:</b> the stable operator key from
///       <see cref="FilterOperatorDescriptor.Key"/> (e.g. <c>"string.contains"</c>).
///       Using the string key rather than the <see cref="FilterOperator"/> enum value
///       ensures cache entries are reusable if custom operators are registered alongside
///       built-in ones.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b><see cref="Value"/>:</b> the raw string value from <see cref="GridFilter.Value"/>.
///       Storing the pre-parse string avoids any type-specific hash instability and means
///       the cache key round-trips cleanly through JSON serialisation of
///       <see cref="GridSnapshot"/>.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b><see cref="StringComparison"/>:</b> ensures case-sensitive and case-insensitive
///       predicates for the same value are cached separately.
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// The entity type (<c>T</c>) is implicit: <see cref="FilterExpressionCache{T}"/> is
/// itself generic, so two caches for different entity types never share entries.
/// </para>
/// </remarks>
internal readonly record struct FilterCacheKey(
    string ColumnKey,
    string OperatorKey,
    string? Value,
    StringComparison StringComparison);

// ─────────────────────────────────────────────────────────────────────────────
// FilterExpressionCache<T>
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Thread-safe cache of compiled <see cref="Expression{TDelegate}"/> filter predicates
/// for entity type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// <para>
/// The cache is held as an instance field on <see cref="QueryableDataSource{T}"/> and is
/// valid for the lifetime of the data source instance.  Since
/// <see cref="IColumnRegistryAware{T}.Initialize"/> is called once (before first
/// <c>QueryAsync</c>) and columns do not hot-swap, the underlying column metadata used
/// to build predicates never changes, so cached entries are always valid.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row / entity type.</typeparam>
internal sealed class FilterExpressionCache<T>
{
    private readonly ConcurrentDictionary<FilterCacheKey, LambdaExpression> _cache = new();

    /// <summary>
    /// Returns a cached predicate for <paramref name="key"/> if one exists, otherwise
    /// invokes <paramref name="factory"/>, stores the result, and returns it.
    /// </summary>
    /// <param name="key">The cache key derived from the current <see cref="GridFilter"/>.</param>
    /// <param name="factory">
    /// Called on first access to build the predicate expression tree.  Must not return
    /// <see langword="null"/> — callers should only call <c>GetOrAdd</c> once they know a
    /// non-null predicate can be produced.
    /// </param>
    /// <returns>The cached or freshly constructed predicate expression.</returns>
    public Expression<Func<T, bool>>? GetOrAdd(FilterCacheKey key, Func<Expression<Func<T, bool>>> factory)
        => (Expression<Func<T, bool>>?)_cache.GetOrAdd(key, _ => factory());

    /// <summary>Returns the number of entries currently held in the cache.</summary>
    public int Count => _cache.Count;
}

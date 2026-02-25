namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Applies a list of <see cref="GridFilter"/> definitions to an <see cref="IEnumerable{T}"/>
/// using pre-compiled <see cref="ColumnDescriptor{T}.ValueSelector"/> delegates and the
/// per-type <see cref="ITypeFilterStrategy"/> implementations from
/// <see cref="TypeStrategyRegistry"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>AOT safety:</b> No expression trees are built here.  All comparisons use
/// pre-compiled <see cref="Func{T, TResult}"/> delegates (from
/// <see cref="ColumnDescriptor{T}.ValueSelector"/>) and direct strategy dispatch —
/// zero reflection at query time.
/// </para>
/// <para>
/// <b>Operator dispatch:</b> Instead of a monolithic <c>switch</c> on
/// <see cref="FilterOperator"/>, this class resolves the appropriate
/// <see cref="ITypeFilterStrategy"/> from <see cref="TypeStrategyRegistry"/> once per
/// filter application and delegates to <see cref="ITypeFilterStrategy.Evaluate"/>.
/// Type-agnostic operators (<c>IsNull</c>, <c>IsNotNull</c>) are intercepted here
/// before strategy resolution, as they require no type information.
/// </para>
/// </remarks>
internal static class InMemoryFilterEvaluator<T>
{
    /// <summary>
    /// Returns <paramref name="source"/> filtered to rows that pass all
    /// <paramref name="filters"/>.  Filters targeting unknown columns, columns with no
    /// <see cref="ColumnDescriptor{T}.ValueSelector"/>, or operators with no registered
    /// strategy, are silently skipped (rows pass).
    /// </summary>
    public static IEnumerable<T> Apply(
        IEnumerable<T> source,
        IReadOnlyList<GridFilter> filters,
        IReadOnlyDictionary<string, ColumnDescriptor<T>> descriptors,
        FilterOptions options)
    {
        if (filters.Count == 0) return source;

        // Resolve strategies once outside the per-row loop.
        var resolved = BuildResolvedFilters(filters, descriptors, options);
        if (resolved.Count == 0) return source;

        return source.Where(item => PassesAll(item, resolved));
    }

    // ── Resolution ────────────────────────────────────────────────────────────

    private readonly record struct ResolvedFilter(
        Func<T, object?> Selector,
        FilterOperatorDescriptor OpDescriptor,
        object? ParsedValue,
        FilterOptions Options,
        // Null strategy means the operator is type-agnostic (IsNull/IsNotNull).
        ITypeFilterStrategy? Strategy,
        bool IsNullCheck,
        bool IsNotNullCheck);

    private static List<ResolvedFilter> BuildResolvedFilters(
        IReadOnlyList<GridFilter> filters,
        IReadOnlyDictionary<string, ColumnDescriptor<T>> descriptors,
        FilterOptions options)
    {
        var result = new List<ResolvedFilter>(filters.Count);

        foreach (var filter in filters)
        {
            if (!descriptors.TryGetValue(filter.ColumnKey, out var descriptor)) continue;
            if (descriptor.ValueSelector is null) continue;

            // Type-agnostic null checks — no strategy or parsed value needed.
            if (filter.Operator == FilterOperator.IsNull)
            {
                result.Add(new ResolvedFilter(
                    descriptor.ValueSelector, null!, null, options,
                    Strategy: null, IsNullCheck: true, IsNotNullCheck: false));
                continue;
            }
            if (filter.Operator == FilterOperator.IsNotNull)
            {
                result.Add(new ResolvedFilter(
                    descriptor.ValueSelector, null!, null, options,
                    Strategy: null, IsNullCheck: false, IsNotNullCheck: true));
                continue;
            }

            var opDescriptor = BuiltInOperators.Get(filter.Operator);

            // Skip value-required operators when the value is missing.
            if (opDescriptor.ValueRequirement == FilterValueRequirement.Required
                && string.IsNullOrEmpty(filter.Value))
                continue;

            var propertyType = descriptor.PropertyType ?? typeof(object);
            var strategy = TypeStrategyRegistry.ResolveByType(propertyType);
            if (strategy is null) continue;

            var parsedValue = FilterValueParser.Parse(filter.Value, propertyType);

            result.Add(new ResolvedFilter(
                descriptor.ValueSelector, opDescriptor, parsedValue, options,
                Strategy: strategy, IsNullCheck: false, IsNotNullCheck: false));
        }

        return result;
    }

    // ── Per-row evaluation ────────────────────────────────────────────────────

    private static bool PassesAll(T item, List<ResolvedFilter> filters)
    {
        foreach (var f in filters)
        {
            var cellValue = f.Selector(item);

            if (f.IsNullCheck)    { if (cellValue is not null) return false; continue; }
            if (f.IsNotNullCheck) { if (cellValue is null)     return false; continue; }

            if (!f.Strategy!.Evaluate(cellValue, f.OpDescriptor, f.ParsedValue, f.Options))
                return false;
        }
        return true;
    }
}

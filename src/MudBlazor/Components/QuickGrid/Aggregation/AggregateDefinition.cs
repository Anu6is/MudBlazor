using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Defines an aggregate function to be computed and displayed in the grid footer cell
/// for a <c>PropertyColumn&lt;T, TProp&gt;</c>.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class AggregateDefinition<T>
{
    /// <summary>
    /// Gets or sets the aggregate function type.
    /// Determines the computation applied by <see cref="GetValue"/>.
    /// </summary>
    public AggregateType Type { get; set; } = AggregateType.Count;

    /// <summary>
    /// Gets or sets the optional display format string applied to the computed aggregate value.
    /// Follows standard .NET format strings (e.g. <c>"N2"</c>, <c>"C"</c>).
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets a fully custom aggregate function.
    /// When non-null, takes precedence over <see cref="Type"/>.
    /// The function receives the full current-page item collection and returns
    /// the value to display (already formatted as a string).
    /// </summary>
    public Func<IReadOnlyList<T>, string>? CustomAggregate { get; set; }

    /// <summary>
    /// Computes the aggregate display value for the given items and property expression.
    /// </summary>
    /// <typeparam name="TProp">The column property type.</typeparam>
    /// <param name="propertyExpression">Selector compiled from the column's <c>Property</c> parameter.</param>
    /// <param name="items">The current-page item collection.</param>
    /// <returns>The formatted aggregate string, or an empty string when <paramref name="items"/> is empty.</returns>
    [RequiresUnreferencedCode("Compiling property expression for aggregate computation is not trimming-safe.")]
    [RequiresDynamicCode("Compiling property expression for aggregate computation requires dynamic code generation.")]
    public string GetValue<TProp>(Expression<Func<T, TProp>>? propertyExpression, IReadOnlyList<T> items)
    {
        if (items.Count == 0)
            return string.Empty;

        if (CustomAggregate is not null)
            return CustomAggregate(items);

        if (Type == AggregateType.Count)
            return FormatValue(items.Count);

        if (propertyExpression is null)
            return string.Empty;

        var selector = propertyExpression.Compile();

        return Type switch
        {
            AggregateType.Sum => FormatValue(ComputeSum(items, selector)),
            AggregateType.Average => FormatValue(ComputeAverage(items, selector)),
            AggregateType.Min => FormatValue(items.Min(selector)),
            AggregateType.Max => FormatValue(items.Max(selector)),
            _ => string.Empty
        };
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private string FormatValue(object? value)
    {
        if (value is null)
            return string.Empty;

        return Format is not null && value is IFormattable formattable
            ? formattable.ToString(Format, System.Globalization.CultureInfo.CurrentCulture)
            : value.ToString() ?? string.Empty;
    }

    private static decimal ComputeSum<TProp>(IReadOnlyList<T> items, Func<T, TProp> selector)
        => items.Select(selector).Sum(x => Convert.ToDecimal(x));

    private static decimal ComputeAverage<TProp>(IReadOnlyList<T> items, Func<T, TProp> selector)
        => items.Select(selector).Average(x => Convert.ToDecimal(x));
}

/// <summary>
/// Specifies the aggregate computation applied by <see cref="AggregateDefinition{T}"/>.
/// </summary>
public enum AggregateType
{
    /// <summary>Displays the count of items on the current page.</summary>
    Count,

    /// <summary>Displays the sum of the column's numeric values.</summary>
    Sum,

    /// <summary>Displays the arithmetic mean of the column's numeric values.</summary>
    Average,

    /// <summary>Displays the minimum value in the column.</summary>
    Min,

    /// <summary>Displays the maximum value in the column.</summary>
    Max,

    /// <summary>
    /// A fully custom aggregate computed by <see cref="AggregateDefinition{T}.CustomAggregate"/>.
    /// </summary>
    Custom
}

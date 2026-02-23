namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Provides data to a column's <c>FooterTemplate</c> render fragment.
/// Gives access to the aggregate value computed by the column's
/// <c>AggregateDefinition</c> (when set), or <see langword="null"/> when no aggregate
/// is configured.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class FooterContext<T>
{
    /// <summary>
    /// Gets the pre-computed aggregate value for this column, or <see langword="null"/>
    /// when no <c>AggregateDefinition</c> is configured.
    /// </summary>
    public object? AggregateValue { get; init; }

    /// <summary>
    /// Gets the items currently visible in the grid (i.e. the current page's
    /// <c>DisplayItems</c>).  Consumers can use this to compute custom aggregates
    /// inside a <c>FooterTemplate</c>.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];
}

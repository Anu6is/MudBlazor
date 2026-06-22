namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Provides full filter state and mutation delegates to a column's <c>FilterTemplate</c>
/// render fragment.  Enables rich custom filter controls such as <c>MudSelect</c>,
/// <c>MudAutocomplete</c>, and <c>MudDateRangePicker</c>.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
/// <typeparam name="TProp">The column property type being filtered.</typeparam>
public sealed class FilterTemplateContext<T, TProp>
{
    /// <summary>
    /// Gets the currently active filter for this column, or <see langword="null"/> if no
    /// filter is applied.
    /// </summary>
    public IFilterDefinition<T>? CurrentFilter { get; init; }

    /// <summary>
    /// Gets the current filter value cast to <typeparamref name="TProp"/>, or
    /// <see langword="default"/> when no filter is active or the value is unset.
    /// </summary>
    public TProp? CurrentValue { get; init; }

    /// <summary>Gets the currently selected filter operator.</summary>
    public FilterOperator CurrentOperator { get; init; }

    /// <summary>
    /// Gets the optional list of allowed filter values for enum or list-backed columns.
    /// Populated from the column's <c>FilterItems</c> parameter.
    /// <see langword="null"/> when not configured.
    /// </summary>
    public IReadOnlyList<TProp>? FilterItems { get; init; }

    /// <summary>
    /// Gets a delegate that sets the filter value and triggers a grid refresh.
    /// Pass <see langword="null"/> to clear the filter value without removing the filter.
    /// </summary>
    public Func<TProp?, Task> SetValueAsync { get; init; } = _ => Task.CompletedTask;

    /// <summary>
    /// Gets a delegate that changes the filter operator and triggers a grid refresh.
    /// </summary>
    public Func<FilterOperator, Task> SetOperatorAsync { get; init; } = _ => Task.CompletedTask;

    /// <summary>Gets a delegate that removes the filter for this column entirely.</summary>
    public Func<Task> ClearAsync { get; init; } = () => Task.CompletedTask;
}

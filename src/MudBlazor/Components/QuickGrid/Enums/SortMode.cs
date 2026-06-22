namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Controls how many columns can be sorted simultaneously in <c>MudQuickGrid&lt;T&gt;</c>.
/// </summary>
public enum SortMode
{
    /// <summary>Sorting is disabled. Column headers show no sort affordance.</summary>
    None,

    /// <summary>
    /// At most one column can be sorted at a time. Clicking a new sortable column header clears
    /// the previous sort and applies the new one.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple columns can be sorted simultaneously. Clicking a column header while holding
    /// Shift adds it to (or toggles it within) the current sort list. Without Shift, clicking
    /// a header replaces all existing sorts with the clicked column.
    /// </summary>
    Multiple
}

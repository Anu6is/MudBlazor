namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// The result returned by <see cref="IGridDataSource{T}.QueryAsync"/> in response
/// to a <see cref="GridQuery"/>.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed record GridDataPage<T>
{
    /// <summary>
    /// Gets or initialises the items for the requested page (or virtualization window).
    /// </summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>
    /// Gets or initialises the total number of items that match all active filters
    /// <em>before</em> pagination is applied.
    /// Used by the grid to compute <c>TotalPages</c> and aria-rowcount.
    /// </summary>
    public required int TotalItemCount { get; init; }

    /// <summary>Returns an empty page with a total count of zero.</summary>
    public static GridDataPage<T> Empty { get; } = new() { Items = [], TotalItemCount = 0 };
}

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Describes a single visible column included in a <see cref="GridExportSnapshot{T}"/>.
/// <c>IGridExporter&lt;T&gt;</c> implementations use this to build column headers and
/// extract values from each row.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed record ExportColumn<T>
{
    /// <summary>
    /// Gets or initialises the stable column identifier.
    /// Matches <c>QuickGridColumnBase&lt;T&gt;.Id</c>.
    /// </summary>
    public string Id { get; init; } = "";

    /// <summary>
    /// Gets or initialises the human-readable column header title.
    /// Used as the column heading in the exported file.
    /// </summary>
    public string Title { get; init; } = "";

    /// <summary>
    /// Gets or initialises the function that extracts the display value for a row item.
    /// Returns <see langword="null"/> when the cell has no value.
    /// </summary>
    public Func<T, object?> ValueSelector { get; init; } = _ => null;

    /// <summary>
    /// Gets or initialises the optional .NET format string applied to the value returned
    /// by <see cref="ValueSelector"/> when the value implements <see cref="IFormattable"/>.
    /// For example <c>"N2"</c>, <c>"yyyy-MM-dd"</c>, <c>"C"</c>.
    /// <see langword="null"/> uses the default <c>ToString()</c> representation.
    /// </summary>
    public string? Format { get; init; }
}

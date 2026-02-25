namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Describes a single column filter applied within a <see cref="GridQuery"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="GridFilter"/> is intentionally non-generic.  Filter values are stored
/// as strings — <see cref="IGridDataSource{T}"/> implementations are responsible for
/// parsing them to the appropriate property type using the column's type metadata from
/// <see cref="ColumnDescriptor{T}.PropertyType"/>.
/// </para>
/// <para>
/// This design makes <see cref="GridFilter"/> natively JSON-serialisable without
/// round-trip loss, enabling <see cref="GridSnapshot"/> to be truly non-generic.
/// </para>
/// </remarks>
public sealed record GridFilter
{
    /// <summary>
    /// Gets or initialises the stable column key this filter targets.
    /// Must match <c>ColumnDescriptor&lt;T&gt;.Key</c>.
    /// </summary>
    public required string ColumnKey { get; init; }

    /// <summary>Gets or initialises the comparison operator applied by this filter.</summary>
    public required FilterOperator Operator { get; init; }

    /// <summary>
    /// Gets or initialises the primary filter value encoded as a culture-invariant string.
    /// <see langword="null"/> for operators that require no value (e.g.
    /// <see cref="FilterOperator.IsNull"/>, <see cref="FilterOperator.BooleanTrue"/>).
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// Gets or initialises the secondary filter value for range-based operators
    /// (e.g. a future <c>Between</c> operator). <see langword="null"/> when unused.
    /// </summary>
    public string? SecondaryValue { get; init; }
}

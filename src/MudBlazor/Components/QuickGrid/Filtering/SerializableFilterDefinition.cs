namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// A non-generic, JSON-serialisable snapshot of a filter definition.
/// Used in <see cref="GridSnapshot{T}"/> so that grid state can be persisted and
/// restored without needing to know the concrete property type at serialisation time.
/// </summary>
/// <remarks>
/// <c>QuickGridState&lt;T&gt;</c> can reconstruct the typed <see cref="IFilterDefinition{T}"/>
/// from this record using registered column type information when
/// <c>ApplySnapshotAsync</c> is called.
/// </remarks>
public sealed record SerializableFilterDefinition
{
    /// <summary>
    /// Gets or initialises the stable column identifier this filter targets.
    /// Must match <c>QuickGridColumnBase&lt;T&gt;.Id</c>.
    /// </summary>
    public string ColumnId { get; init; } = "";

    /// <summary>Gets or initialises the filter operator.</summary>
    public FilterOperator Operator { get; init; }

    /// <summary>
    /// Gets or initialises the filter value encoded as a string.
    /// The encoding is type-specific (e.g. ISO-8601 for <see cref="DateTime"/>,
    /// invariant culture for numerics, plain string otherwise).
    /// <see langword="null"/> for operators that require no value (e.g. <c>IsNull</c>,
    /// <c>BooleanTrue</c>).
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// Gets or initialises the assembly-qualified name of the column property type
    /// (e.g. <c>"System.Int32"</c>). Used during deserialisation to reconstruct the
    /// correct typed filter.
    /// </summary>
    public string? PropertyTypeName { get; init; }
}

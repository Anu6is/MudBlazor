using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// The concrete, strongly-typed implementation of <see cref="IFilterDefinition{T}"/>.
/// Binds a column property expression to a specific operator and comparison value.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
/// <typeparam name="TProp">The column property type being filtered.</typeparam>
public sealed class FilterDefinition<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T, TProp> : IFilterDefinition<T>
{
    /// <summary>
    /// Gets or sets the stable column identifier this filter targets.
    /// Must match <c>QuickGridColumnBase&lt;T&gt;.Id</c>.
    /// </summary>
    public string ColumnId { get; set; } = "";

    /// <inheritdoc/>
    public FilterOperator Operator { get; set; }

    /// <summary>
    /// Gets or sets the value to compare the column property against.
    /// May be <see langword="null"/> for operators that require no value
    /// (e.g. <see cref="FilterOperator.IsNull"/>, <see cref="FilterOperator.BooleanTrue"/>).
    /// </summary>
    public TProp? Value { get; set; }

    /// <summary>
    /// Gets or sets the property selector expression used to read the column value from
    /// a row item.  This expression is compiled and composed into an
    /// <see cref="IQueryable{T}"/> <c>Where</c> clause.
    /// </summary>
    public Expression<Func<T, TProp>>? PropertyExpression { get; set; }

    /// <inheritdoc/>
    /// <remarks>
    /// Expression generation is delegated to
    /// <c>FilterExpressionGenerator&lt;T&gt;</c> in Phase 2B.  This stub returns a
    /// constant <see langword="true"/> predicate so that the type compiles cleanly in
    /// Phase 1 without a dependency on the generator.
    /// </remarks>
    public Expression<Func<T, bool>> GenerateExpression(FilterOptions options)
    {
        // Full implementation supplied by FilterExpressionGenerator<T> in Phase 2B.
        // Returns a constant-true predicate as a compile-time placeholder.
        return _ => true;
    }

    /// <inheritdoc/>
    public SerializableFilterDefinition ToSerializable()
    {
        return new SerializableFilterDefinition
        {
            ColumnId = ColumnId,
            Operator = Operator,
            Value = SerializeValue(Value),
            PropertyTypeName = typeof(TProp).FullName,
        };
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string? SerializeValue(TProp? value)
    {
        if (value is null)
            return null;

        return value switch
        {
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
            DateOnly d => d.ToString("O", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }
}

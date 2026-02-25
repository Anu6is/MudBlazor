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
    /// Delegates to <see cref="FilterExpressionGenerator{T}.Generate{TProp}"/>,
    /// which builds a composable LINQ predicate expression tree for the configured
    /// <see cref="Operator"/> and <see cref="Value"/>.
    /// </remarks>
    [RequiresUnreferencedCode()]
    public Expression<Func<T, bool>> GenerateExpression(FilterOptions options)
        => FilterExpressionGenerator<T>.Generate(this, options);

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

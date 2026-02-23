using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Represents a single filter applied to a column.  All filter definitions implement this
/// interface so that <c>QuickGridState&lt;T&gt;</c> and <c>IGridFilterService&lt;T&gt;</c>
/// can work with a heterogeneous list of filters without knowing the concrete property type.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public interface IFilterDefinition<T>
{
    /// <summary>
    /// Gets the stable column identifier this filter targets.
    /// Must match <c>QuickGridColumnBase&lt;T&gt;.Id</c>.
    /// </summary>
    string ColumnId { get; }

    /// <summary>Gets the operator applied by this filter.</summary>
    FilterOperator Operator { get; }

    /// <summary>
    /// Builds a predicate expression that can be composed into an <see cref="IQueryable{T}"/>
    /// <c>Where</c> clause.
    /// </summary>
    /// <param name="options">Case-sensitivity and culture options for string comparisons.</param>
    Expression<Func<T, bool>> GenerateExpression(FilterOptions options);

    /// <summary>
    /// Returns a non-generic, JSON-serialisable representation of this filter suitable for
    /// inclusion in a <see cref="GridSnapshot{T}"/>.
    /// </summary>
    SerializableFilterDefinition ToSerializable();
}

using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// A type-specific filter strategy that knows how to evaluate a <see cref="FilterOperatorDescriptor"/>
/// against a particular property type via two execution paths:
/// an expression tree for <see cref="IQueryable{T}"/> / EF Core, and a direct delegate path for
/// in-memory collections.
/// </summary>
/// <remarks>
/// <para>
/// Strategies are registered per property type in <see cref="TypeStrategyRegistry"/>.
/// The registry's <see cref="TypeStrategyRegistry.ResolveByType"/> method returns the appropriate
/// implementation for a given <see cref="System.Type"/>, including <c>Nullable&lt;T&gt;</c>
/// variants (wrapped in <see cref="NullableStrategy"/>) and enum types (handled by
/// <see cref="EnumStrategy"/>).
/// </para>
/// <para>
/// <b>Type-agnostic operators</b> (<c>IsNull</c>, <c>IsNotNull</c>) are intercepted at the
/// orchestration layer — <see cref="DefaultFilterExpressionBuilder{T}"/> — before a strategy
/// is resolved, so no strategy needs to handle them.
/// </para>
/// <para>
/// Implementations should return <see langword="null"/> from <see cref="BuildExpression"/> when
/// the operator is not supported by this strategy or when a required value is missing, to signal
/// "skip this filter" rather than throw.
/// </para>
/// </remarks>
internal interface ITypeFilterStrategy
{
    /// <summary>
    /// Builds an <see cref="Expression"/> body (not a full lambda) that tests
    /// <paramref name="member"/> against <paramref name="value"/> using <paramref name="op"/>.
    /// </summary>
    /// <param name="member">
    /// The member-access expression for the column property, typed to this strategy's target type.
    /// Already unwrapped from <c>Nullable&lt;T&gt;</c> by <see cref="NullableStrategy"/> before
    /// reaching this call.
    /// </param>
    /// <param name="op">The resolved operator descriptor.</param>
    /// <param name="value">
    /// The parsed filter value, already converted to the appropriate CLR type by
    /// <see cref="FilterValueParser"/>. May be <see langword="null"/> for value-less operators.
    /// </param>
    /// <param name="options">String comparison and case-sensitivity options.</param>
    /// <returns>
    /// An expression body ready to wrap in a lambda, or <see langword="null"/> to skip the filter.
    /// </returns>
    Expression? BuildExpression(Expression member, FilterOperatorDescriptor op, object? value, FilterOptions options);

    /// <summary>
    /// Evaluates the filter directly against a boxed cell value from a
    /// <see cref="ColumnDescriptor{T}.ValueSelector"/> delegate, without building an
    /// expression tree.
    /// </summary>
    /// <param name="cellValue">
    /// The boxed cell value produced by the pre-compiled <c>ValueSelector</c> delegate.
    /// Already unwrapped from <c>Nullable&lt;T&gt;</c> by <see cref="NullableStrategy"/> before
    /// reaching this call; may be <see langword="null"/> for null-valued properties.
    /// </param>
    /// <param name="op">The resolved operator descriptor.</param>
    /// <param name="parsedValue">
    /// The parsed filter value from <see cref="FilterValueParser"/>. May be
    /// <see langword="null"/> for value-less operators.
    /// </param>
    /// <param name="options">String comparison and case-sensitivity options.</param>
    /// <returns>
    /// <see langword="true"/> if the row passes the filter; <see langword="false"/> to exclude it.
    /// </returns>
    bool Evaluate(object? cellValue, FilterOperatorDescriptor op, object? parsedValue, FilterOptions options);
}

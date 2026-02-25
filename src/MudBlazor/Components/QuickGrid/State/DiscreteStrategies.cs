using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

// ─────────────────────────────────────────────────────────────────────────────
// BoolStrategy
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// <see cref="ITypeFilterStrategy"/> for <see cref="bool"/> properties.
/// Handles <c>BooleanTrue</c> and <c>BooleanFalse</c> operators.
/// </summary>
internal sealed class BoolStrategy : ITypeFilterStrategy
{
    public static readonly BoolStrategy Instance = new();

    private BoolStrategy() { }

    public Expression? BuildExpression(
        Expression member,
        FilterOperatorDescriptor op,
        object? value,
        FilterOptions options)
    {
        var boolMember = FilterExpressionHelpers.EnsureType(member, typeof(bool));

        return op.EnumValue switch
        {
            FilterOperator.BooleanTrue => Expression.IsTrue(boolMember),
            FilterOperator.BooleanFalse => Expression.IsFalse(boolMember),
            _ => null,
        };
    }

    public bool Evaluate(
        object? cellValue,
        FilterOperatorDescriptor op,
        object? parsedValue,
        FilterOptions options)
        => op.EnumValue switch
        {
            FilterOperator.BooleanTrue => cellValue is true,
            FilterOperator.BooleanFalse => cellValue is false,
            _ => false,
        };
}

// ─────────────────────────────────────────────────────────────────────────────
// GuidStrategy
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// <see cref="ITypeFilterStrategy"/> for <see cref="Guid"/> properties.
/// Handles <c>GuidEqual</c> and <c>GuidNotEqual</c> operators.
/// </summary>
internal sealed class GuidStrategy : ITypeFilterStrategy
{
    public static readonly GuidStrategy Instance = new();

    private GuidStrategy() { }

    public Expression? BuildExpression(
        Expression member,
        FilterOperatorDescriptor op,
        object? value,
        FilterOptions options)
    {
        if (value is not Guid guidValue) return null;

        var (alignedMember, constant) =
            FilterExpressionHelpers.AlignTypes(member, guidValue, member.Type);

        return op.EnumValue switch
        {
            FilterOperator.GuidEqual => Expression.Equal(alignedMember, constant),
            FilterOperator.GuidNotEqual => Expression.NotEqual(alignedMember, constant),
            _ => null,
        };
    }

    public bool Evaluate(
        object? cellValue,
        FilterOperatorDescriptor op,
        object? parsedValue,
        FilterOptions options)
    {
        if (cellValue is not Guid cell || parsedValue is not Guid filter) return false;

        return op.EnumValue switch
        {
            FilterOperator.GuidEqual => cell == filter,
            FilterOperator.GuidNotEqual => cell != filter,
            _ => false,
        };
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// EnumStrategy
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// <see cref="ITypeFilterStrategy"/> for any <see cref="Enum"/> property type.
/// Handles <c>EnumIs</c> and <c>EnumIsNot</c> set-membership operators.
/// </summary>
/// <remarks>
/// <para>
/// A single shared <see cref="Instance"/> handles all enum types at runtime —
/// enum-type specifics are resolved by the expressions themselves (which carry
/// the concrete type) and by <see cref="Equals(object, object)"/> in the in-memory path.
/// </para>
/// <para>
/// The filter value may represent either a single enum value or a collection of values
/// (when the UI allows selecting multiple enum members). Both cases are supported.
/// </para>
/// </remarks>
internal sealed class EnumStrategy : ITypeFilterStrategy
{
    public static readonly EnumStrategy Instance = new();

    private EnumStrategy() { }

    // ── Expression path ───────────────────────────────────────────────────────

    public Expression? BuildExpression(
        Expression member,
        FilterOperatorDescriptor op,
        object? value,
        FilterOptions options)
    {
        if (value is null) return null;

        bool negate = op.EnumValue == FilterOperator.EnumIsNot;

        var values = ExtractValues(value);
        if (values.Count == 0)
            return Expression.Constant(negate); // no values → EnumIs = false, EnumIsNot = true

        // Build: m == v1 || m == v2 || ...
        Expression? combined = null;
        foreach (var v in values)
        {
            var (alignedMember, constant) =
                FilterExpressionHelpers.AlignTypes(member, v, member.Type);
            var eq = Expression.Equal(alignedMember, constant);
            combined = combined is null ? eq : Expression.OrElse(combined, eq);
        }

        return negate ? Expression.Not(combined!) : combined!;
    }

    // ── In-memory path ────────────────────────────────────────────────────────

    public bool Evaluate(
        object? cellValue,
        FilterOperatorDescriptor op,
        object? parsedValue,
        FilterOptions options)
    {
        if (cellValue is null || parsedValue is null) return false;

        bool negate = op.EnumValue == FilterOperator.EnumIsNot;
        var values = ExtractValues(parsedValue);
        bool contained = values.Any(v => Equals(cellValue, v));
        return negate ? !contained : contained;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IReadOnlyList<object> ExtractValues(object value)
    {
        if (value is System.Collections.IEnumerable enumerable and not string)
            return enumerable.Cast<object>().ToList();

        return [value];
    }
}

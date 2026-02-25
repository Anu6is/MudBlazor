using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// <see cref="ITypeFilterStrategy"/> for <see cref="string"/> properties.
/// Handles all <c>String.*</c> operators.
/// </summary>
internal sealed class StringStrategy : ITypeFilterStrategy
{
    public static readonly StringStrategy Instance = new();

    private StringStrategy() { }

    // ── Expression path ───────────────────────────────────────────────────────

    public Expression? BuildExpression(
        Expression member,
        FilterOperatorDescriptor op,
        object? value,
        FilterOptions options)
    {
        var stringMember = FilterExpressionHelpers.EnsureType(member, typeof(string));
        var comparison = Expression.Constant(options.StringComparison);
        var isNullOrEmpty = Expression.Call(FilterExpressionHelpers.StringIsNullOrEmptyMethod, stringMember);

        // Value-less operators.
        if (op.EnumValue == FilterOperator.StringEmpty) return isNullOrEmpty;
        if (op.EnumValue == FilterOperator.StringNotEmpty) return Expression.Not(isNullOrEmpty);

        if (value is not string s) return null;

        var valueExpr = Expression.Constant(s, typeof(string));

        Expression positive = op.EnumValue switch
        {
            FilterOperator.StringContains or FilterOperator.StringNotContains =>
                Expression.Call(stringMember, FilterExpressionHelpers.StringContainsMethod, valueExpr, comparison),

            FilterOperator.StringStartsWith =>
                Expression.Call(stringMember, FilterExpressionHelpers.StringStartsWithMethod, valueExpr, comparison),

            FilterOperator.StringEndsWith =>
                Expression.Call(stringMember, FilterExpressionHelpers.StringEndsWithMethod, valueExpr, comparison),

            FilterOperator.StringEqual or FilterOperator.StringNotEqual =>
                Expression.Call(FilterExpressionHelpers.StringStaticEqualsMethod, stringMember, valueExpr, comparison),

            _ => throw new NotSupportedException($"StringStrategy does not handle operator {op.Key}."),
        };

        // Not-variants: null/empty cell passes (not-contains on null cell = true).
        if (op.EnumValue is FilterOperator.StringNotContains or FilterOperator.StringNotEqual)
            return Expression.OrElse(isNullOrEmpty, Expression.Not(positive));

        // Positive operators: null/empty cell fails.
        return Expression.AndAlso(Expression.Not(isNullOrEmpty), positive);
    }

    // ── In-memory path ────────────────────────────────────────────────────────

    public bool Evaluate(object? cellValue, FilterOperatorDescriptor op, object? parsedValue, FilterOptions options)
    {
        // Value-less operators.
        if (op.EnumValue == FilterOperator.StringEmpty)
            return string.IsNullOrEmpty(cellValue as string);
        if (op.EnumValue == FilterOperator.StringNotEmpty)
            return !string.IsNullOrEmpty(cellValue as string);

        var cell = cellValue as string;

        if (parsedValue is not string filter) return false;

        var sc = options.StringComparison;

        return op.EnumValue switch
        {
            FilterOperator.StringContains =>
                cell is not null && cell.Contains(filter, sc),

            FilterOperator.StringNotContains =>
                string.IsNullOrEmpty(cell) || !cell!.Contains(filter, sc),

            FilterOperator.StringEqual =>
                string.Equals(cell, filter, sc),

            FilterOperator.StringNotEqual =>
                string.IsNullOrEmpty(cell) || !string.Equals(cell, filter, sc),

            FilterOperator.StringStartsWith =>
                cell is not null && cell.StartsWith(filter, sc),

            FilterOperator.StringEndsWith =>
                cell is not null && cell.EndsWith(filter, sc),

            _ => false,
        };
    }
}

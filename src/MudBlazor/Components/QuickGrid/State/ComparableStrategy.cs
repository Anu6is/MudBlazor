using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// <see cref="ITypeFilterStrategy"/> for numeric and other <see cref="IComparable{T}"/>
/// value types.  Handles all <c>Number.*</c> operators.
/// </summary>
/// <typeparam name="T">
/// The concrete comparable type (e.g. <see cref="int"/>, <see cref="double"/>,
/// <see cref="decimal"/>).
/// </typeparam>
/// <remarks>
/// A single static <see cref="Instance"/> per <typeparamref name="T"/> is registered in
/// <see cref="TypeStrategyRegistry"/>.  The generic constraint ensures the in-memory
/// comparison path uses <see cref="IComparable{T}.CompareTo"/> directly, avoiding
/// boxing that would occur with the non-generic <see cref="IComparable"/> interface.
/// </remarks>
internal sealed class ComparableStrategy<T> : ITypeFilterStrategy
    where T : IComparable<T>
{
    public static readonly ComparableStrategy<T> Instance = new();

    private ComparableStrategy() { }

    // ── Expression path ───────────────────────────────────────────────────────

    public Expression? BuildExpression(
        Expression member,
        FilterOperatorDescriptor op,
        object? value,
        FilterOptions options)
    {
        if (value is null) return null;

        var (alignedMember, constant) =
            FilterExpressionHelpers.AlignTypes(member, value, member.Type);

        return op.EnumValue switch
        {
            FilterOperator.NumberEqual =>
                Expression.Equal(alignedMember, constant),

            FilterOperator.NumberNotEqual =>
                Expression.NotEqual(alignedMember, constant),

            FilterOperator.NumberGreaterThan =>
                Expression.GreaterThan(alignedMember, constant),

            FilterOperator.NumberGreaterThanOrEqual =>
                Expression.GreaterThanOrEqual(alignedMember, constant),

            FilterOperator.NumberLessThan =>
                Expression.LessThan(alignedMember, constant),

            FilterOperator.NumberLessThanOrEqual =>
                Expression.LessThanOrEqual(alignedMember, constant),

            _ => null,
        };
    }

    // ── In-memory path ────────────────────────────────────────────────────────

    public bool Evaluate(
        object? cellValue,
        FilterOperatorDescriptor op,
        object? parsedValue,
        FilterOptions options)
    {
        if (cellValue is null || parsedValue is null) return false;

        // Convert both sides to T for a type-safe IComparable<T>.CompareTo call.
        T? cell = TryConvert(cellValue);
        T? filter = TryConvert(parsedValue);
        if (cell is null || filter is null) return false;

        int cmp = cell.CompareTo(filter);

        return op.EnumValue switch
        {
            FilterOperator.NumberEqual => cmp == 0,
            FilterOperator.NumberNotEqual => cmp != 0,
            FilterOperator.NumberGreaterThan => cmp > 0,
            FilterOperator.NumberGreaterThanOrEqual => cmp >= 0,
            FilterOperator.NumberLessThan => cmp < 0,
            FilterOperator.NumberLessThanOrEqual => cmp <= 0,
            _ => false,
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static T? TryConvert(object? value)
    {
        if (value is T typed) return typed;
        try
        {
            return (T)Convert.ChangeType(value, typeof(T),
                System.Globalization.CultureInfo.InvariantCulture);
        }
        catch { return default; }
    }
}

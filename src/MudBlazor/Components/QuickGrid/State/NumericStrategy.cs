// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// <see cref="ITypeFilterStrategy"/> for numeric value types.
/// Handles all <c>Number.*</c> operators.
/// </summary>
/// <typeparam name="T">
/// A numeric struct type (e.g. <see cref="int"/>, <see cref="double"/>,
/// <see cref="decimal"/>). The <c>struct</c> constraint guarantees that
/// <c>T?</c> is unambiguously <see cref="Nullable{T}"/> throughout this class,
/// eliminating nullable-reference-type annotation ambiguity.
/// </typeparam>
/// <remarks>
/// <para>
/// The <c>struct, <see cref="INumber{T}"/></c> constraint is tighter than the
/// former <c>IComparable&lt;T&gt;</c> constraint and is accurate: every type
/// registered in <see cref="TypeStrategyRegistry"/> for <c>Number.*</c> operators
/// is a primitive numeric struct. Non-numeric reference comparables (which do not
/// exist in this operator set) are not affected.
/// </para>
/// <para>
/// A single static <see cref="Instance"/> per <typeparamref name="T"/> is registered
/// in <see cref="TypeStrategyRegistry"/>. The in-memory path calls
/// <see cref="IComparable{T}.CompareTo"/> on the unwrapped <c>.Value</c> of each
/// <see cref="Nullable{T}"/>, with no boxing.
/// </para>
/// </remarks>
internal sealed class NumericStrategy<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]T> : ITypeFilterStrategy
    where T : struct, INumber<T>
{
    public static readonly NumericStrategy<T> Instance = new();

    private NumericStrategy() { }

    // ── Expression path ───────────────────────────────────────────────────────

    private static readonly PropertyInfo NullableHasValueProperty = typeof(T?).GetProperty(nameof(Nullable<T>.HasValue))!;

    private static readonly PropertyInfo NullableValueProperty = typeof(T?).GetProperty(nameof(Nullable<T>.Value))!;

    public Expression? BuildExpression(Expression member, FilterOperatorDescriptor op, object? value, FilterOptions options)
    {
        if (value is not T typed)
            return null;

        var constant = Expression.Constant(typed, typeof(T));

        if (member.Type == typeof(T?))
        {
            var hasValue = Expression.Property(member, NullableHasValueProperty);
            var valueProp = Expression.Property(member, NullableValueProperty);

            var comparison = BuildComparison(valueProp, constant, op);

            return Expression.AndAlso(hasValue, comparison);
        }

        return BuildComparison(member, constant, op);
    }

    private static BinaryExpression BuildComparison(Expression left, Expression right, FilterOperatorDescriptor op) => op.EnumValue switch
    {
        FilterOperator.NumberEqual => Expression.Equal(left, right),
        FilterOperator.NumberNotEqual => Expression.NotEqual(left, right),
        FilterOperator.NumberGreaterThan => Expression.GreaterThan(left, right),
        FilterOperator.NumberGreaterThanOrEqual => Expression.GreaterThanOrEqual(left, right),
        FilterOperator.NumberLessThan => Expression.LessThan(left, right),
        FilterOperator.NumberLessThanOrEqual => Expression.LessThanOrEqual(left, right),
        _ => throw new NotSupportedException()
    };

    // ── In-memory path ────────────────────────────────────────────────────────

    public bool Evaluate(object? cellValue, FilterOperatorDescriptor op, object? parsedValue, FilterOptions options)
    {
        if (cellValue is null || parsedValue is null) return false;

        var cell = TryConvert(cellValue);
        var filter = TryConvert(parsedValue);

        if (cell is null || filter is null) return false;

        // After the null guard, .Value is safe. CompareTo(T) is called on the
        // unwrapped struct — no boxing, no Nullable<T> overload ambiguity.
        var cmp = cell.Value.CompareTo(filter.Value);

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

    /// <summary>
    /// Attempts to convert <paramref name="value"/> to <typeparamref name="T"/>.
    /// Returns <see langword="null"/> (an empty <see cref="Nullable{T}"/>) on failure
    /// rather than throwing, so that <see cref="Evaluate"/> can silently skip
    /// unconvertible filter values.
    /// </summary>
    /// <remarks>
    /// The fast path (<c>value is T</c>) handles the common case where
    /// <see cref="FilterValueParser"/> already returned the correct type.
    /// </remarks>
    private static T? TryConvert(object? value)
    {
        if (value is null)
            return null;

        if (value is T typed)
            return typed;

        try
        {
            return value switch
            {
                byte b => T.CreateChecked(b),
                sbyte sb => T.CreateChecked(sb),
                short s => T.CreateChecked(s),
                ushort us => T.CreateChecked(us),
                int i => T.CreateChecked(i),
                uint ui => T.CreateChecked(ui),
                long l => T.CreateChecked(l),
                ulong ul => T.CreateChecked(ul),
                float f => T.CreateChecked(f),
                double d => T.CreateChecked(d),
                decimal m => T.CreateChecked(m),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }
}

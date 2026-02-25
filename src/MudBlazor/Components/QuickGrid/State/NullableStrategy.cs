using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Wraps an inner <see cref="ITypeFilterStrategy"/> to add <c>Nullable&lt;T&gt;</c> support.
/// </summary>
/// <remarks>
/// <para>
/// When a column property is <c>Nullable&lt;T&gt;</c>, the expression path must guard against
/// null values before accessing <c>.Value</c>.  This strategy adds that guard and then
/// delegates to the non-nullable inner strategy with the unwrapped <c>.Value</c> expression,
/// keeping each inner strategy clean and focused on its non-nullable type.
/// </para>
/// <para>
/// <b>Expression path:</b> produces <c>member.HasValue &amp;&amp; inner(member.Value, ...)</c>.
/// A null cell value therefore always fails for positive operators — callers wishing to
/// handle <c>IsNull</c> / <c>IsNotNull</c> should do so at the orchestration layer
/// (before a strategy is resolved) as those operators are type-agnostic.
/// </para>
/// <para>
/// <b>In-memory path:</b> a null <paramref name="cellValue"/> returns <see langword="false"/>
/// immediately; non-null delegates to the inner strategy unchanged (the boxed value is
/// already unwrapped by the <c>ValueSelector</c> delegate).
/// </para>
/// <para>
/// Instances are created on demand by <see cref="TypeStrategyRegistry.ResolveByType"/>
/// and cached for the process lifetime.
/// </para>
/// </remarks>
internal sealed class NullableStrategy : ITypeFilterStrategy
{
    private readonly ITypeFilterStrategy _inner;

    internal NullableStrategy(ITypeFilterStrategy inner)
    {
        _inner = inner;
    }

    // ── Expression path ───────────────────────────────────────────────────────

    public Expression? BuildExpression(
        Expression member,
        FilterOperatorDescriptor op,
        object? value,
        FilterOptions options)
    {
        // Unwrap Nullable<T>: guard on HasValue, then delegate with .Value accessor.
        var hasValue   = Expression.Property(member, nameof(Nullable<int>.HasValue));
        var valueAccess = Expression.Property(member, nameof(Nullable<int>.Value));

        var innerExpr = _inner.BuildExpression(valueAccess, op, value, options);
        if (innerExpr is null) return null;

        return Expression.AndAlso(hasValue, innerExpr);
    }

    // ── In-memory path ────────────────────────────────────────────────────────

    public bool Evaluate(
        object? cellValue,
        FilterOperatorDescriptor op,
        object? parsedValue,
        FilterOptions options)
    {
        // A null cell fails all non-null operators (IsNull/IsNotNull are handled upstream).
        if (cellValue is null) return false;

        // The ValueSelector for a Nullable<T> property boxes the inner value (or null),
        // so the non-null cellValue here is already the unwrapped T — delegate directly.
        return _inner.Evaluate(cellValue, op, parsedValue, options);
    }
}

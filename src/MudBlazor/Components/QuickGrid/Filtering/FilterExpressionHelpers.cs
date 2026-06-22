using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Shared expression-tree building utilities used by <see cref="ITypeFilterStrategy"/>
/// implementations and the orchestration layer.
/// </summary>
/// <remarks>
/// All members are static and allocation-free hot-path helpers. Reflection members are
/// cached as static fields — expression tree construction never uses <c>GetMethod</c> at
/// query time.
/// </remarks>
internal static class FilterExpressionHelpers
{
    // ── Cached reflection members ─────────────────────────────────────────────

    internal static readonly MethodInfo StringContainsMethod =
        typeof(string).GetMethod(nameof(string.Contains), [typeof(string), typeof(StringComparison)])!;

    internal static readonly MethodInfo StringStartsWithMethod =
        typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string), typeof(StringComparison)])!;

    internal static readonly MethodInfo StringEndsWithMethod =
        typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string), typeof(StringComparison)])!;

    internal static readonly MethodInfo StringStaticEqualsMethod =
        typeof(string).GetMethod(nameof(string.Equals), [typeof(string), typeof(string), typeof(StringComparison)])!;

    internal static readonly MethodInfo StringIsNullOrEmptyMethod =
        typeof(string).GetMethod(nameof(string.IsNullOrEmpty), [typeof(string)])!;

    // ── Null predicate ────────────────────────────────────────────────────────

    /// <summary>
    /// Builds an expression that is <see langword="true"/> when <paramref name="member"/> is null.
    /// For non-nullable value types, returns a constant <see langword="false"/> (they are never null).
    /// </summary>
    internal static Expression BuildIsNull(Expression member)
    {
        if (Nullable.GetUnderlyingType(member.Type) is not null || !member.Type.IsValueType)
            return Expression.Equal(member, Expression.Constant(null, member.Type));

        return Expression.Constant(false);
    }

    // ── Type coercion ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <paramref name="expr"/> unchanged if it is already <paramref name="targetType"/>,
    /// otherwise wraps it in a Expression.Convert node.
    /// </summary>
    internal static Expression EnsureType(Expression expr, Type targetType)
        => expr.Type == targetType ? expr : Expression.Convert(expr, targetType);

    /// <summary>
    /// Produces an aligned <c>(member, constant)</c> pair for equality or comparison expressions.
    /// Unwraps <c>Nullable&lt;T&gt;</c> on the member side and widens the parsed filter value to
    /// match the member's underlying type (e.g. <c>int</c> member vs <c>double</c> parsed value).
    /// </summary>
    [RequiresUnreferencedCode("Calls System.Linq.Expressions.Expression.Property(Expression, String)")]
    internal static (Expression member, Expression constant) AlignTypes(
        Expression memberAccess,
        object filterValue,
        Type memberType)
    {
        var underlying = Nullable.GetUnderlyingType(memberType) ?? memberType;
        var filterType = filterValue.GetType();

        object aligned;
        try
        {
            aligned = underlying != filterType
                ? Convert.ChangeType(filterValue, underlying, System.Globalization.CultureInfo.InvariantCulture)
                : filterValue;
        }
        catch
        {
            aligned = filterValue;
        }

        var member = Nullable.GetUnderlyingType(memberType) is not null
            ? Expression.Property(memberAccess, "Value")
            : memberAccess;

        return (member, Expression.Constant(aligned, underlying));
    }

    // ── Visitor ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Replaces all occurrences of <paramref name="oldParam"/> with <paramref name="newParam"/>
    /// in an expression tree.  Used to compose lambdas that share a single parameter.
    /// </summary>
    internal sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}

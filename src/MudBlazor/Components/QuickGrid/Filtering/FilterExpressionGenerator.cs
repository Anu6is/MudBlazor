// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Builds strongly-typed LINQ predicate <see cref="Expression{TDelegate}"/> trees
/// for every <see cref="FilterOperator"/> / property-type combination.
/// The expressions are composable into <c>IQueryable&lt;T&gt;.Where()</c> calls,
/// enabling EF Core server-side translation as well as in-memory LINQ evaluation.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
[RequiresUnreferencedCode("FilterExpressionGenerator builds LINQ expression trees using reflection.")]
[RequiresDynamicCode("FilterExpressionGenerator requires dynamic LINQ expression compilation.")]
public static class FilterExpressionGenerator<T>
{
    // ── Cached reflection members ─────────────────────────────────────────────

    private static readonly MethodInfo _stringContains =
        typeof(string).GetMethod(nameof(string.Contains), [typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo _stringStartsWith =
        typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo _stringEndsWith =
        typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo _stringEquals =
        typeof(string).GetMethod(nameof(string.Equals), [typeof(string), typeof(string), typeof(StringComparison)])!;

    private static readonly MethodInfo _stringIsNullOrEmpty =
        typeof(string).GetMethod(nameof(string.IsNullOrEmpty), [typeof(string)])!;

    // ── Public entry point ────────────────────────────────────────────────────

    /// <summary>
    /// Generates a predicate expression for the given <paramref name="filter"/>.
    /// Returns a constant-<see langword="true"/> expression when the filter value
    /// is not set and the operator requires a value.
    /// </summary>
    /// <typeparam name="TProp">The column property type.</typeparam>
    /// <param name="filter">The filter definition to compile.</param>
    /// <param name="options">Case sensitivity and culture options.</param>
    /// <returns>A predicate expression over <typeparamref name="T"/>.</returns>
    public static Expression<Func<T, bool>> Generate<TProp>(FilterDefinition<T, TProp> filter, FilterOptions options)
    {
        var param = Expression.Parameter(typeof(T), "x");

        if (filter.PropertyExpression is null)
            return AlwaysTrue();

        // Build: x => <property_access>
        var memberAccess = ReplaceParameter(filter.PropertyExpression, param);

        var body = BuildBody(filter.Operator, memberAccess, filter.Value, options);

        return body is null
            ? AlwaysTrue()
            : Expression.Lambda<Func<T, bool>>(body, param);
    }

    // ── Body builder dispatch ─────────────────────────────────────────────────

    private static Expression? BuildBody<TProp>(
        FilterOperator op,
        Expression memberAccess,
        TProp? filterValue,
        FilterOptions options)
    {
        // Null-check operators do not need a value.
        if (op == FilterOperator.IsNull)
            return BuildIsNull(memberAccess);
        if (op == FilterOperator.IsNotNull)
            return Expression.Not(BuildIsNull(memberAccess));

        // Boolean operators
        if (op == FilterOperator.BooleanTrue)
            return Expression.IsTrue(Expression.Convert(memberAccess, typeof(bool)));
        if (op == FilterOperator.BooleanFalse)
            return Expression.IsFalse(Expression.Convert(memberAccess, typeof(bool)));

        // String operators – always operate on string properties
        if (IsStringOperator(op))
            return BuildStringBody(op, memberAccess, filterValue as string, options);

        // Guard: remaining operators require a non-null value.
        if (filterValue is null)
            return null;

        return op switch
        {
            FilterOperator.NumberEqual or
            FilterOperator.GuidEqual => BuildEqual(memberAccess, filterValue),

            FilterOperator.NumberNotEqual or
            FilterOperator.GuidNotEqual => Expression.Not(BuildEqual(memberAccess, filterValue)),

            FilterOperator.NumberGreaterThan => BuildComparison(memberAccess, filterValue, ExpressionType.GreaterThan),
            FilterOperator.NumberGreaterThanOrEqual => BuildComparison(memberAccess, filterValue, ExpressionType.GreaterThanOrEqual),
            FilterOperator.NumberLessThan => BuildComparison(memberAccess, filterValue, ExpressionType.LessThan),
            FilterOperator.NumberLessThanOrEqual => BuildComparison(memberAccess, filterValue, ExpressionType.LessThanOrEqual),

            FilterOperator.DateIs => BuildDateEqual(memberAccess, filterValue),
            FilterOperator.DateIsNot => Expression.Not(BuildDateEqual(memberAccess, filterValue)),
            FilterOperator.DateBefore => BuildDateComparison(memberAccess, filterValue, ExpressionType.LessThan),
            FilterOperator.DateOnOrBefore => BuildDateComparison(memberAccess, filterValue, ExpressionType.LessThanOrEqual),
            FilterOperator.DateAfter => BuildDateComparison(memberAccess, filterValue, ExpressionType.GreaterThan),
            FilterOperator.DateOnOrAfter => BuildDateComparison(memberAccess, filterValue, ExpressionType.GreaterThanOrEqual),

            FilterOperator.EnumIs => BuildEnumContains(memberAccess, filterValue, negate: false),
            FilterOperator.EnumIsNot => BuildEnumContains(memberAccess, filterValue, negate: true),

            _ => null
        };
    }

    // ── String predicates ─────────────────────────────────────────────────────

    private static bool IsStringOperator(FilterOperator op) => op is
        FilterOperator.StringContains or FilterOperator.StringNotContains or
        FilterOperator.StringEqual or FilterOperator.StringNotEqual or
        FilterOperator.StringStartsWith or FilterOperator.StringEndsWith or
        FilterOperator.StringEmpty or FilterOperator.StringNotEmpty;

    private static Expression? BuildStringBody(
        FilterOperator op,
        Expression memberAccess,
        string? value,
        FilterOptions options)
    {
        var comparison = Expression.Constant(options.StringComparison);

        if (op == FilterOperator.StringEmpty)
            return Expression.Call(_stringIsNullOrEmpty, memberAccess);

        if (op == FilterOperator.StringNotEmpty)
            return Expression.Not(Expression.Call(_stringIsNullOrEmpty, memberAccess));

        // Guard: remaining string operators require a non-null value.
        if (value is null)
            return null;

        // Ensure member is typed as string for method calls.
        var stringMember = memberAccess.Type == typeof(string)
            ? memberAccess
            : Expression.Convert(memberAccess, typeof(string));

        var valueExpr = Expression.Constant(value, typeof(string));

        Expression callExpr = op switch
        {
            FilterOperator.StringContains or
            FilterOperator.StringNotContains =>
                Expression.Call(stringMember, _stringContains, valueExpr, comparison),

            FilterOperator.StringStartsWith =>
                Expression.Call(stringMember, _stringStartsWith, valueExpr, comparison),

            FilterOperator.StringEndsWith =>
                Expression.Call(stringMember, _stringEndsWith, valueExpr, comparison),

            FilterOperator.StringEqual or
            FilterOperator.StringNotEqual =>
                Expression.Call(_stringEquals, stringMember, valueExpr, comparison),

            _ => throw new NotSupportedException($"String operator {op} is not handled.")
        };

        // Wrap null-guard: if the string member can be null, short-circuit.
        Expression? nullGuard = memberAccess.Type == typeof(string)
            ? Expression.Not(Expression.Call(_stringIsNullOrEmpty, stringMember))
            : null;

        Expression body = nullGuard is not null
            ? Expression.AndAlso(nullGuard, callExpr)
            : callExpr;

        // Negate for Not variants.
        if (op is FilterOperator.StringNotContains or FilterOperator.StringNotEqual)
        {
            // For negations: empty or null strings should pass through (not-contains("")
            // on a null value should be true).
            body = Expression.OrElse(
                Expression.Call(_stringIsNullOrEmpty, stringMember),
                Expression.Not(
                    op == FilterOperator.StringNotContains
                        ? Expression.Call(stringMember, _stringContains, valueExpr, comparison)
                        : Expression.Call(_stringEquals, stringMember, valueExpr, comparison)
                )
            );
        }

        return body;
    }

    // ── Equality predicates ───────────────────────────────────────────────────

    private static Expression BuildEqual<TProp>(Expression memberAccess, TProp filterValue)
    {
        var converted = ConvertMemberIfNeeded(memberAccess, typeof(TProp));
        var valueExpr = Expression.Constant(filterValue, typeof(TProp));
        return Expression.Equal(converted, valueExpr);
    }

    // ── Comparison predicates (numeric / comparable) ──────────────────────────

    private static Expression BuildComparison<TProp>(
        Expression memberAccess,
        TProp filterValue,
        ExpressionType comparison)
    {
        var converted = ConvertMemberIfNeeded(memberAccess, typeof(TProp));
        var valueExpr = Expression.Constant(filterValue, typeof(TProp));
        return Expression.MakeBinary(comparison, converted, valueExpr);
    }

    // ── Date predicates ───────────────────────────────────────────────────────

    private static Expression BuildDateEqual<TProp>(Expression memberAccess, TProp filterValue)
    {
        // Compare date portion only (strip time component).
        var dateProp = typeof(DateTime).GetProperty(nameof(DateTime.Date));
        var dateOnlyProp = typeof(DateOnly).GetProperty("DayNumber"); // for DateOnly equality

        if (memberAccess.Type == typeof(DateTime) || memberAccess.Type == typeof(DateTime?))
        {
            var accessor = UnwrapNullable(memberAccess, out var nullCheck);
            var dateAccess = Expression.Property(accessor, dateProp!);
            var valueDate = ExtractDateOnly(filterValue);
            var valueDateExpr = Expression.Constant(valueDate, typeof(DateTime));
            Expression eq = Expression.Equal(dateAccess, valueDateExpr);
            return nullCheck is not null ? Expression.AndAlso(nullCheck, eq) : eq;
        }

        if (memberAccess.Type == typeof(DateTimeOffset) || memberAccess.Type == typeof(DateTimeOffset?))
        {
            var accessor = UnwrapNullable(memberAccess, out var nullCheck);
            var dateAccess = Expression.Property(Expression.Property(accessor, "Date"), dateProp!);
            var valueDate = ExtractDateOnly(filterValue);
            var valueDateExpr = Expression.Constant(valueDate, typeof(DateTime));
            Expression eq = Expression.Equal(dateAccess, valueDateExpr);
            return nullCheck is not null ? Expression.AndAlso(nullCheck, eq) : eq;
        }

        if (memberAccess.Type == typeof(DateOnly) || memberAccess.Type == typeof(DateOnly?))
        {
            var accessor = UnwrapNullable(memberAccess, out var nullCheck);
            var converted = Expression.Convert(accessor, typeof(DateOnly));
            var valueExpr = Expression.Constant(filterValue is DateOnly d ? d : default(DateOnly), typeof(DateOnly));
            Expression eq = Expression.Equal(converted, valueExpr);
            return nullCheck is not null ? Expression.AndAlso(nullCheck, eq) : eq;
        }

        return BuildEqual(memberAccess, filterValue);
    }

    private static Expression BuildDateComparison<TProp>(
        Expression memberAccess, TProp filterValue, ExpressionType comparison)
    {
        if (memberAccess.Type == typeof(DateTime) || memberAccess.Type == typeof(DateTime?))
        {
            var accessor = UnwrapNullable(memberAccess, out var nullCheck);
            var valueDate = ExtractDateOnly(filterValue);
            var datePart = Expression.Property(accessor, typeof(DateTime).GetProperty(nameof(DateTime.Date))!);
            var valueDateExpr = Expression.Constant(valueDate, typeof(DateTime));
            Expression cmp = Expression.MakeBinary(comparison, datePart, valueDateExpr);
            return nullCheck is not null ? Expression.AndAlso(nullCheck, cmp) : cmp;
        }

        if (memberAccess.Type == typeof(DateTimeOffset) || memberAccess.Type == typeof(DateTimeOffset?))
        {
            // Similar pattern — extract .Date.Date (the DateTime date-part)
            var accessor = UnwrapNullable(memberAccess, out var nullCheck);
            var dateProp = typeof(DateTime).GetProperty(nameof(DateTime.Date))!;
            var datePart = Expression.Property(
                Expression.Property(accessor, nameof(DateTimeOffset.Date)), dateProp);
            var valueDate = ExtractDateOnly(filterValue);
            var valueDateExpr = Expression.Constant(valueDate, typeof(DateTime));
            Expression cmp = Expression.MakeBinary(comparison, datePart, valueDateExpr);
            return nullCheck is not null ? Expression.AndAlso(nullCheck, cmp) : cmp;
        }

        if (memberAccess.Type == typeof(DateOnly) || memberAccess.Type == typeof(DateOnly?))
        {
            var accessor = UnwrapNullable(memberAccess, out var nullCheck);
            var valueExpr = Expression.Constant(
                filterValue is DateOnly d ? d : (filterValue is DateTime dt ? DateOnly.FromDateTime(dt) : default),
                typeof(DateOnly));
            Expression cmp = Expression.MakeBinary(comparison, accessor, valueExpr);
            return nullCheck is not null ? Expression.AndAlso(nullCheck, cmp) : cmp;
        }

        return BuildComparison(memberAccess, filterValue, comparison);
    }

    private static DateTime ExtractDateOnly<TProp>(TProp value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        return value switch
        {
            DateTime dt => dt.Date,
            DateTimeOffset dto => dto.Date.Date,
            DateOnly d => d.ToDateTime(TimeOnly.MinValue),

            _ => throw new NotSupportedException(
                    $"Type '{typeof(TProp)}' is not supported for date extraction.")
        };
    }

    // ── Null predicate ────────────────────────────────────────────────────────

    private static Expression BuildIsNull(Expression memberAccess)
    {
        if (!memberAccess.Type.IsValueType)
            return Expression.Equal(memberAccess, Expression.Constant(null, memberAccess.Type));

        // Nullable<T>
        if (Nullable.GetUnderlyingType(memberAccess.Type) is not null)
            return Expression.Equal(memberAccess, Expression.Constant(null, memberAccess.Type));

        // Non-nullable value types can never be null.
        return Expression.Constant(false);
    }

    // ── Enum set membership predicate ─────────────────────────────────────────

    private static Expression BuildEnumContains<TProp>(
        Expression memberAccess,
        TProp filterValue,
        bool negate)
    {
        // filterValue is expected to be IEnumerable<TProp> for EnumIs / EnumIsNot.
        // Handle single-value fallback to equality for convenience.
        if (filterValue is System.Collections.IEnumerable enumerable)
        {
            var values = enumerable.Cast<object>().ToList();
            if (values.Count == 0)
                return negate ? Expression.Constant(true) : Expression.Constant(false);

            Expression? combined = null;
            foreach (var v in values)
            {
                var eq = Expression.Equal(
                    ConvertMemberIfNeeded(memberAccess, v.GetType()),
                    Expression.Constant(v));
                combined = combined is null ? eq : Expression.OrElse(combined, eq);
            }

            return negate ? Expression.Not(combined!) : combined!;
        }

        // Single-value fallback.
        var singleEq = BuildEqual(memberAccess, filterValue);
        return negate ? Expression.Not(singleEq) : singleEq;
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private static Expression ConvertMemberIfNeeded(Expression memberAccess, Type targetType)
    {
        var underlyingTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;
        var underlyingMember = Nullable.GetUnderlyingType(memberAccess.Type) ?? memberAccess.Type;

        if (memberAccess.Type == targetType)
            return memberAccess;

        if (underlyingMember == underlyingTarget && Nullable.GetUnderlyingType(memberAccess.Type) is not null)
            return Expression.Property(memberAccess, "Value"); // unwrap Nullable<T>.Value

        return Expression.Convert(memberAccess, targetType);
    }

    /// <summary>
    /// Unwraps a <c>Nullable&lt;T&gt;</c> expression to its inner value,
    /// returning a non-null guard expression via <paramref name="nullCheck"/>.
    /// For non-nullable expressions, <paramref name="nullCheck"/> is <see langword="null"/>.
    /// </summary>
    private static Expression UnwrapNullable(Expression memberAccess, out Expression? nullCheck)
    {
        if (Nullable.GetUnderlyingType(memberAccess.Type) is not null)
        {
            nullCheck = Expression.Property(memberAccess, "HasValue");
            return Expression.Property(memberAccess, "Value");
        }

        nullCheck = null;
        return memberAccess;
    }

    /// <summary>
    /// Replaces the parameter in a property-selector expression so it shares the
    /// top-level <c>ParameterExpression</c> of the outer predicate lambda.
    /// </summary>
    private static Expression ReplaceParameter<TProp>(
        Expression<Func<T, TProp>> expression,
        ParameterExpression newParam)
    {
        return new ParameterReplacer(expression.Parameters[0], newParam)
            .Visit(expression.Body);
    }

    private static Expression<Func<T, bool>> AlwaysTrue()
        => _ => true;

    // ── Parameter replacer visitor ────────────────────────────────────────────

    private sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}

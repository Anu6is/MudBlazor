using System.Linq.Expressions;
using System.Reflection;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// <see cref="ITypeFilterStrategy"/> for date types: <see cref="DateTime"/>,
/// <see cref="DateTimeOffset"/>, and <see cref="DateOnly"/>.
/// Handles all <c>Date.*</c> operators by comparing the <i>date portion only</i>
/// (time components are ignored), matching the behaviour of MudDataGrid's date filters.
/// </summary>
/// <remarks>
/// A single <see cref="Instance"/> handles all three date types by branching internally
/// on <c>member.Type</c> — no separate strategy class per date type is needed.
/// </remarks>
internal sealed class DateStrategy : ITypeFilterStrategy
{
    public static readonly DateStrategy Instance = new();

    private static readonly PropertyInfo _dateTimeDateProp =
        typeof(DateTime).GetProperty(nameof(DateTime.Date))!;

    private static readonly MethodInfo _dateOnlyFromDateTimeMethod =
        typeof(DateOnly).GetMethod(nameof(DateOnly.FromDateTime), [typeof(DateTime)])!;

    private DateStrategy() { }

    // ── Expression path ───────────────────────────────────────────────────────

    public Expression? BuildExpression(
        Expression member,
        FilterOperatorDescriptor op,
        object? value,
        FilterOptions options)
    {
        if (value is null) return null;

        // Extract the date-part accessor expression and the matching constant
        // for the filter value, both in the same type.
        var (dateMember, dateConstant) = ExtractDateParts(member, value);
        if (dateMember is null) return null;

        return op.EnumValue switch
        {
            FilterOperator.DateIs =>
                Expression.Equal(dateMember, dateConstant),

            FilterOperator.DateIsNot =>
                Expression.NotEqual(dateMember, dateConstant),

            FilterOperator.DateBefore =>
                Expression.LessThan(dateMember, dateConstant),

            FilterOperator.DateOnOrBefore =>
                Expression.LessThanOrEqual(dateMember, dateConstant),

            FilterOperator.DateAfter =>
                Expression.GreaterThan(dateMember, dateConstant),

            FilterOperator.DateOnOrAfter =>
                Expression.GreaterThanOrEqual(dateMember, dateConstant),

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
        var cellDate = ToDatePart(cellValue);
        var filterDate = ToDatePart(parsedValue);
        if (cellDate is null || filterDate is null) return false;

        int cmp = cellDate.Value.CompareTo(filterDate.Value);

        return op.EnumValue switch
        {
            FilterOperator.DateIs          => cmp == 0,
            FilterOperator.DateIsNot       => cmp != 0,
            FilterOperator.DateBefore      => cmp < 0,
            FilterOperator.DateOnOrBefore  => cmp <= 0,
            FilterOperator.DateAfter       => cmp > 0,
            FilterOperator.DateOnOrAfter   => cmp >= 0,
            _ => false,
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Produces an (accessor expression, constant expression) pair that both represent
    /// the date portion of the column member and the filter value in a comparable form.
    /// </summary>
    private static (Expression? dateMember, Expression dateConstant) ExtractDateParts(
        Expression member, object filterValue)
    {
        // Unwrap Nullable<T> if present — NullableStrategy adds the HasValue guard before
        // calling here, so we receive the already-unwrapped .Value accessor.
        var effectiveMember = member;
        var memberType = member.Type;
        var underlying = Nullable.GetUnderlyingType(memberType) ?? memberType;

        if (underlying == typeof(DateTime))
        {
            // Access .Date to strip the time component.
            var datePart = Expression.Property(effectiveMember, _dateTimeDateProp);
            var filterDate = ToDateTimeDate(filterValue);
            return (datePart, Expression.Constant(filterDate, typeof(DateTime)));
        }

        if (underlying == typeof(DateTimeOffset))
        {
            // DateTimeOffset.Date returns a DateTime (already strips time).
            var datePart = Expression.Property(
                Expression.Property(effectiveMember, nameof(DateTimeOffset.Date)),
                _dateTimeDateProp);
            var filterDate = ToDateTimeDate(filterValue);
            return (datePart, Expression.Constant(filterDate, typeof(DateTime)));
        }

        if (underlying == typeof(DateOnly))
        {
            // DateOnly has no time component — compare directly.
            var filterDateOnly = ToDateOnly(filterValue);
            return (effectiveMember, Expression.Constant(filterDateOnly, typeof(DateOnly)));
        }

        return (null, Expression.Constant(null));
    }

    private static DateTime ToDateTimeDate(object value) => value switch
    {
        DateTime dt          => dt.Date,
        DateTimeOffset dto   => dto.Date,
        DateOnly d           => d.ToDateTime(TimeOnly.MinValue),
        _ => default,
    };

    private static DateOnly ToDateOnly(object value) => value switch
    {
        DateOnly d           => d,
        DateTime dt          => DateOnly.FromDateTime(dt),
        DateTimeOffset dto   => DateOnly.FromDateTime(dto.Date),
        _ => default,
    };

    private static DateOnly? ToDatePart(object? value) => value switch
    {
        DateOnly d           => d,
        DateTime dt          => DateOnly.FromDateTime(dt.Date),
        DateTimeOffset dto   => DateOnly.FromDateTime(dto.Date),
        _ => null,
    };
}

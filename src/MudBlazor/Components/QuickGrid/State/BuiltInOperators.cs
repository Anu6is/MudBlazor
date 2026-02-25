namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Static registry of <see cref="FilterOperatorDescriptor"/> instances for every
/// <see cref="FilterOperator"/> enum value defined in the built-in operator set.
/// </summary>
/// <remarks>
/// Provides two lookup paths:
/// <list type="bullet">
///   <item><description>
///     <see cref="Get(FilterOperator)"/> — resolves the descriptor for a known enum value;
///     used by <see cref="TypeStrategyRegistry"/> and <see cref="DefaultFilterExpressionBuilder{T}"/>.
///   </description></item>
///   <item><description>
///     <see cref="For(Type)"/> — returns all operators whose <see cref="FilterOperatorDescriptor.SupportedTypes"/>
///     include the given property type; used by filter UI components to populate operator dropdowns
///     without hardcoded type-to-operator mappings.
///   </description></item>
/// </list>
/// </remarks>
internal static class BuiltInOperators
{
    // ── Null / existence ──────────────────────────────────────────────────────

    public static readonly FilterOperatorDescriptor IsNull =
        new(FilterOperator.IsNull, "any.null", FilterValueRequirement.None);

    public static readonly FilterOperatorDescriptor IsNotNull =
        new(FilterOperator.IsNotNull, "any.not_null", FilterValueRequirement.None);

    // ── Boolean ───────────────────────────────────────────────────────────────

    public static readonly FilterOperatorDescriptor BooleanTrue =
        new(FilterOperator.BooleanTrue, "bool.true", FilterValueRequirement.None, typeof(bool));

    public static readonly FilterOperatorDescriptor BooleanFalse =
        new(FilterOperator.BooleanFalse, "bool.false", FilterValueRequirement.None, typeof(bool));

    // ── String ────────────────────────────────────────────────────────────────

    public static readonly FilterOperatorDescriptor StringContains =
        new(FilterOperator.StringContains, "string.contains", FilterValueRequirement.Required, typeof(string));

    public static readonly FilterOperatorDescriptor StringNotContains =
        new(FilterOperator.StringNotContains, "string.not_contains", FilterValueRequirement.Required, typeof(string));

    public static readonly FilterOperatorDescriptor StringEqual =
        new(FilterOperator.StringEqual, "string.equal", FilterValueRequirement.Required, typeof(string));

    public static readonly FilterOperatorDescriptor StringNotEqual =
        new(FilterOperator.StringNotEqual, "string.not_equal", FilterValueRequirement.Required, typeof(string));

    public static readonly FilterOperatorDescriptor StringStartsWith =
        new(FilterOperator.StringStartsWith, "string.starts_with", FilterValueRequirement.Required, typeof(string));

    public static readonly FilterOperatorDescriptor StringEndsWith =
        new(FilterOperator.StringEndsWith, "string.ends_with", FilterValueRequirement.Required, typeof(string));

    public static readonly FilterOperatorDescriptor StringEmpty =
        new(FilterOperator.StringEmpty, "string.empty", FilterValueRequirement.None, typeof(string));

    public static readonly FilterOperatorDescriptor StringNotEmpty =
        new(FilterOperator.StringNotEmpty, "string.not_empty", FilterValueRequirement.None, typeof(string));

    // ── Numeric ───────────────────────────────────────────────────────────────

    private static readonly Type[] _numericTypes =
    [
        typeof(int), typeof(long), typeof(short), typeof(byte),
        typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte),
        typeof(double), typeof(float), typeof(decimal),
    ];

    public static readonly FilterOperatorDescriptor NumberEqual =
        new(FilterOperator.NumberEqual, "number.equal", FilterValueRequirement.Required, _numericTypes);

    public static readonly FilterOperatorDescriptor NumberNotEqual =
        new(FilterOperator.NumberNotEqual, "number.not_equal", FilterValueRequirement.Required, _numericTypes);

    public static readonly FilterOperatorDescriptor NumberGreaterThan =
        new(FilterOperator.NumberGreaterThan, "number.gt", FilterValueRequirement.Required, _numericTypes);

    public static readonly FilterOperatorDescriptor NumberGreaterThanOrEqual =
        new(FilterOperator.NumberGreaterThanOrEqual, "number.gte", FilterValueRequirement.Required, _numericTypes);

    public static readonly FilterOperatorDescriptor NumberLessThan =
        new(FilterOperator.NumberLessThan, "number.lt", FilterValueRequirement.Required, _numericTypes);

    public static readonly FilterOperatorDescriptor NumberLessThanOrEqual =
        new(FilterOperator.NumberLessThanOrEqual, "number.lte", FilterValueRequirement.Required, _numericTypes);

    // ── Date ──────────────────────────────────────────────────────────────────

    private static readonly Type[] _dateTypes =
        [typeof(DateTime), typeof(DateTimeOffset), typeof(DateOnly)];

    public static readonly FilterOperatorDescriptor DateIs =
        new(FilterOperator.DateIs, "date.is", FilterValueRequirement.Required, _dateTypes);

    public static readonly FilterOperatorDescriptor DateIsNot =
        new(FilterOperator.DateIsNot, "date.is_not", FilterValueRequirement.Required, _dateTypes);

    public static readonly FilterOperatorDescriptor DateBefore =
        new(FilterOperator.DateBefore, "date.before", FilterValueRequirement.Required, _dateTypes);

    public static readonly FilterOperatorDescriptor DateOnOrBefore =
        new(FilterOperator.DateOnOrBefore, "date.on_or_before", FilterValueRequirement.Required, _dateTypes);

    public static readonly FilterOperatorDescriptor DateAfter =
        new(FilterOperator.DateAfter, "date.after", FilterValueRequirement.Required, _dateTypes);

    public static readonly FilterOperatorDescriptor DateOnOrAfter =
        new(FilterOperator.DateOnOrAfter, "date.on_or_after", FilterValueRequirement.Required, _dateTypes);

    // ── Enum ──────────────────────────────────────────────────────────────────
    // SupportedTypes intentionally empty — enum operators apply to any enum type.
    // TypeStrategyRegistry.Resolve handles the IsEnum check.

    public static readonly FilterOperatorDescriptor EnumIs =
        new(FilterOperator.EnumIs, "enum.is", FilterValueRequirement.Required);

    public static readonly FilterOperatorDescriptor EnumIsNot =
        new(FilterOperator.EnumIsNot, "enum.is_not", FilterValueRequirement.Required);

    // ── Guid ──────────────────────────────────────────────────────────────────

    public static readonly FilterOperatorDescriptor GuidEqual =
        new(FilterOperator.GuidEqual, "guid.equal", FilterValueRequirement.Required, typeof(Guid));

    public static readonly FilterOperatorDescriptor GuidNotEqual =
        new(FilterOperator.GuidNotEqual, "guid.not_equal", FilterValueRequirement.Required, typeof(Guid));

    // ── Lookup tables ─────────────────────────────────────────────────────────

    private static readonly Dictionary<FilterOperator, FilterOperatorDescriptor> _byEnum;
    private static readonly IReadOnlyList<FilterOperatorDescriptor> _all;

    static BuiltInOperators()
    {
        _all =
        [
            IsNull, IsNotNull,
            BooleanTrue, BooleanFalse,
            StringContains, StringNotContains, StringEqual, StringNotEqual,
            StringStartsWith, StringEndsWith, StringEmpty, StringNotEmpty,
            NumberEqual, NumberNotEqual, NumberGreaterThan, NumberGreaterThanOrEqual,
            NumberLessThan, NumberLessThanOrEqual,
            DateIs, DateIsNot, DateBefore, DateOnOrBefore, DateAfter, DateOnOrAfter,
            EnumIs, EnumIsNot,
            GuidEqual, GuidNotEqual,
        ];

        _byEnum = _all.ToDictionary(d => d.EnumValue);
    }

    /// <summary>
    /// Returns the <see cref="FilterOperatorDescriptor"/> for <paramref name="op"/>.
    /// Throws <see cref="ArgumentOutOfRangeException"/> for unknown values.
    /// </summary>
    public static FilterOperatorDescriptor Get(FilterOperator op)
        => _byEnum.TryGetValue(op, out var d)
            ? d
            : throw new ArgumentOutOfRangeException(nameof(op), op, "Unknown FilterOperator.");

    /// <summary>
    /// Returns all operators whose <see cref="FilterOperatorDescriptor.SupportedTypes"/>
    /// include <paramref name="propertyType"/> (after unwrapping <c>Nullable&lt;T&gt;</c>).
    /// Operators with an empty <c>SupportedTypes</c> list (e.g. <c>IsNull</c>) are always included.
    /// </summary>
    public static IReadOnlyList<FilterOperatorDescriptor> For(Type propertyType)
    {
        var underlying = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return _all.Where(d => d.Supports(underlying)).ToList();
    }

    /// <summary>Returns all registered operator descriptors.</summary>
    public static IReadOnlyList<FilterOperatorDescriptor> All => _all;
}

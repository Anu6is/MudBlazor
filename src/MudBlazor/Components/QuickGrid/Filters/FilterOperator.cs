namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Defines the comparison operator applied when evaluating a filter against a column value.
/// Not all operators are valid for all data types; the filter UI surfaces only applicable operators.
/// </summary>
public enum FilterOperator
{
    // ── String operators ───────────────────────────────────────────────────────

    /// <summary>The column value contains the filter value as a substring.</summary>
    StringContains,

    /// <summary>The column value does not contain the filter value as a substring.</summary>
    StringNotContains,

    /// <summary>The column value is equal to the filter value (case-sensitivity per grid setting).</summary>
    StringEqual,

    /// <summary>The column value is not equal to the filter value.</summary>
    StringNotEqual,

    /// <summary>The column value starts with the filter value.</summary>
    StringStartsWith,

    /// <summary>The column value ends with the filter value.</summary>
    StringEndsWith,

    /// <summary>The column value is <see langword="null"/> or an empty string.</summary>
    StringEmpty,

    /// <summary>The column value is not <see langword="null"/> and not an empty string.</summary>
    StringNotEmpty,

    // ── Numeric / comparable operators ────────────────────────────────────────

    /// <summary>The column value is equal to the filter value.</summary>
    NumberEqual,

    /// <summary>The column value is not equal to the filter value.</summary>
    NumberNotEqual,

    /// <summary>The column value is greater than the filter value.</summary>
    NumberGreaterThan,

    /// <summary>The column value is greater than or equal to the filter value.</summary>
    NumberGreaterThanOrEqual,

    /// <summary>The column value is less than the filter value.</summary>
    NumberLessThan,

    /// <summary>The column value is less than or equal to the filter value.</summary>
    NumberLessThanOrEqual,

    // ── DateTime operators ────────────────────────────────────────────────────

    /// <summary>The column value (date portion) is equal to the filter date.</summary>
    DateIs,

    /// <summary>The column value (date portion) is not equal to the filter date.</summary>
    DateIsNot,

    /// <summary>The column value is earlier than the filter date.</summary>
    DateBefore,

    /// <summary>The column value is on or before the filter date.</summary>
    DateOnOrBefore,

    /// <summary>The column value is later than the filter date.</summary>
    DateAfter,

    /// <summary>The column value is on or after the filter date.</summary>
    DateOnOrAfter,

    // ── Boolean operators ─────────────────────────────────────────────────────

    /// <summary>The column value is <see langword="true"/>.</summary>
    BooleanTrue,

    /// <summary>The column value is <see langword="false"/>.</summary>
    BooleanFalse,

    // ── Null / empty (generic) ────────────────────────────────────────────────

    /// <summary>The column value is <see langword="null"/>.</summary>
    IsNull,

    /// <summary>The column value is not <see langword="null"/>.</summary>
    IsNotNull,

    // ── Enum / list operators ─────────────────────────────────────────────────

    /// <summary>The column value is one of the provided filter values.</summary>
    EnumIs,

    /// <summary>The column value is not one of the provided filter values.</summary>
    EnumIsNot,

    // ── Guid operators ────────────────────────────────────────────────────────

    /// <summary>The column value equals the filter <see cref="System.Guid"/>.</summary>
    GuidEqual,

    /// <summary>The column value does not equal the filter <see cref="System.Guid"/>.</summary>
    GuidNotEqual,
}

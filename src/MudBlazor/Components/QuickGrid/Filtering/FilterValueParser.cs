using System.Globalization;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Converts <see cref="GridFilter.Value"/> strings to typed <see cref="object"/> instances
/// for use in filter predicate construction.
/// </summary>
/// <remarks>
/// All parsing uses <see cref="CultureInfo.InvariantCulture"/> to match the culture-invariant
/// serialisation applied when filter values are written to <see cref="GridFilter.Value"/>.
/// </remarks>
internal static class FilterValueParser
{
    /// <summary>
    /// Parses <paramref name="rawValue"/> into an instance of <paramref name="targetType"/>.
    /// Returns <see langword="null"/> when <paramref name="rawValue"/> is <see langword="null"/>
    /// or when the parsed result would be <see langword="null"/>.
    /// </summary>
    /// <param name="rawValue">The string value from <see cref="GridFilter.Value"/>.</param>
    /// <param name="targetType">The property type of the column being filtered.</param>
    /// <returns>The parsed value boxed as <see cref="object"/>, or <see langword="null"/>.</returns>
    public static object? Parse(string? rawValue, Type targetType)
    {
        if (rawValue is null)
            return null;

        // Unwrap Nullable<T> to its underlying type for parsing.
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Enum: parse from name or numeric string.
        if (underlying.IsEnum)
        {
            return Enum.TryParse(underlying, rawValue, ignoreCase: true, out var enumResult)
                ? enumResult
                : null;
        }

        return underlying switch
        {
            _ when underlying == typeof(string) => rawValue,

            _ when underlying == typeof(bool) =>
                bool.TryParse(rawValue, out var b) ? b : null,

            _ when underlying == typeof(int) =>
                int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : null,

            _ when underlying == typeof(long) =>
                long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l) ? l : null,

            _ when underlying == typeof(short) =>
                short.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var s) ? s : null,

            _ when underlying == typeof(byte) =>
                byte.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var by) ? by : null,

            _ when underlying == typeof(double) =>
                double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null,

            _ when underlying == typeof(float) =>
                float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : null,

            _ when underlying == typeof(decimal) =>
                decimal.TryParse(rawValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var dec) ? dec : null,

            _ when underlying == typeof(uint) =>
                uint.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ui) ? ui : null,

            _ when underlying == typeof(ulong) =>
                ulong.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ul) ? ul : null,

            _ when underlying == typeof(DateTime) =>
                DateTime.TryParse(rawValue, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var dt) ? dt : null,

            _ when underlying == typeof(DateTimeOffset) =>
                DateTimeOffset.TryParse(rawValue, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var dto) ? dto : null,

            _ when underlying == typeof(DateOnly) =>
                DateOnly.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly)
                    ? dateOnly
                    : null,

            _ when underlying == typeof(TimeOnly) =>
                TimeOnly.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeOnly)
                    ? timeOnly
                    : null,
            //TODO - add stratergies for TimeOnly and TimeSpan
            _ when underlying == typeof(TimeSpan) =>
                TimeSpan.TryParse(rawValue, CultureInfo.InvariantCulture, out var ts) ? ts : null,

            _ when underlying == typeof(Guid) =>
                Guid.TryParse(rawValue, out var g) ? g : null,

            _ when underlying == typeof(char) =>
                rawValue.Length == 1 ? rawValue[0] : null,

            // Fallback: attempt Convert.ChangeType for any IConvertible type.
            _ => TryConvert(rawValue, underlying),
        };
    }

    private static object? TryConvert(string value, Type targetType)
    {
        try
        {
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }
}

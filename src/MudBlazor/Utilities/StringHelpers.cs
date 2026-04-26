using System.Globalization;

namespace MudBlazor.Utilities;

internal static partial class StringHelpers
{
    /// <summary>
    /// Converts a double value to its string representation, rounded to 4 decimal places.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <param name="format">An optional format string.</param>
    /// <returns>The string representation of the double value.</returns>
    public static string ToS(double value, string? format = null)
    {
        value = Math.Round(value, 4);

        // Normalize negative zero (-0) to standard zero (0) after rounding,
        if (value == 0)
        {
            value = 0;
        }

        return string.IsNullOrEmpty(format)
            ? value.ToString(CultureInfo.InvariantCulture)
            : value.ToString(format);
    }

    /// <summary>
    /// Converts a double value to its string representation, rounded to 4 decimal places.
    /// </summary>
    /// <param name="value">The double value to convert</param>
    /// <returns>
    /// The string representation of the double value. <br/>
    /// </returns>
    public static string ToStr(this double value)
    {
        return ToS(value, null);
    }
}

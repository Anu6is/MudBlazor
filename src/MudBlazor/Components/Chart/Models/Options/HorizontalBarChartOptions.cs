// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable
namespace MudBlazor;

/// <summary>
/// Options specific to horizontal bar charts, extending <see cref="BarChartOptions"/>.
/// </summary>
public class HorizontalBarChartOptions : BarChartOptions
{
    /// <summary>
    /// The value interval between ticks on the X-axis
    /// </summary>
    public int? XAxisTicks { get; set; }

    /// <summary>
    /// The maximum value to display on the X-axis.
    /// </summary>
    public double? XAxisSuggestedMax { get; set; }

    /// <summary>
    /// The maximum number of ticks to display on the X-axis.
    /// </summary>
    public int MaxNumXAxisTicks { get; set; } = 20;

    /// <summary>
    /// A function to convert a value to a string for display on the X-axis.
    /// </summary>
    public Func<double, string>? XAxisToStringFunc { get; set; }

    /// <summary>
    /// The format to use for the X-axis labels.
    /// </summary>
    public string? XAxisFormat { get; set; }
}

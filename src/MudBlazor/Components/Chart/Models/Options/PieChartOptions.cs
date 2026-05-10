// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MudBlazor.Charts;

namespace MudBlazor;

/// <summary>
/// Options specific to pie charts, extending <see cref="DefaultRadialChartOptions"/>.
/// </summary>
public class PieChartOptions : DefaultRadialChartOptions, IHasValueLabelOptions
{
    /// <summary>
    /// Whether values should be displayed within the chart.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    public bool ShowValues { get; set; } = false;

    /// <summary>
    /// The starting angle of the chart in degrees.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>270.0</c> (top).
    /// </remarks>
    public virtual double StartAngle { get; set; } = 270.0;

    /// <summary>
    /// The total angle of the chart arc in degrees.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>360.0</c> (full circle).
    /// </remarks>
    public virtual double SweepAngle { get; set; } = 360.0;

    public static implicit operator PieChartOptions(ChartOptions options) => new()
    {
        ShowLegend = options.ShowLegend,
        ShowToolTips = options.ShowToolTips,
        TooltipTitleFormat = options.TooltipTitleFormat,
        TooltipSubtitleFormat = options.TooltipSubtitleFormat,
        ChartPalette = options.ChartPalette,
    };
}

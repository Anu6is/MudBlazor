// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable
using MudBlazor.Charts;

namespace MudBlazor;

public class HorizontalBarChartOptions : DefaultBarChartOptions
{
    /// <summary>
    /// Defines the spacing between bars in a category group as a ratio of the bar height, with a value between 0.0 and 1.0.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>0.20</c> (20%). This is equivalent to BarSpacingRatio in the base class, but kept for clarity if needed.
    /// Consider if this should override or supplement base.For now, it's a new property.
    /// </remarks>
    public double BarSpacingRatio { get; set; } = 0.20;

    /// <summary>
    /// If set, all bars will have this fixed height in pixels. Overrides BarHeightRatio.
    /// </summary>
    public double? FixedBarHeight { get; set; }

    /// <summary>
    /// Defines the thickness of the bars as a ratio of the available category space, with a value between 0.0 and 1.0.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>0.60</c> (60%).
    /// </remarks>
    public double BarHeightRatio { get; set; } = 0.60;

    /// <summary>
    /// If true, lines will be drawn to separate categories on the Y-axis.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    public bool ShowCategoryLines { get; set; } = true;

    /// <summary>
    /// Spacing for Y-axis labels from the axis line.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>10</c>.
    /// </remarks>
    public double YAxisLabelSpacing { get; set; } = 10;

    /// <summary>
    /// Spacing for X-axis labels from the axis line.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>15</c>. (as used in GenerateValueGridLines before)
    /// </remarks>
    public double XAxisLabelSpacing { get; set; } = 15;

    /// <summary>
    /// Defines the gap between category groups in pixels.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>10</c>. Used by Justify logic.
    /// </remarks>
    public double BarGroupGap { get; set; } = 10;
    
    /// <summary>
    /// If FixedBarHeight is set, this defines the gap in pixels between bars within a category group.
    /// If null, the gap will be calculated based on BarSpacingRatio.
    /// </summary>
    public double? FixedBarGap { get; set; }

    // X-axis specific properties for the value axis of the HorizontalBar chart
    // These mirror the Y-axis properties from DefaultAxisChartOptions but are for the X-axis.

    private int _xAxisTicks = 20;
    /// <summary>
    /// The spacing between horizontal value tick marks.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>20</c>. Value must be greater than or equal to <c>1</c>.
    /// </remarks>
    public int XAxisTicks
    {
        get => _xAxisTicks;
        set => _xAxisTicks = Math.Max(1, value);
    }

    private int _maxNumXAxisTicks = 20;
    /// <summary>
    /// The maximum allowed number of horizontal value tick marks.
    /// </summary>
    /// <remarks>
    /// If the number of ticks calculated exceeds this value, the tick marks will automatically be thinned out.
    /// Value must be greater than or equal to <c>1</c>.
    /// </remarks>
    public int MaxNumXAxisTicks
    {
        get => _maxNumXAxisTicks;
        set => _maxNumXAxisTicks = Math.Max(1, value);
    }

    /// <summary>
    /// The maximum value for the horizontal value axis.
    /// </summary>
    /// <remarks>
    /// This value is used only if all data points are less than or equal to it. 
    /// If any data point exceeds this value, the X-axis maximum will automatically adjust to fit the data.
    /// If this value is <c>null</c>, the X-axis maximum will be calculated automatically.
    /// </remarks>
    public double? XAxisSuggestedMax { get; set; }

    /// <summary>
    /// The format applied to numbers on the horizontal value axis.
    /// </summary>
    /// <remarks>
    /// Values in this property are standard .NET format strings, such as those passed into the <c>ToString()</c> method.
    /// </remarks>
    public string? XAxisFormat { get; set; }


    public static implicit operator HorizontalBarChartOptions(ChartOptions options) => new()
    {
        ShowLegend = options.ShowLegend,
        ShowToolTips = options.ShowToolTips,
        TooltipTitleFormat = options.TooltipTitleFormat,
        TooltipSubtitleFormat = options.TooltipSubtitleFormat,
        ChartPalette = options.ChartPalette,
        // Note: Other properties from DefaultBarChartOptions like Justify, SeriesSpacingRatio etc. are inherited.
        // We are adding new ones specific to HorizontalBar or renaming concepts.
    };
}

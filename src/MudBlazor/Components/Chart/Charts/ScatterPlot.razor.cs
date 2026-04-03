// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;
using Microsoft.AspNetCore.Components;
using MudBlazor.Interpolation;

namespace MudBlazor.Charts;

/// <summary>
/// Represents a chart which displays series values as individual data points plotted by X and Y coordinates.
/// </summary>
/// <remarks>
/// Each data point must have an <c>X</c> value set on the <see cref="ChartPoint{T}"/>.
/// Use the <c>(T x, T y)</c> constructor or the implicit conversion from a <c>(T, T)</c> tuple.
/// </remarks>
/// <seealso cref="Line{T}"/>
/// <seealso cref="Bar{T}"/>
/// <seealso cref="TimeSeries{T}"/>
partial class ScatterPlot<T> : MudAxisLineChartBase<T, ScatterPlotChartOptions> where T : struct, INumber<T>, IMinMaxValue<T>, IFormattable
{
    public override RenderFragment? OverlayContent { get; set; }

    protected override bool ShouldInterpolate => false;

    // X-axis scale information, computed during RebuildChart
    private T _gridXUnits;
    private int _lowestVerticalLine;
    private double _horizontalSpacePerXUnit;

    protected override void OnInitialized()
    {
        ChartType = ChartType.ScatterPlot;
        ChartOptions ??= new ScatterPlotChartOptions();

        base.OnInitialized();
    }

    public override void RebuildChart()
    {
        Series = (ChartContainer != null && ChartReference is MudChart<T>)
            ? ChartContainer.ChartSeries
            : ChartSeries;

        SetBounds();
        ComputeXAxisScale(out _gridXUnits, out _lowestVerticalLine, out var numVerticalLines);
        ComputeYAxisScale(out var gridYUnits, out var lowestHorizontalLine, out var numHorizontalLines);

        var verticalSpace = (_boundHeight - VerticalStartSpace - VerticalEndSpace) / Math.Max(1, numHorizontalLines - 1);
        _horizontalSpacePerXUnit = (_boundWidth - HorizontalStartSpace - HorizontalEndSpace) / Math.Max(1, numVerticalLines - 1);

        GenerateHorizontalGridLines(numHorizontalLines, lowestHorizontalLine, gridYUnits, verticalSpace);
        GenerateVerticalGridLines(numVerticalLines, 0, _horizontalSpacePerXUnit);
        GenerateChartLines(lowestHorizontalLine, gridYUnits, _horizontalSpacePerXUnit, verticalSpace);
        GenerateLegends();
    }

    private void ComputeYAxisScale(out T gridYUnits, out int lowestHorizontalLine, out int numHorizontalLines)
    {
        gridYUnits = ChartOptions?.YAxisTicks > 0
            ? T.CreateSaturating(ChartOptions.YAxisTicks)
            : T.CreateSaturating(20);

        var visiblePoints = Series
            .Where(s => s.Visible)
            .SelectMany(s => s.Data.Points)
            .ToArray();

        if (visiblePoints.Length == 0)
        {
            lowestHorizontalLine = 0;
            numHorizontalLines = 1;
            return;
        }

        var minY = visiblePoints.Min(p => p.Y);
        var maxY = visiblePoints.Max(p => p.Y);

        if (ChartOptions?.YAxisSuggestedMax is { } suggestedMax)
        {
            maxY = T.Max(T.CreateSaturating(suggestedMax), maxY);
        }

        if (ChartOptions?.YAxisRequireZeroPoint is true)
        {
            minY = T.Min(minY, T.Zero);
            maxY = T.Max(maxY, T.Zero);
        }

        lowestHorizontalLine = (int)Math.Floor(double.CreateSaturating(minY) / double.CreateSaturating(gridYUnits));
        var highestHorizontalLine = (int)Math.Ceiling(double.CreateSaturating(maxY) / double.CreateSaturating(gridYUnits));
        numHorizontalLines = highestHorizontalLine - lowestHorizontalLine + 1;

        var maxYTicks = ChartOptions?.MaxNumYAxisTicks ?? 20;
        while (numHorizontalLines > maxYTicks)
        {
            gridYUnits *= T.CreateSaturating(2);
            lowestHorizontalLine = (int)Math.Floor(double.CreateSaturating(minY) / double.CreateSaturating(gridYUnits));
            highestHorizontalLine = (int)Math.Ceiling(double.CreateSaturating(maxY) / double.CreateSaturating(gridYUnits));
            numHorizontalLines = highestHorizontalLine - lowestHorizontalLine + 1;
        }
    }

    private void ComputeXAxisScale(out T gridXUnits, out int lowestVerticalLine, out int numVerticalLines)
    {
        gridXUnits = ChartOptions?.XAxisTicks > 0
            ? T.CreateSaturating(ChartOptions.XAxisTicks)
            : T.CreateSaturating(20);

        var points = Series
            .Where(s => s.Visible)
            .SelectMany(s => s.Data.Points)
            .ToArray();

        if (points.Length == 0)
        {
            lowestVerticalLine = 0;
            numVerticalLines = 1;
            return;
        }

        var xValues = points
            .Select(p => p.X)
            .OfType<T>()
            .ToArray();

        T minX, maxX;
        if (xValues.Length < points.Length)
        {
            // If any points have non-numeric X, use indices for all points to ensure a consistent scale
            minX = T.Zero;
            maxX = T.CreateSaturating(Series.Max(s => s.Data.Points.Count) - 1);
        }
        else
        {
            minX = xValues.Min();
            maxX = xValues.Max();
        }

        lowestVerticalLine = (int)Math.Floor(double.CreateSaturating(minX) / double.CreateSaturating(gridXUnits));
        var highestVerticalLine = (int)Math.Ceiling(double.CreateSaturating(maxX) / double.CreateSaturating(gridXUnits));
        numVerticalLines = highestVerticalLine - lowestVerticalLine + 1;

        var maxXTicks = ChartOptions?.MaxNumXAxisTicks ?? 20;
        while (numVerticalLines > maxXTicks)
        {
            gridXUnits *= T.CreateSaturating(2);
            lowestVerticalLine = (int)Math.Floor(double.CreateSaturating(minX) / double.CreateSaturating(gridXUnits));
            highestVerticalLine = (int)Math.Ceiling(double.CreateSaturating(maxX) / double.CreateSaturating(gridXUnits));
            numVerticalLines = highestVerticalLine - lowestVerticalLine + 1;
        }
    }

    protected override string GetVerticalGridLineLabel(int index)
    {
        var value = T.CreateSaturating(_lowestVerticalLine + index) * _gridXUnits;
        return ChartOptions?.XAxisFormat is { } fmt
            ? value.ToString(fmt, null)
            : value.ToString(null, null);
    }

    protected override TReturn GetDataValue<TReturn>(int seriesIndex, int dataPointIndex)
    {
        return (TReturn)Convert.ChangeType(Series[seriesIndex].Data.Points[dataPointIndex].Y, typeof(TReturn));
    }

    protected override SeriesDisplayOverride? GetSeriesDisplayOverride(ChartSeries<T> series)
    {
        return ChartOptions?.SeriesDisplayOverrides?.TryGetValue(series, out var overrideData) is true
            ? overrideData
            : null;
    }

    protected override string GetLabelXValue(int seriesIndex, int dataPointIndex)
    {
        var x = Series[seriesIndex].Data.Points[dataPointIndex].X;
        if (x is T xVal)
        {
            return ChartOptions?.XAxisFormat is { } fmt
                ? xVal.ToString(fmt, null)
                : xVal.ToString(null, null);
        }

        return dataPointIndex.ToString();
    }

    protected override (double x, double y) GetXYForDataPoint(int seriesIndex, int dataPointIndex, int lowestHorizontalLine, T gridYUnits, double horizontalSpace, double verticalSpace)
    {
        var point = Series[seriesIndex].Data.Points[dataPointIndex];

        // Map Y to screen coordinate
        var gridValueY = ((double.CreateSaturating(point.Y) / double.CreateSaturating(gridYUnits)) - lowestHorizontalLine) * verticalSpace;
        var screenY = _boundHeight - VerticalStartSpace - gridValueY;

        // Map X to screen coordinate using the X-axis scale
        double screenX;
        if (point.X is T xVal)
        {
            var gridValueX = ((double.CreateSaturating(xVal) / double.CreateSaturating(_gridXUnits)) - _lowestVerticalLine) * _horizontalSpacePerXUnit;
            screenX = HorizontalStartSpace + gridValueX;
        }
        else
        {
            // Fallback: use index mapped through the X-axis scale
            var gridValueX = ((double.CreateSaturating(dataPointIndex) / double.CreateSaturating(_gridXUnits)) - _lowestVerticalLine) * _horizontalSpacePerXUnit;
            screenX = HorizontalStartSpace + gridValueX;
        }

        return (screenX, screenY);
    }

    protected override (double firstX, double firstY, double lastX) GenerateStraightLines(int seriesIndex,
        System.Text.StringBuilder chartLine,
        List<SvgCircle> chartDataCircles,
        int lowestHorizontalLine,
        T gridYUnits,
        double horizontalSpace,
        double verticalSpace)
    {
        double firstPointX = 0, firstPointY = 0, lastPointX = 0;

        var series = Series[seriesIndex];
        var dataLength = series.Data.Points.Count;

        for (var j = 0; j < dataLength; j++)
        {
            var (x, y) = GetXYForDataPoint(seriesIndex, j, lowestHorizontalLine, gridYUnits, horizontalSpace, verticalSpace);

            if (j == 0)
            {
                chartLine.Append("M ");
                firstPointX = x;
                firstPointY = y;
            }
            else
            {
                chartLine.Append(" L ");
            }

            if (j == dataLength - 1)
            {
                lastPointX = x;
            }

            chartLine.Append(Utilities.StringHelpers.ToS(x));
            chartLine.Append(' ');
            chartLine.Append(Utilities.StringHelpers.ToS(y));

            chartDataCircles.Add(new SvgCircle
            {
                Index = seriesIndex,
                CX = x,
                CY = y,
                LabelX = x,
                LabelXValue = GetLabelXValue(seriesIndex, j),
                LabelY = y,
                LabelYValue = GetDataValueAsString(seriesIndex, j)
            });
        }

        return (firstPointX, firstPointY, lastPointX);
    }

    internal override ILineInterpolator CreateInterpolator(int seriesIndex, int lowestHorizontalLine, T gridYUnits, double horizontalSpace, double verticalSpace)
    {
        throw new NotSupportedException("Interpolation is not supported for scatter plot charts.");
    }
}

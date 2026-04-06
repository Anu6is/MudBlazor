// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AwesomeAssertions;
using Bunit;
using MudBlazor.Charts;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Charts;

[TestFixture]
public class RadarChartScalingTests : BunitTest
{
    [Test]
    public void RadarChart_Scaling_SmallValues_GridLevels1()
    {
        var seriesData = new double[] { 5, 5, 5 };
        var options = new RadarChartOptions { GridLevels = 1, ShowAxisValues = true };

        var comp = Context.Render<Radar<double>>(parameters => parameters
            .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Name = "Series", Data = seriesData } })
            .Add(p => p.ChartOptions, options)
        );

        var axisValues = comp.FindAll("text.mud-chart-axis-value");
        axisValues.Count.Should().Be(1);
        axisValues[0].TextContent.Should().Be("5");
    }

    [Test]
    public void RadarChart_Scaling_SmallValues_GridLevels6()
    {
        var seriesData = new double[] { 5, 5, 5 };
        var options = new RadarChartOptions { GridLevels = 6, ShowAxisValues = true };

        var comp = Context.Render<Radar<double>>(parameters => parameters
            .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Name = "Series", Data = seriesData } })
            .Add(p => p.ChartOptions, options)
        );

        var axisValues = comp.FindAll("text.mud-chart-axis-value");
        axisValues.Count.Should().Be(6);
        // max value is 5. 5/6 = 0.833. FindNextNiceStep(0.833) -> exponent -1, fraction 8.33 -> niceFraction 10 -> step 1.
        // 1 * 6 = 6. axisMaxValue = 6.
        // steps: 1, 2, 3, 4, 5, 6
        axisValues[5].TextContent.Should().Be("6");
    }

    [Test]
    public void RadarChart_Scaling_SmallValues_GridLevels2()
    {
        var seriesData = new double[] { 5, 5, 5 };
        var options = new RadarChartOptions { GridLevels = 2, ShowAxisValues = true };

        var comp = Context.Render<Radar<double>>(parameters => parameters
            .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Name = "Series", Data = seriesData } })
            .Add(p => p.ChartOptions, options)
        );

        var axisValues = comp.FindAll("text.mud-chart-axis-value");
        axisValues.Count.Should().Be(2);
        // max 5. 5/2 = 2.5. FindNextNiceStep(2.5) -> 2.5.
        // 2.5 * 2 = 5.
        axisValues[0].TextContent.Should().Be("2.5");
        axisValues[1].TextContent.Should().Be("5");
    }

    [Test]
    public void RadarChart_Option_YAxisSuggestedMax()
    {
        var seriesData = new double[] { 5, 5, 5 };
        var options = new RadarChartOptions { GridLevels = 1, ShowAxisValues = true, YAxisSuggestedMax = 10 };

        var comp = Context.Render<Radar<double>>(parameters => parameters
            .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Name = "Series", Data = seriesData } })
            .Add(p => p.ChartOptions, options)
        );

        var axisValues = comp.FindAll("text.mud-chart-axis-value");
        axisValues.Last().TextContent.Should().Be("10");
    }

    [Test]
    public void RadarChart_Option_YAxisFormat()
    {
        var seriesData = new double[] { 5, 5, 5 };
        var options = new RadarChartOptions { GridLevels = 1, ShowAxisValues = true, YAxisFormat = "N2" };

        var comp = Context.Render<Radar<double>>(parameters => parameters
            .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Name = "Series", Data = seriesData } })
            .Add(p => p.ChartOptions, options)
        );

        var axisValues = comp.FindAll("text.mud-chart-axis-value");
        axisValues[0].TextContent.Should().Be("5.00");
    }

    [Test]
    public void RadarChart_Option_YAxisToStringFunc()
    {
        var seriesData = new double[] { 5, 5, 5 };
        var options = new RadarChartOptions
        {
            GridLevels = 1,
            ShowAxisValues = true,
            YAxisToStringFunc = (val) => $"Value: {val}"
        };

        var comp = Context.Render<Radar<double>>(parameters => parameters
            .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Name = "Series", Data = seriesData } })
            .Add(p => p.ChartOptions, options)
        );

        var axisValues = comp.FindAll("text.mud-chart-axis-value");
        axisValues[0].TextContent.Should().Be("Value: 5");
    }

    [Test]
    public void RadarChart_Scaling_GridLevelsZero()
    {
        var seriesData = new double[] { 5, 5, 5 };
        var options = new RadarChartOptions { GridLevels = 0, ShowAxisValues = true };

        var comp = Context.Render<Radar<double>>(parameters => parameters
            .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Name = "Series", Data = seriesData } })
            .Add(p => p.ChartOptions, options)
        );

        // When GridLevels is 0, GenerateAxisValues returns early.
        var axisValues = comp.FindAll("text.mud-chart-axis-value");
        axisValues.Count.Should().Be(0);
    }

    [Test]
    public void RadarChart_Scaling_AllZeroValues()
    {
        var seriesData = new double[] { 0, 0, 0 };
        var options = new RadarChartOptions { GridLevels = 5, ShowAxisValues = true };

        var comp = Context.Render<Radar<double>>(parameters => parameters
            .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Name = "Series", Data = seriesData } })
            .Add(p => p.ChartOptions, options)
        );

        var axisValues = comp.FindAll("text.mud-chart-axis-value");
        // CalculateAxisMaxValue returns T.Zero for all zeros.
        // GenerateAxisValues then uses stepValue = 0.
        // All labels should be "0".
        foreach (var label in axisValues)
        {
            label.TextContent.Should().Be("0");
        }
    }

    [Test]
    public void RadarChart_Scaling_VeryLargeValues()
    {
        var seriesData = new double[] { 1e15, 1e15, 1e15 };
        var options = new RadarChartOptions { GridLevels = 2, ShowAxisValues = true };

        var comp = Context.Render<Radar<double>>(parameters => parameters
            .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Name = "Series", Data = seriesData } })
            .Add(p => p.ChartOptions, options)
        );

        var axisValues = comp.FindAll("text.mud-chart-axis-value");
        axisValues.Count.Should().Be(2);
        // 1e15 / 2 = 0.5e15 -> 5e14. FindNextNiceStep(5e14) -> 5e14.
        // 5e14 * 2 = 1e15.
        axisValues[0].TextContent.Should().Be((5e14).ToString(CultureInfo.InvariantCulture));
        axisValues[1].TextContent.Should().Be((1e15).ToString(CultureInfo.InvariantCulture));
    }

    [Test]
    public void RadarChart_Scaling_NegativeValues()
    {
        var seriesData = new double[] { -5, -10, -5 };
        var options = new RadarChartOptions { GridLevels = 2, ShowAxisValues = true };

        var comp = Context.Render<Radar<double>>(parameters => parameters
            .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Name = "Series", Data = seriesData } })
            .Add(p => p.ChartOptions, options)
        );

        // Max of (-5, -10, -5) is -5.
        // CalculateAxisMaxValue treats <= 0 as T.Zero.
        var axisValues = comp.FindAll("text.mud-chart-axis-value");
        foreach (var label in axisValues)
        {
            label.TextContent.Should().Be("0");
        }
    }
}

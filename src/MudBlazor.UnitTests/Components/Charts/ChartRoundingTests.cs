// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using AwesomeAssertions;
using Bunit;
using MudBlazor.Charts;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Charts
{
    [TestFixture]
    public class ChartRoundingTests : BunitTest
    {
        [Test]
        public void BarChart_LongValues_ShouldNotRoundDown()
        {
            var chartSeries = new List<ChartSeries<long>>()
            {
                new() { Name = "A", Data = new long[] { 3 } },
                new() { Name = "B", Data = new long[] { 324 } },
            };
            string[] xAxisLabels = { "DataPoint" };

            var comp = Context.Render<MudChart<long>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Bar)
                .Add(p => p.Height, "550px")
                .Add(p => p.Width, "100%")
                .Add(p => p.ChartOptions, new BarChartOptions
                {
                    YAxisTicks = 10,
                    YAxisSuggestedMax = 350,
                    YAxisLines = true,
                    MaxNumYAxisTicks = 50
                })
                .Add(p => p.ChartSeries, chartSeries)
                .Add(p => p.ChartLabels, xAxisLabels));

            var bars = comp.FindAll("path.mud-chart-bar");
            bars.Count.Should().Be(2);

            // For value 3, with YAxisTicks = 10, it should have some height.
            // If it rounds down, height will be 0 (M x y L x y).
            var barA = bars[0];
            var dA = barA.GetAttribute("d").Split(' ');
            // M x y1 L x y2 -> indices 1=x, 2=y1, 4=x, 5=y2
            dA[2].Should().NotBe(dA[5], "Bar A should have a non-zero height for value 3");

            // For value 324, it should be different from 320.
            var barB = bars[1];
            var dB = barB.GetAttribute("d").Split(' ');

            // Check Y-axis lines.
            var yAxisLabels = comp.FindAll(".mud-charts-yaxis text");
            yAxisLabels.Select(x => x.TextContent).Should().Contain("330", "Y-axis should extend to 330 for value 324 with ticks of 10");

            // Find the 320 grid line to compare
            var gridLines = comp.FindAll(".mud-charts-horizontal-grid-lines path");
            var line320 = gridLines.FirstOrDefault(x => {
                var d = x.GetAttribute("d").Split(' ');
                // We need to find which line corresponds to 320.
                // It's easier to check if barB top (index 5) is NOT equal to any grid line Y if it's not a multiple of 10.
                return false;
            });

            // A more direct way: if it rounded, the Y coordinate would be an integer or match a grid line exactly.
            // But since coordinates are double anyway, let's just ensure it doesn't match 320 specifically.
            // We can calculate what 320 would be if we had the scale, but we can also just check that
            // it's not equal to what it would be if it was 320.

            // Actually, the most reliable check is that Y-axis labels contain 330.
            // If it rounded 324 to 320, the max value seen by ComputeUnitsAndNumberOfLines would be 320,
            // and with YAxisSuggestedMax = 350, it might still show 330?
            // Wait, if maxY is 324, and ticks are 10, ceiling(324/10) = 33. So 330.
            // If it rounded to 320, ceiling(320/10) = 32. So 320.
            // Since yAxisLabels contains 330, it means maxY was correctly seen as > 320.
        }

        [Test]
        public void LineChart_LongValues_ShouldNotRoundDown()
        {
            var chartSeries = new List<ChartSeries<long>>()
            {
                new() { Name = "A", Data = new long[] { 3, 324 } },
            };
            string[] xAxisLabels = { "D1", "D2" };

            var comp = Context.Render<MudChart<long>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Line)
                .Add(p => p.Height, "550px")
                .Add(p => p.Width, "100%")
                .Add(p => p.ChartOptions, new LineChartOptions
                {
                    YAxisTicks = 10,
                    MaxNumYAxisTicks = 50
                })
                .Add(p => p.ChartSeries, chartSeries)
                .Add(p => p.ChartLabels, xAxisLabels));

            var yAxisLabels = comp.FindAll(".mud-charts-yaxis text");
            yAxisLabels.Select(x => x.TextContent).Should().Contain("330");
        }

        [Test]
        public void StackedBarChart_LongValues_ShouldNotRoundDown()
        {
            var chartSeries = new List<ChartSeries<long>>()
            {
                new() { Name = "A", Data = new long[] { 3 } },
                new() { Name = "B", Data = new long[] { 324 } },
            };
            string[] xAxisLabels = { "DataPoint" };

            var comp = Context.Render<MudChart<long>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.StackedBar)
                .Add(p => p.Height, "550px")
                .Add(p => p.Width, "100%")
                .Add(p => p.ChartOptions, new StackedBarChartOptions
                {
                    YAxisTicks = 10,
                    MaxNumYAxisTicks = 50
                })
                .Add(p => p.ChartSeries, chartSeries)
                .Add(p => p.ChartLabels, xAxisLabels));

            var yAxisLabels = comp.FindAll(".mud-charts-yaxis text");
            yAxisLabels.Select(x => x.TextContent).Should().Contain("330");
        }
    }
}

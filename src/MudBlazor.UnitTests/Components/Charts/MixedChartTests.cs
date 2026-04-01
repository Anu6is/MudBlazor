// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using AwesomeAssertions;
using Bunit;
using MudBlazor.Charts;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Charts
{
    public class MixedChartTests : BunitTest
    {
        [Test]
        public void MixedChart_AlignmentTest()
        {
            var barSeries = new List<ChartSeries<double>>()
            {
                new() { Name = "Bar", Data = new double[] { 100 } }
            };
            var lineSeries = new List<ChartSeries<double>>()
            {
                new() { Name = "Line", Data = new double[] { 100 } }
            };

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Bar)
                .Add(p => p.Height, "350px")
                .Add(p => p.Width, "100%")
                .Add(p => p.ChartSeries, barSeries)
                .Add(p => p.ChartOptions, new BarChartOptions { YAxisTicks = 100, YAxisSuggestedMax = 100 })
                .AddChildContent<Line<double>>(lineParams => lineParams
                    .Add(p => p.ChartSeries, lineSeries)
                )
            );

            comp.Render();

            // Expected Y for 100: 350 - 30 - 295 = 25

            var linePoints = comp.FindAll("circle.mud-chart-point");
            linePoints.Should().HaveCount(1);
            var circle = linePoints[0];
            var cy = double.Parse(circle.GetAttribute("cy")!);

            cy.Should().BeInRange(24.9, 25.1, "Line point for value 100 should be at y=25");

            var bars = comp.FindAll("path.mud-chart-bar");
            bars.Should().HaveCount(1);
            bars[0].GetAttribute("d").Should().EndWith(" 25");
        }

        [Test]
        public void MixedChart_BarOverlayAlignmentTest()
        {
            var lineSeries = new List<ChartSeries<double>>()
            {
                new() { Name = "Line", Data = new double[] { 100 } }
            };
            var barSeries = new List<ChartSeries<double>>()
            {
                new() { Name = "Bar", Data = new double[] { 50 } }
            };

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Line)
                .Add(p => p.Height, "350px")
                .Add(p => p.Width, "100%")
                .Add(p => p.ChartSeries, lineSeries)
                .Add(p => p.ChartOptions, new LineChartOptions { YAxisTicks = 100, YAxisSuggestedMax = 100, YAxisRequireZeroPoint = true })
                .AddChildContent<Bar<double>>(barParams => barParams
                    .Add(p => p.ChartSeries, barSeries)
                )
            );

            comp.Render();

            // Total height for grid = 350 - 30 - 25 = 295
            // base (0) = 320
            // value 50: (50/100) * 295 = 147.5
            // y = 320 - 147.5 = 172.5

            var bars = comp.FindAll("path.mud-chart-bar");
            bars.Should().HaveCount(1);

            var d = bars[0].GetAttribute("d")!;
            d.Should().Contain(" 320 "); // Base should be at 320
            d.Should().EndWith(" 172.5"); // Top should be at 172.5
        }

        [Test]
        public void MixedChart_DocumentationExampleAlignmentTest()
        {
            // Reproduce the case: 880 point vs 800 line
            // YAxisTicks = 400, YAxisSuggestedMax = 1600
            // numHorizontalLines = 5 (0, 400, 800, 1200, 1600)

            var barSeries = new List<ChartSeries<double>>()
            {
                new() { Name = "Bar", Data = new double[] { 1200 } }
            };
            var lineSeries = new List<ChartSeries<double>>()
            {
                new() { Name = "Line", Data = new double[] { 880 } }
            };

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.StackedBar)
                .Add(p => p.Height, "350px")
                .Add(p => p.Width, "100%")
                .Add(p => p.ChartSeries, barSeries)
                .Add(p => p.ChartOptions, new StackedBarChartOptions { YAxisTicks = 400, YAxisSuggestedMax = 1600 })
                .AddChildContent<Line<double>>(lineParams => lineParams
                    .Add(p => p.ChartSeries, lineSeries)
                    .Add(p => p.ChartOptions, new LineChartOptions { ShowDataMarkers = true })
                )
            );

            comp.Render();

            // verticalSpace = 295 / 4 = 73.75
            // Line 800 is at index 2 (0, 1, 2)
            // Line 800 Y = 320 - 2 * 73.75 = 172.5
            // Value 880 Y = 320 - (880/400) * 73.75 = 320 - 2.2 * 73.75 = 320 - 162.25 = 157.75

            // Grid lines
            var gridLines = comp.FindAll("g.mud-charts-gridlines-yaxis path");
            gridLines.Should().HaveCount(5);
            // Grid line for 800 is the 3rd one (index 2)
            var line800D = gridLines[2].GetAttribute("d")!;
            line800D.Should().Contain(" 172.5 ");

            // Line point for 880
            var linePoint = comp.Find("circle.mud-chart-point");
            var cy = double.Parse(linePoint.GetAttribute("cy")!);
            cy.Should().BeInRange(157.7, 157.8);

            cy.Should().BeLessThan(172.5, "Point 880 should be ABOVE (smaller Y) line 800");
        }
    }
}

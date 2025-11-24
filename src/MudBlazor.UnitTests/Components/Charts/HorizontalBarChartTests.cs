// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using Bunit;
using FluentAssertions;
using MudBlazor.Charts;
using MudBlazor.UnitTests.Components;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Charts
{
    public class HorizontalBarChartTests : BunitTest
    {
        [Test]
        public void HorizontalBarChartEmptyData()
        {
            var comp = Context.RenderComponent<HorizontalBar<double>>();
            comp.Markup.Should().Contain("mud-chart");
        }

        [Test]
        public void HorizontalBarChartExampleData()
        {
            var chartSeries = new List<ChartSeries<double>>()
            {
                new () { Name = "United States", Data = new double[] { 40, 20, 25, 27, 46, 60, 48, 80, 15 } },
                new () { Name = "Germany", Data = new double[] { 19, 24, 35, 13, 28, 15, -4, 16, 31 } },
                new () { Name = "Sweden", Data = new double[] { 8, 6, -11, 13, 4, 16, 10, 16, 18 } },
            };
            string[] xAxisLabels = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep" };

            var comp = Context.RenderComponent<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.Height, "350px")
                .Add(p => p.Width, "100%")
                .Add(p => p.ChartOptions, new HorizontalBarChartOptions { FixedBarWidth = 8 })
                .Add(p => p.ChartSeries, chartSeries)
                .Add(p => p.ChartLabels, xAxisLabels));

            var bars = comp.FindAll("path.mud-chart-bar");
            bars.Count.Should().Be(3 * 9, because: "3 series with 9 data points each");

            var yAxisLabels = comp.FindAll(".mud-charts-yaxis text");
            yAxisLabels.Count.Should().Be(9);
            yAxisLabels[0].TextContent.Should().Be("Jan");

            var xAxisLabelsRendered = comp.FindAll(".mud-charts-xaxis text");
            xAxisLabelsRendered.Count.Should().BeGreaterThan(0);
        }
    }
}

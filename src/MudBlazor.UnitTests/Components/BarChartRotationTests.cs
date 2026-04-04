using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Charts;
using MudBlazor.UnitTests.TestComponents;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class BarChartRotationTests : BunitTest
    {
        [Test]
        public void BarChart_RotatedLabels_ShouldHaveCorrectOffset()
        {
            var series = new List<ChartSeries<double>>()
            {
                new() { Name = "Series 1", Data = new double[] { 1, 2, 3 } },
            };
            var labels = new[] { "Label 1", "Label 2", "Label 3" };
            var options = new BarChartOptions
            {
                XAxisLabelRotation = 90
            };

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Bar)
                .Add(p => p.ChartSeries, series)
                .Add(p => p.ChartLabels, labels)
                .Add(p => p.ChartOptions, options)
                .Add(p => p.Height, "400px")
                .Add(p => p.Width, "600px")
            );

            // Initially _xAxisLabelSize is null, so it uses default 20.
            // XAxisLabelOffset = 20 (since rotation != 0)
            // VerticalStartSpace = Max(10 + 0, 30) = 30.
            // Height = "400px" -> _boundHeight = 350. (Default since Width/Height are "80%" by default in MudChartBase, but we passed "400px")
            // Wait, MudChart height is "80%" by default. In MudChart.razor.cs it doesn't seem to set _boundHeight from Height parameter unless MatchBoundsToSize is true.
            // MudAxisChartBase.SetBounds:
            // if (MatchBoundsToSize) { ... } else { _boundWidth = 700; _boundHeight = 350; }
            // So _boundHeight = 350.
            // Y = 350 - (20 + 10) = 320.

            var texts = comp.FindAll("g.mud-charts-xaxis text");
            foreach (var text in texts)
            {
                var y = double.Parse(text.GetAttribute("y"));
                var textAnchor = text.GetAttribute("text-anchor");
                y.Should().Be(320);
                textAnchor.Should().Be("end");
            }
        }

        [Test]
        public void BarChart_NoRotationLabels_ShouldHaveCorrectOffset()
        {
            var series = new List<ChartSeries<double>>()
            {
                new() { Name = "Series 1", Data = new double[] { 1, 2, 3 } },
            };
            var labels = new[] { "Label 1", "Label 2", "Label 3" };
            var options = new BarChartOptions
            {
                XAxisLabelRotation = 0
            };

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Bar)
                .Add(p => p.ChartSeries, series)
                .Add(p => p.ChartLabels, labels)
                .Add(p => p.ChartOptions, options)
                .Add(p => p.Height, "400px")
                .Add(p => p.Width, "600px")
            );

            // Initially _xAxisLabelSize is null, so it uses default 20.
            // XAxisLabelOffset = 20 / 2 = 10 (since rotation == 0)
            // VerticalStartSpace = Max(10 + 0, 30) = 30.
            // _boundHeight = 350.
            // Y = 350 - 10 = 340.

            var texts = comp.FindAll("g.mud-charts-xaxis text");
            foreach (var text in texts)
            {
                var y = double.Parse(text.GetAttribute("y"));
                var textAnchor = text.GetAttribute("text-anchor");
                y.Should().Be(340);
                textAnchor.Should().Be("middle");
            }
        }
    }
}

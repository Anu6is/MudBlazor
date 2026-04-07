using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Charts;
using MudBlazor.Interop;
using System.Globalization;
using MudBlazor.UnitTests.TestComponents;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class BarChartRotationTests : BunitTest
    {
        [Test]
        public async Task BarChart_RotatedLabels_ShouldHaveCorrectOffset()
        {
            Context.JSInterop.Setup<ElementSize>("mudGetSvgBBox", _ => true).SetResult(new ElementSize { Width = 50, Height = 20 });
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

            // XAxisLabelOffset = Height (20) + 10 = 30
            // _boundHeight = 350.
            // Y = 350 - 30 = 320.

            await comp.WaitForAssertionAsync(() =>
            {
                var texts = comp.FindAll("g.mud-charts-xaxis text");
                texts.Should().NotBeEmpty();
                foreach (var text in texts)
                {
                    var y = double.Parse(text.GetAttribute("y"), CultureInfo.InvariantCulture);
                    var textAnchor = text.GetAttribute("text-anchor");
                    y.Should().Be(320);
                    textAnchor.Should().Be("end");
                }
            });
        }

        [Test]
        public async Task BarChart_NoRotationLabels_ShouldHaveCorrectOffset()
        {
            Context.JSInterop.Setup<ElementSize>("mudGetSvgBBox", _ => true).SetResult(new ElementSize { Width = 50, Height = 20 });
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

            // XAxisLabelOffset = Height (20) / 2 = 10
            // _boundHeight = 350.
            // Y = 350 - 10 = 340.

            await comp.WaitForAssertionAsync(() =>
            {
                var texts = comp.FindAll("g.mud-charts-xaxis text");
                texts.Should().NotBeEmpty();
                foreach (var text in texts)
                {
                    var y = double.Parse(text.GetAttribute("y"), CultureInfo.InvariantCulture);
                    var textAnchor = text.GetAttribute("text-anchor");
                    y.Should().Be(340);
                    textAnchor.Should().Be("middle");
                }
            });
        }
    }
}

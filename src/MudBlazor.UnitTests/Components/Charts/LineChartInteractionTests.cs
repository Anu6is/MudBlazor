// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AwesomeAssertions;
using Bunit;
using MudBlazor.Charts;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Charts
{
    public class LineChartInteractionTests : BunitTest
    {
        [Test]
        public void LineChart_SharedTooltip_ShouldRender()
        {
            var chartSeries = new List<ChartSeries<double>>()
            {
                new() { Name = "Series 1", Data = new double[] { 10, 20, 30 } },
                new() { Name = "Series 2", Data = new double[] { 5, 15, 25 } }
            };
            string[] xAxisLabels = { "Jan", "Feb", "Mar" };

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Line)
                .Add(p => p.ChartSeries, chartSeries)
                .Add(p => p.ChartLabels, xAxisLabels)
                .Add(p => p.ChartOptions, new LineChartOptions
                {
                    TooltipMode = TooltipMode.Shared
                }));

            // Initially no tooltip
            comp.FindAll(".mud-chart-shared-tooltip").Should().BeEmpty();

            // Simulate pointer move over the interaction layer
            var interactionLayer = comp.Find("rect[pointer-events='all']");
            interactionLayer.TriggerEvent("onpointermove", new Microsoft.AspNetCore.Components.Web.PointerEventArgs
            {
                OffsetX = 30 + 320, // Middle point roughly (Jan is at 30, Feb at 30+space, Mar at 30+2*space)
                OffsetY = 100
            });

            // Now shared tooltip should be rendered
            comp.FindAll(".mud-chart-shared-tooltip").Should().NotBeEmpty();
            comp.Markup.Should().Contain("Series 1: 20");
            comp.Markup.Should().Contain("Series 2: 15");
        }

        [Test]
        public void LineChart_Crosshair_ShouldRender()
        {
            var chartSeries = new List<ChartSeries<double>>()
            {
                new() { Name = "Series 1", Data = new double[] { 10, 20, 30 } }
            };

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Line)
                .Add(p => p.ChartSeries, chartSeries)
                .Add(p => p.ChartOptions, new LineChartOptions
                {
                    ShowCrosshair = true,
                    CrosshairMode = CrosshairMode.Both
                }));

            // Simulate pointer move
            var interactionLayer = comp.Find("rect[pointer-events='all']");
            interactionLayer.TriggerEvent("onpointermove", new Microsoft.AspNetCore.Components.Web.PointerEventArgs
            {
                OffsetX = 100,
                OffsetY = 150
            });

            // Crosshair lines should be rendered
            comp.FindAll(".mud-chart-crosshair").Count.Should().Be(2);
        }

        [Test]
        public void LineChart_InteractionDisabled_ShouldNotHaveInteractionLayer()
        {
            var chartSeries = new List<ChartSeries<double>>()
            {
                new() { Name = "Series 1", Data = new double[] { 10, 20, 30 } }
            };

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Line)
                .Add(p => p.ChartSeries, chartSeries));

            comp.FindAll("rect[pointer-events='all']").Should().BeEmpty();
        }
    }
}

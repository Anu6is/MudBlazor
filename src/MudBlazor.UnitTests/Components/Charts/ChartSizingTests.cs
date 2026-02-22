// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using Bunit;
using MudBlazor.Charts;
using MudBlazor.Interop;
using MudBlazor.UnitTests.Components;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Charts
{
    public class ChartSizingTests : BunitTest
    {
        [Test]
        public async Task MudAxisChartBase_MatchBoundsToSize_ShouldMatchMeasuredSizeExactly()
        {
            var chartSeries = new List<ChartSeries<double>>()
            {
                new () { Name = "Series 1", Data = new double[] { 10, 20 } },
            };
            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Bar)
                .Add(p => p.MatchBoundsToSize, true)
                .Add(p => p.ChartSeries, chartSeries));

            // Find the internal Bar component
            var barComponent = comp.FindComponent<Bar<double>>();
            var axisChartBase = barComponent.Instance as MudAxisChartBase<double, BarChartOptions>;

            // Manually invoke OnElementSizeChanged with a specific width
            const double measuredWidth = 800.0;
            const double measuredHeight = 400.0;

            await comp.InvokeAsync(() => axisChartBase.OnElementSizeChanged(new ElementSize
            {
                Width = measuredWidth,
                Height = measuredHeight,
                Timestamp = DateTime.Now.Ticks
            }));

            // The viewBox should match the measured size exactly
            comp.WaitForAssertion(() =>
            {
                var svg = comp.Find("svg.mud-chart-bar");
                svg.GetAttribute("viewBox").Should().Be($"0 0 {measuredWidth} {measuredHeight}");
            });
        }

        [Test]
        public async Task MudAxisChartBase_LineChart_MatchBoundsToSize_ShouldMatchMeasuredSizeExactly()
        {
            var chartSeries = new List<ChartSeries<double>>()
            {
                new () { Name = "Series 1", Data = new double[] { 10, 20 } },
            };
            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Line)
                .Add(p => p.MatchBoundsToSize, true)
                .Add(p => p.ChartSeries, chartSeries));

            // Find the internal Line component
            var lineComponent = comp.FindComponent<Line<double>>();
            var axisChartBase = lineComponent.Instance as MudAxisChartBase<double, LineChartOptions>;

            // Manually invoke OnElementSizeChanged with a specific width
            const double measuredWidth = 900.0;
            const double measuredHeight = 500.0;

            await comp.InvokeAsync(() => axisChartBase.OnElementSizeChanged(new ElementSize
            {
                Width = measuredWidth,
                Height = measuredHeight,
                Timestamp = DateTime.Now.Ticks
            }));

            // The viewBox should match the measured size exactly
            comp.WaitForAssertion(() =>
            {
                var svg = comp.Find("svg.mud-chart-line");
                svg.GetAttribute("viewBox").Should().Be($"0 0 {measuredWidth} {measuredHeight}");
            });
        }
    }
}

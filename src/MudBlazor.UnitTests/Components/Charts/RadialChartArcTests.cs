// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Bunit;
using MudBlazor.Charts;
using NUnit.Framework;
using AwesomeAssertions;

namespace MudBlazor.UnitTests.Charts
{
    public class RadialChartArcTests : BunitTest
    {
        [Test]
        public void PieChart_SemiCircle_ViewBox()
        {
            var options = new PieChartOptions
            {
                StartAngle = 180,
                SweepAngle = 180
            };

            var comp = Context.Render<Pie<double>>(parameters => parameters
                .Add(p => p.ChartOptions, options)
                .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Data = new double[] { 100 } } }));

            // Semi-circle from 180 to 360 (top half)
            // Start (180 deg): (-100, 0)
            // Mid (270 deg): (0, -100)
            // End (360 deg): (100, 0)
            // Center: (0, 0)
            // MinX: -100, MaxX: 100, MinY: -100, MaxY: 0
            // Width: 200, Height: 100
            // ViewBox should be "-100 -100 200 100"
            var svg = comp.Find("svg");
            svg.GetAttribute("viewBox").Should().Be("-100 -100 200 100");
        }

        [Test]
        public void PieChart_QuarterCircle_ViewBox()
        {
            var options = new PieChartOptions
            {
                StartAngle = 0,
                SweepAngle = 90
            };

            var comp = Context.Render<Pie<double>>(parameters => parameters
                .Add(p => p.ChartOptions, options)
                .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Data = new double[] { 100 } } }));

            // Quarter-circle from 0 to 90 (bottom-right quadrant)
            // Start (0 deg): (100, 0)
            // Mid (45 deg): (70.71, 70.71)
            // End (90 deg): (0, 100)
            // Center: (0, 0)
            // MinX: 0, MaxX: 100, MinY: 0, MaxY: 100
            // Width: 100, Height: 100
            // ViewBox should be "0 0 100 100"
            var svg = comp.Find("svg");
            svg.GetAttribute("viewBox").Should().Be("0 0 100 100");
        }

        [Test]
        public void DonutChart_SemiCircle_Path()
        {
            var options = new DonutChartOptions
            {
                StartAngle = 180,
                SweepAngle = 180,
                DonutRingRatio = 0.5
            };

            var comp = Context.Render<Donut<double>>(parameters => parameters
                .Add(p => p.ChartOptions, options)
                .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Data = new double[] { 100 } } }));

            // Outer Radius: 100, Inner Radius: 50
            // Start (180 deg): Outer (-100, 0), Inner (-50, 0)
            // End (360 deg): Outer (100, 0), Inner (50, 0)
            // Path should move to (-100, 0), arc to (100, 0), line to (50, 0), arc back to (-50, 0)
            var path = comp.Find("path");
            path.GetAttribute("d").Should().Contain("M -100 0");
            path.GetAttribute("d").Should().Contain("A 100 100 0 0 1 100 0");
            path.GetAttribute("d").Should().Contain("L 50 0");
            path.GetAttribute("d").Should().Contain("A 50 50 0 0 0 -50 0");
        }
    }
}

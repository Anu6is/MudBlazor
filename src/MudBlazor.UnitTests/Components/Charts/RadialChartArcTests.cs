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
            // Bounds: MinX: -100, MaxX: 100, MinY: -100, MaxY: 0
            // Center of bounds: (0, -50)
            // ViewBox is Radius 100 centered at (0, -50) => MinX: 0 - 100 = -100, MinY: -50 - 100 = -150
            // ViewBox should be "-100 -150 200 200"
            var svg = comp.Find("svg");
            svg.GetAttribute("viewBox").Should().Be("-100 -150 200 200");
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
            // Bounds: MinX: 0, MaxX: 100, MinY: 0, MaxY: 100
            // Center of bounds: (50, 50)
            // ViewBox is Radius 100 centered at (50, 50) => MinX: 50 - 100 = -50, MinY: 50 - 100 = -50
            // ViewBox should be "-50 -50 200 200"
            var svg = comp.Find("svg");
            svg.GetAttribute("viewBox").Should().Be("-50 -50 200 200");
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

        [Test]
        public void PieChart_SweepAngleZero_ViewBox()
        {
            var options = new PieChartOptions
            {
                StartAngle = 0,
                SweepAngle = 0
            };

            var comp = Context.Render<Pie<double>>(parameters => parameters
                .Add(p => p.ChartOptions, options)
                .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Data = new double[] { 100 } } }));

            // SweepAngle <= 0 should fallback to full-circle centered viewBox
            var svg = comp.Find("svg");
            svg.GetAttribute("viewBox").Should().Be("-100 -100 200 200");
        }

        [Test]
        public void PieChart_SingleSegmentPartialSweep_NoSplit()
        {
            var options = new PieChartOptions
            {
                StartAngle = 0,
                SweepAngle = 270
            };

            var comp = Context.Render<Pie<double>>(parameters => parameters
                .Add(p => p.ChartOptions, options)
                .Add(p => p.ChartSeries, new List<ChartSeries<double>> { new() { Data = new double[] { 100 } } }));

            // Single segment with SweepAngle 270 should NOT split into two arcs.
            // Normalized data will be 1.0, but segment radians will be 270 deg (1.5 PI).
            // A 270 deg arc should have arcFlag 1 and NOT use the two-arc split workaround.
            var path = comp.Find("path");
            var d = path.GetAttribute("d");
            // Check for single 'A' command (no split)
            var arcCommandsCount = d.Split('A').Length - 1;
            arcCommandsCount.Should().Be(1);
            // Check for large arc flag 1
            d.Should().Contain("A 100 100 0 1 1");
        }
    }
}

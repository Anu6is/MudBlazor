// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor.Charts;
using MudBlazor.Interop;
using MudBlazor.UnitTests.Shared;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class ChartMatchBoundsToSizeTests : BunitTest
    {
        [Test]
        public async Task MatchBoundsToSize_WithPercentageHeight_ShouldNotLoop()
        {
            var series = new List<ChartSeries<double>>
            {
                new() { Data = new double[] { 12.2, 14.3, 11.5 } }
            };
            var labels = new[] { "1/1/26", "2/1/26", "3/1/26" };

            var initialSize = new ElementSize { Width = 700, Height = 350, Timestamp = 1 };

            Context.JSInterop.Setup<ElementSize>("mudObserveElementSize", _ => true)
                .SetResult(initialSize);

            Context.JSInterop.Setup<ElementSize>("mudGetSvgBBox", _ => true)
                .SetResult(new ElementSize { Width = 50, Height = 20 });

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Line)
                .Add(p => p.ChartSeries, series)
                .Add(p => p.ChartLabels, labels)
                .Add(p => p.MatchBoundsToSize, true)
                .Add(p => p.Width, "100%")
                .Add(p => p.Height, "80%")
            );

            var chartBase = comp.FindComponent<Line<double>>().Instance;

            var svg = comp.Find("svg");
            svg.GetAttribute("viewBox").Should().Be("0 0 700 350");

            var largerSize = new ElementSize { Width = 700, Height = 400, Timestamp = 2 };

            await comp.InvokeAsync(() => chartBase.OnElementSizeChanged(largerSize));

            await Task.Delay(300);

            svg = comp.Find("svg");
            // CURRENT BEHAVIOR: It updates to 400.
            // EXPECTED BEHAVIOR after fix: It should stay at 350 (or whatever it was initially set to when parsing Height/Width)
            // because Height is percentage.
            svg.GetAttribute("viewBox").Should().Be("0 0 700 350");
        }

        [Test]
        public async Task MatchBoundsToSize_WithPixelHeight_ShouldUpdate()
        {
            var series = new List<ChartSeries<double>>
            {
                new() { Data = new double[] { 12.2, 14.3, 11.5 } }
            };
            var labels = new[] { "1/1/26", "2/1/26", "3/1/26" };

            var initialSize = new ElementSize { Width = 700, Height = 350, Timestamp = 1 };

            Context.JSInterop.Setup<ElementSize>("mudObserveElementSize", _ => true)
                .SetResult(initialSize);

            Context.JSInterop.Setup<ElementSize>("mudGetSvgBBox", _ => true)
                .SetResult(new ElementSize { Width = 50, Height = 20 });

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Line)
                .Add(p => p.ChartSeries, series)
                .Add(p => p.ChartLabels, labels)
                .Add(p => p.MatchBoundsToSize, true)
                .Add(p => p.Width, "700px")
                .Add(p => p.Height, "350px")
            );

            var chartBase = comp.FindComponent<Line<double>>().Instance;

            var svg = comp.Find("svg");
            svg.GetAttribute("viewBox").Should().Be("0 0 700 350");

            var largerSize = new ElementSize { Width = 800, Height = 400, Timestamp = 2 };

            await comp.InvokeAsync(() => chartBase.OnElementSizeChanged(largerSize));

            await Task.Delay(300);

            svg = comp.Find("svg");
            svg.GetAttribute("viewBox").Should().Be("0 0 800 400");
        }

        [Test]
        public void MatchBoundsToSize_WithFixedWidthAndRelativeHeight_ShouldUseFixedWidthAndDefaultHeight()
        {
            var series = new List<ChartSeries<double>>
            {
                new() { Data = new double[] { 12.2, 14.3, 11.5 } }
            };
            var labels = new[] { "1/1/26", "2/1/26", "3/1/26" };

            // We do NOT mock the ResizeObserver here, as we are testing the fallback logic in SetBounds
            // during the initial render before OnAfterRenderAsync (where mudObserveElementSize is called).
            // Actually, mudObserveElementSize is called in OnAfterRenderAsync, but SetBounds is called
            // in RebuildChart, which is called in OnParametersSet.

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Line)
                .Add(p => p.ChartSeries, series)
                .Add(p => p.ChartLabels, labels)
                .Add(p => p.MatchBoundsToSize, true)
                .Add(p => p.Width, "500px")
                .Add(p => p.Height, "80%")
            );

            var svg = comp.Find("svg");
            // width should be 500, height should be BoundHeightDefault (350)
            svg.GetAttribute("viewBox").Should().Be("0 0 500 350");
        }

        [Test]
        public void RadialMatchBoundsToSize_WithFixedWidthAndRelativeHeight_ShouldUseFixedWidthAndDefaultHeight()
        {
            var series = new List<ChartSeries<double>>
            {
                new() { Data = new double[] { 12.2 } }
            };

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Pie)
                .Add(p => p.ChartSeries, series)
                .Add(p => p.MatchBoundsToSize, true)
                .Add(p => p.Width, "200px")
                .Add(p => p.Height, "80%")
            );

            var svg = comp.Find("svg");
            // min dimension between 200 and 280 (default) is 200. Radius is 100.
            // viewBox is 0 0 200 200
            svg.GetAttribute("viewBox").Should().Be("0 0 200 200");
        }
    }
}

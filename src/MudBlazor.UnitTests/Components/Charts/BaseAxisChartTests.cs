// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable
using Microsoft.AspNetCore.Components;
using Bunit;
using AwesomeAssertions;
using MudBlazor.Charts;
using MudBlazor.Interop;
using NUnit.Framework;
using MudBlazor.UnitTests.Shared;

namespace MudBlazor.UnitTests.Components.Charts
{
    [TestFixture]
    public class BaseAxisChartTests : BunitTest
    {
        [Test]
        public void YAxisLabelSizeChanged_ShouldFire_WhenHeightChanges()
        {
            var yAxisLabelSize = new ElementSize { Width = 50, Height = 100 };
            var callCount = 0;
            ElementSize? lastReportedSize = null;

            var dotNetObject = Context.JSInterop.Setup<ElementSize>("mudGetSvgBBox", _ => true);
            dotNetObject.SetResult(yAxisLabelSize);

            var comp = Context.Render<BaseAxisChart<double, BarChartOptions>>(parameters => parameters
                .Add(p => p.YAxisLabelSizeChanged, EventCallback.Factory.Create<ElementSize?>(this, size =>
                {
                    callCount++;
                    lastReportedSize = size;
                }))
                .Add(p => p.ChartOptions, new BarChartOptions())
            );

            callCount.Should().Be(1);
            lastReportedSize.Should().NotBeNull();
            lastReportedSize!.Height.Should().Be(100);
            lastReportedSize.Width.Should().Be(50);

            // Change only Height
            yAxisLabelSize = new ElementSize { Width = 50, Height = 120 };
            dotNetObject.SetResult(yAxisLabelSize);

            comp.Render();

            // Now this should PASS
            callCount.Should().Be(2, "YAxisLabelSizeChanged should fire when Height changes");
            lastReportedSize.Height.Should().Be(120);
        }

        [Test]
        public void XAxisLabelSizeChanged_ShouldFire_WhenWidthChanges()
        {
            var xAxisLabelSize = new ElementSize { Width = 200, Height = 20 };
            var callCount = 0;
            ElementSize? lastReportedSize = null;

            var dotNetObject = Context.JSInterop.Setup<ElementSize>("mudGetSvgBBox", _ => true);
            dotNetObject.SetResult(xAxisLabelSize);

            var comp = Context.Render<BaseAxisChart<double, BarChartOptions>>(parameters => parameters
                .Add(p => p.XAxisLabelSizeChanged, EventCallback.Factory.Create<ElementSize?>(this, size =>
                {
                    callCount++;
                    lastReportedSize = size;
                }))
                .Add(p => p.ChartOptions, new BarChartOptions())
            );

            callCount.Should().Be(1);
            lastReportedSize.Should().NotBeNull();
            lastReportedSize!.Height.Should().Be(20);
            lastReportedSize.Width.Should().Be(200);

            // Change only Width
            xAxisLabelSize = new ElementSize { Width = 250, Height = 20 };
            dotNetObject.SetResult(xAxisLabelSize);

            comp.Render();

            // Now this should PASS
            callCount.Should().Be(2, "XAxisLabelSizeChanged should fire when Width changes");
            lastReportedSize.Width.Should().Be(250);
        }
    }
}

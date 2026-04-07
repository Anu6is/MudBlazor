using Bunit;
using MudBlazor.Charts;
using NUnit.Framework;
using AwesomeAssertions;

namespace MudBlazor.UnitTests.Charts
{
    [TestFixture]
    public class ScatterPlotOverlayTests : BunitTest
    {
        [Test]
        public async Task ScatterPlotAsOverlay_RendersPoints()
        {
            var barSeries = new List<ChartSeries<double>>
            {
                new() { Name = "Bar Series", Data = new double[] { 10, 20, 30 } }
            };

            var scatterSeries = new List<ChartSeries<double>>
            {
                new() { Name = "Scatter Overlay", Data = new double[] { 15, 25, 35 } }
            };

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Bar)
                .Add(p => p.ChartSeries, barSeries)
                .Add(p => p.ChildContent, builder =>
                {
                    builder.OpenComponent<ScatterPlot<double>>(0);
                    builder.AddAttribute(1, nameof(ScatterPlot<double>.ChartSeries), scatterSeries);
                    builder.CloseComponent();
                }));

            // Trigger a re-render to ensure child components are initialized and registered
            comp.Render();

            var circles = comp.FindAll("circle.mud-chart-point");
            circles.Count.Should().Be(6);
        }

        [Test]
        public async Task ScatterPlotAsOverlay_UsesIndicesForX_WhenXIsNull()
        {
            var barSeries = new List<ChartSeries<double>>
            {
                new() { Name = "Bar Series", Data = new double[] { 100 } }
            };

            var scatterSeries = new List<ChartSeries<double>>
            {
                new() { Name = "Scatter Overlay", Data = new double[] { 50 } } // X is null
            };

            var comp = Context.Render<MudChart<double>>(parameters => parameters
                .Add(p => p.ChartType, ChartType.Bar)
                .Add(p => p.ChartSeries, barSeries)
                .Add(p => p.ChildContent, builder =>
                {
                    builder.OpenComponent<ScatterPlot<double>>(0);
                    builder.AddAttribute(1, nameof(ScatterPlot<double>.ChartSeries), scatterSeries);
                    builder.CloseComponent();
                }));

            // Trigger a re-render to ensure child components are initialized and registered
            comp.Render();

            // If it didn't throw and rendered circles, it's using indices.
            comp.FindAll("circle.mud-chart-point").Count.Should().Be(2);
        }
    }
}

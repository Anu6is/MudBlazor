using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Charts;
using MudBlazor.Docs.Examples; // For HorizontalBarChartExample
using MudBlazor.UnitTests.TestComponents; // For general test components
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components.Charts
{
    [TestFixture]
    public class HorizontalBarChartTests : BunitTest
    {
        private List<ChartSeries> _singleSeries = new List<ChartSeries>()
        {
            new ChartSeries() { Name = "Series 1", Data = new double[] { 90, 79, 72 } }
        };
        private string[] _xAxisLabels = { "Category A", "Category B", "Category C" };

        [SetUp]
        public void Setup()
        {
            TestContext.Services.AddMudServices();
        }

        [Test]
        public void HorizontalBarChart_ShouldRender()
        {
            // Arrange
            var comp = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, _singleSeries)
                .Add(p => p.XAxisLabels, _xAxisLabels)
                .Add(p => p.Width, 400)
                .Add(p => p.Height, 300)
            );

            // Assert
            comp.Markup.Should().NotBeNullOrEmpty();
            comp.Find("svg").Should().NotBeNull();
            comp.Find(".mud-chart-horizontalbar").Should().NotBeNull();
        }

        [Test]
        public void HorizontalBarChart_ShouldRender_NoData()
        {
            // Arrange
            var comp = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, new List<ChartSeries>()) // No data
                .Add(p => p.XAxisLabels, Array.Empty<string>())
                .Add(p => p.Width, 400)
                .Add(p => p.Height, 300)
            );

            // Assert
            comp.Markup.Should().NotBeNullOrEmpty();
            comp.Find("svg").Should().NotBeNull();
            comp.Find(".mud-chart-horizontalbar").Should().NotBeNull();
            comp.FindAll("path.mud-chart-bar").Should().BeEmpty(); // No bars should be rendered
        }

        [Test]
        public void HorizontalBarChart_DataDisplay_CorrectNumberOfBars()
        {
            // Arrange
            var series = new List<ChartSeries>()
            {
                new ChartSeries() { Name = "Series 1", Data = new double[] { 10, 20, 30 } },
                new ChartSeries() { Name = "Series 2", Data = new double[] { 15, 25, 35 } }
            };
            var labels = new[] { "A", "B", "C" };

            var comp = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, series)
                .Add(p => p.XAxisLabels, labels)
            );

            // Assert
            // Each series contributes 'labels.Length' bars
            comp.FindAll("path.mud-chart-bar").Count.Should().Be(series.Count * labels.Length);
        }
        
        // Helper to extract path 'd' attributes for bars
        private IEnumerable<string> GetBarPathData(IRenderedComponent<MudChart> chartComponent)
        {
            return chartComponent.FindAll("g.mud-charts-bar-series path.mud-chart-bar").Select(p => p.GetAttribute("d"));
        }

        // Helper to parse M and L commands from SVG path data for horizontal bars
        // M x1 y1 L x2 y2  => (x1, y1, x2, y2)
        // For horizontal bar: y1 == y2
        private IEnumerable<(double x1, double y, double x2)> ParseHorizontalBarPath(string pathData)
        {
            var regex = new Regex(@"M\s*([0-9.-]+)\s*([0-9.-]+)\s*L\s*([0-9.-]+)\s*([0-9.-]+)");
            var matches = regex.Matches(pathData);
            foreach (Match match in matches)
            {
                if (match.Groups.Count == 5)
                {
                    double x1 = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                    double y1 = double.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
                    double x2 = double.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
                    double y2 = double.Parse(match.Groups[4].Value, System.Globalization.CultureInfo.InvariantCulture);
                    if (Math.Abs(y1 - y2) < 0.001) // Ensure it's a horizontal line
                    {
                        yield return (x1, y1, x2);
                    }
                }
            }
        }


        [Test]
        public void HorizontalBarChart_DataDisplay_BarLengthAndPosition()
        {
            // This test is more complex and qualitative without exact rendering dimensions.
            // We'll check for relative lengths and positions.
            // Series 1: 10, 30. Series 2: 5, 15 (for same categories)
            var series = new List<ChartSeries>()
            {
                new ChartSeries() { Name = "S1", Data = new double[] { 10, 30 } },
                new ChartSeries() { Name = "S2", Data = new double[] { 5, 15 } }
            };
            var labels = new[] { "CatA", "CatB" };

            var comp = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, series)
                .Add(p => p.XAxisLabels, labels)
                .Add(p => p.Width, 400)
                .Add(p => p.Height, 300)
                .Add(p => p.ChartOptions, new HorizontalBarChartOptions { XAxisMinSet = 0, XAxisMaxSet = 30 }) // Fixed scale for predictability
            );
            
            var paths = GetBarPathData(comp).ToList();
            paths.Count.Should().Be(4); // 2 series * 2 categories

            var parsedBars = paths.SelectMany(ParseHorizontalBarPath).ToList();

            // S1, CatA (value 10)
            // S2, CatA (value 5)
            // S1, CatB (value 30)
            // S2, CatB (value 15)
            // Order in DOM might be by series then category, or by category then series.
            // The component's GenerateBars iterates series first, then categories.
            
            var barS1C1 = parsedBars[0]; // Series 1, Cat A (val 10)
            var barS1C2 = parsedBars[1]; // Series 1, Cat B (val 30)
            var barS2C1 = parsedBars[2]; // Series 2, Cat A (val 5)
            var barS2C2 = parsedBars[3]; // Series 2, Cat B (val 15)

            // Bar lengths (x2 - x1) should be proportional to data values.
            // And x1 should be similar if they start from 0 line.
            (barS1C2.x2 - barS1C2.x1).Should().BeApproximately(3 * (barS1C1.x2 - barS1C1.x1), 0.1); // 30 vs 10
            (barS2C2.x2 - barS2C2.x1).Should().BeApproximately(3 * (barS2C1.x2 - barS2C1.x1), 0.1); // 15 vs 5
            (barS1C1.x2 - barS1C1.x1).Should().BeApproximately(2 * (barS2C1.x2 - barS2C1.x1), 0.1); // 10 vs 5
            
            // Bars in the same category group should have different Y positions
            barS1C1.y.Should().NotBe(barS2C1.y); 
            // Bars for the same series but different categories should have different Y positions
            barS1C1.y.Should().NotBe(barS1C2.y);
        }

        [Test]
        public void HorizontalBarChart_DataDisplay_AxisLabels()
        {
            var comp = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, _singleSeries) // Data: 90, 79, 72
                .Add(p => p.XAxisLabels, _xAxisLabels) // Cat A, Cat B, Cat C
                .Add(p => p.ChartOptions, new HorizontalBarChartOptions() { XAxisTicks = 20, XAxisMinSet=0, XAxisMaxSet=100 })
            );

            // Check Y-Axis Labels (Categories)
            var yAxisTexts = comp.FindAll("g.mud-charts-yaxis text").Select(t => t.TextContent).ToList();
            yAxisTexts.Should().Contain("Category A");
            yAxisTexts.Should().Contain("Category B");
            yAxisTexts.Should().Contain("Category C");

            // Check X-Axis Labels (Values) - these are generated based on data scale and XAxisTicks
            var xAxisTexts = comp.FindAll("g.mud-charts-xaxis text").Select(t => t.TextContent).ToList();
            // Expected values based on XAxisTicks=20, Min=0, Max=100: "0", "20", "40", "60", "80", "100"
            xAxisTexts.Should().Contain("0");
            xAxisTexts.Should().Contain("20");
            xAxisTexts.Should().Contain("40");
            xAxisTexts.Should().Contain("60");
            xAxisTexts.Should().Contain("80");
            xAxisTexts.Should().Contain("100");
        }

        [Test]
        public void HorizontalBarChart_Options_FixedBarHeight()
        {
            double fixedHeight = 25;
            var comp = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, _singleSeries) // 3 categories
                .Add(p => p.XAxisLabels, _xAxisLabels)
                .Add(p => p.ChartOptions, new HorizontalBarChartOptions { FixedBarHeight = fixedHeight })
            );

            var paths = comp.FindAll("path.mud-chart-bar");
            paths.Count.Should().Be(_singleSeries[0].Data.Length); // 3 bars

            foreach (var path in paths)
            {
                // Stroke-width attribute is set to the bar's thickness (_barHeight) in HorizontalBar.razor
                var strokeWidth = path.GetAttribute("stroke-width");
                strokeWidth.Should().NotBeNullOrEmpty();
                double.Parse(strokeWidth, System.Globalization.CultureInfo.InvariantCulture).Should().Be(fixedHeight);
            }
        }

        [Test]
        public void HorizontalBarChart_Options_BarHeightRatio()
        {
            // This test is more challenging as it depends on calculated available space.
            // We'll check that different ratios result in different stroke-widths (bar heights).
            // A larger ratio should result in a larger bar height if other params are constant.

            var options1 = new HorizontalBarChartOptions { BarHeightRatio = 0.3 };
            var comp1 = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, _singleSeries)
                .Add(p => p.XAxisLabels, _xAxisLabels)
                .Add(p => p.ChartOptions, options1)
                .Add(p => p.Height, 300) // Fixed height for chart area
            );
            var barHeight1 = double.Parse(comp1.FindAll("path.mud-chart-bar").First().GetAttribute("stroke-width"), System.Globalization.CultureInfo.InvariantCulture);

            var options2 = new HorizontalBarChartOptions { BarHeightRatio = 0.7 };
            var comp2 = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, _singleSeries)
                .Add(p => p.XAxisLabels, _xAxisLabels)
                .Add(p => p.ChartOptions, options2)
                .Add(p => p.Height, 300) // Fixed height for chart area
            );
            var barHeight2 = double.Parse(comp2.FindAll("path.mud-chart-bar").First().GetAttribute("stroke-width"), System.Globalization.CultureInfo.InvariantCulture);
            
            barHeight2.Should().BeGreaterThan(barHeight1);
        }


        [Test]
        public void HorizontalBarChart_Options_BarGroupGap()
        {
            // Test that increasing BarGroupGap increases the Y distance between midpoints of bars from different categories.
            var series = new List<ChartSeries>() { new ChartSeries() { Name = "S1", Data = new double[] { 10, 20 } } };
            var labels = new[] { "CatA", "CatB" };

            var optionsSmallGap = new HorizontalBarChartOptions { BarGroupGap = 5, FixedBarHeight = 10 };
            var compSmallGap = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, series)
                .Add(p => p.XAxisLabels, labels)
                .Add(p => p.ChartOptions, optionsSmallGap)
                .Add(p => p.Height, 200)
            );
            var parsedBarsSmallGap = GetBarPathData(compSmallGap).SelectMany(ParseHorizontalBarPath).ToList();
            var yDiffSmall = Math.Abs(parsedBarsSmallGap[1].y - parsedBarsSmallGap[0].y);

            var optionsLargeGap = new HorizontalBarChartOptions { BarGroupGap = 25, FixedBarHeight = 10 };
            var compLargeGap = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, series)
                .Add(p => p.XAxisLabels, labels)
                .Add(p => p.ChartOptions, optionsLargeGap)
                .Add(p => p.Height, 200)
            );
            var parsedBarsLargeGap = GetBarPathData(compLargeGap).SelectMany(ParseHorizontalBarPath).ToList();
            var yDiffLarge = Math.Abs(parsedBarsLargeGap[1].y - parsedBarsLargeGap[0].y);

            yDiffLarge.Should().BeGreaterThan(yDiffSmall);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void HorizontalBarChart_Options_ShowCategoryLines(bool showLines)
        {
            var comp = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, _singleSeries)
                .Add(p => p.XAxisLabels, _xAxisLabels)
                .Add(p => p.ChartOptions, new HorizontalBarChartOptions { ShowCategoryLines = showLines })
            );

            // HorizontalLines in HorizontalBarChart are category lines (Y-axis grid lines).
            // BaseAxisChart renders these as <path class="mud-chart-grid-line">
            var gridLines = comp.FindAll("g.mud-charts-grid path.mud-chart-grid-line.mud-chart-grid-line-horizontal");
            
            if (showLines)
            {
                // Number of category lines can be equal to number of categories or more/less depending on exact drawing logic.
                // For this test, just checking presence vs absence is more robust.
                gridLines.Count.Should().BeGreaterThan(0);
            }
            else
            {
                gridLines.Should().BeEmpty();
            }
        }
        
        [Test]
        public void HorizontalBarChart_EdgeCase_NegativeValues()
        {
            var seriesWithNegative = new List<ChartSeries>()
            {
                new ChartSeries() { Name = "Series 1", Data = new double[] { 50, -30, 20 } }
            };
            var labels = new[] { "A", "B", "C" };

            var comp = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, seriesWithNegative)
                .Add(p => p.XAxisLabels, labels)
                .Add(p => p.ChartOptions, new HorizontalBarChartOptions() { XAxisMinSet = -50, XAxisMaxSet = 50, XAxisTicks = 10 })
            );
            
            comp.FindAll("path.mud-chart-bar").Count.Should().Be(3);

            var parsedBars = GetBarPathData(comp).SelectMany(ParseHorizontalBarPath).ToList();
            var barPositive = parsedBars[0]; // Value 50
            var barNegative = parsedBars[1]; // Value -30
            var barPositive2 = parsedBars[2]; // Value 20

            // Assuming 0 is the dividing line.
            // A positive bar starts at/after 0-line and ends to the right.
            // A negative bar starts before 0-line and ends at/before 0-line.
            // The exact X for "0" depends on HorizontalStartSpace and scaling.
            // We can find the "zero line" by looking at where the X-axis labels are.
            var xAxisGridLines = comp.FindAll("g.mud-charts-grid path.mud-chart-grid-line-vertical");
            var zeroLineX = -1.0;

            var xAxisTexts = comp.FindAll("g.mud-charts-xaxis text");
            for(int i=0; i<xAxisTexts.Count; ++i)
            {
                if(xAxisTexts[i].TextContent == "0")
                {
                    // The corresponding grid line x is our zeroLineX
                    // This assumes text elements and line elements are somewhat ordered or matchable.
                    // A more robust way would be to parse the transform of the text and its x attribute.
                    // For now, let's assume the grid lines are ordered similarly to data values.
                    // If XAxisTicks is 10, and MinSet is -50, then "0" is the 5th tick (index 5).
                    // So the 6th vertical grid line (index 5) should be at X=0.
                     var linePath = xAxisGridLines.FirstOrDefault(gl => {
                        var d = gl.GetAttribute("d"); // M x y L x y
                        var match = Regex.Match(d, @"M\s*([0-9.-]+)");
                        if (match.Success) {
                           var xPos = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                           // This is still tricky as we don't know which xPos corresponds to "0" without more details.
                           // Let's find the x of the "0" label directly
                           var xOfLabel = double.Parse(xAxisTexts[i].GetAttribute("x"), System.Globalization.CultureInfo.InvariantCulture);
                           zeroLineX = xOfLabel; // this is the x-coordinate of the "0" label, which should align with the 0-gridline
                           return true;
                        }
                        return false;
                     });
                    break;
                }
            }
            zeroLineX.Should().NotBe(-1.0, "Zero line X coordinate should have been found.");

            // Bar 1 (50): should be to the right of zeroLineX or start near it if zeroLineX is the start.
            (barPositive.x2 - barPositive.x1).Should().BeGreaterThan(0); // Positive length
            barPositive.x1.Should().BeGreaterOrEqualTo(zeroLineX -1); // Allow small tolerance if zeroLineX is the absolute chart start due to all positive values
            barPositive.x2.Should().BeGreaterThan(barPositive.x1);

            // Bar 2 (-30): should be to the left of zeroLineX.
            (barNegative.x2 - barNegative.x1).Should().BeLessThan(0); // Negative length (x2 < x1)
            barNegative.x2.Should().BeLessOrEqualTo(zeroLineX +1 ); // Ends at or before zero
            barNegative.x1.Should().BeLessThan(barNegative.x2);
            
            // Bar 3 (20): similar to Bar 1
            (barPositive2.x2 - barPositive2.x1).Should().BeGreaterThan(0);
            barPositive2.x1.Should().BeGreaterOrEqualTo(zeroLineX -1);
            barPositive2.x2.Should().BeGreaterThan(barPositive2.x1);
        }
        
        [Test]
        public void HorizontalBarChart_Options_CustomColors()
        {
            var customColors = new[] { "#FF0000", "#00FF00", "#0000FF" }; // Red, Green, Blue
            var comp = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, _singleSeries) // 3 categories, 1 series
                .Add(p => p.XAxisLabels, _xAxisLabels)
                .Add(p => p.ChartOptions, new HorizontalBarChartOptions { ChartPalette = customColors })
            );

            var paths = comp.FindAll("path.mud-chart-bar");
            paths.Count.Should().Be(_xAxisLabels.Length);

            // In HorizontalBar, bars are colored by series index.
            // Since there's only one series, all bars should get the first color of the palette.
            // If we had multiple series, series[0] would be customColors[0], series[1] customColors[1], etc.
            // The current implementation in HorizontalBar.razor:
            // color = ChartOptions.ChartPalette.GetValue(bar.Index % ChartOptions.ChartPalette.Length)
            // bar.Index is the series index. So for _singleSeries (index 0), all bars get customColors[0].

            foreach (var path in paths)
            {
                path.GetAttribute("fill").Should().Be(customColors[0]);
            }

            // Test with multiple series to see color cycling
            var multiSeries = new List<ChartSeries>()
            {
                new ChartSeries() { Name = "S1", Data = new double[] { 10 } },
                new ChartSeries() { Name = "S2", Data = new double[] { 20 } },
                new ChartSeries() { Name = "S3", Data = new double[] { 30 } },
                new ChartSeries() { Name = "S4", Data = new double[] { 40 } } // Should cycle back to Red
            };
            var singleLabel = new[] { "Category" };

            comp = Context.RenderComponent<MudChart>(parameters => parameters
                .Add(p => p.ChartType, ChartType.HorizontalBar)
                .Add(p => p.ChartSeries, multiSeries)
                .Add(p => p.XAxisLabels, singleLabel)
                .Add(p => p.ChartOptions, new HorizontalBarChartOptions { ChartPalette = customColors })
            );
            
            paths = comp.FindAll("path.mud-chart-bar");
            paths.Count.Should().Be(multiSeries.Count); // 4 bars, 1 for each series

            // Order of bars in DOM: Series1/CatA, Series2/CatA, Series3/CatA, Series4/CatA
            paths[0].GetAttribute("fill").Should().Be(customColors[0]); // S1
            paths[1].GetAttribute("fill").Should().Be(customColors[1]); // S2
            paths[2].GetAttribute("fill").Should().Be(customColors[2]); // S3
            paths[3].GetAttribute("fill").Should().Be(customColors[0]); // S4 (cycles)
        }
    }
}

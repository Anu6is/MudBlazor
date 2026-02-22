using System.Numerics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Extensions;
using MudBlazor.Interfaces;
using MudBlazor.Justification.BarGroup;

#nullable enable
namespace MudBlazor.Charts.Base
{
    public abstract class MudBarChartBase<T, TOptions> : MudAxisChartBase<T, TOptions> where T : struct, INumber<T>, IMinMaxValue<T>, IFormattable where TOptions : BarChartOptions, new()
    {
        protected readonly List<SvgPath> _bars = [];
        protected SvgPath? _hoveredBar;

        protected double _barGroupWidth;
        protected double _barWidth;
        protected double _barGap;

        protected const double MinBarWidth = 6;

        public override RenderFragment? OverlayContent { get; set; }

        protected abstract RenderFragment Chart { get; }

        protected override void OnInitialized()
        {
            ChartOptions ??= new TOptions();

            if (ChartReference is IMudAxisChart<T> axisChart)
            {
                axisChart.OverlayChart = this;
                axisChart.OverlayContent = this.Chart;
            }

            base.OnInitialized();
        }

        public override void RebuildChart()
        {
            // shared plot points should be initialized before generating overlay charts
            if (IsOverlayChart && SharedData is null) return;

            Series = (ChartContainer != null && ChartReference is MudChart<T>)
                ? ChartContainer.ChartSeries
                : ChartSeries;

            GeneratePlotArea(out var gridYUnits, out var lowestHorizontalLine, out var numHorizontalLines, out var numVerticalLines, out var horizontalSpace, out var verticalSpace);

            if (!IsOverlayChart)
            {
                // If this is not an overlay chart, we generate the shared plot points if an overlay exists
                SharedData = OverlayChart is IMudAxisChart<T> ? new AxisGridData<T>(lowestHorizontalLine, numHorizontalLines, gridYUnits, _boundWidth, _boundHeight) : null;
            }
            else
            {
                // If this is an overlay chart, we use the shared plot points from the main chart
                var area = SharedData!.Value;

                lowestHorizontalLine = SharedData.Value.LowestHorizontalLine;
                gridYUnits = SharedData.Value.YAxisTicks;

                _boundWidth = area.BoundWidth;
                _boundHeight = area.BoundHeight;
            }

            GenerateBars(lowestHorizontalLine, gridYUnits, horizontalSpace, verticalSpace, numVerticalLines);
            GenerateLegends();
            RenderOverlay();
        }

        protected abstract void GenerateBars(int lowestHorizontalLine, T gridYUnits, double horizontalSpace, double verticalSpace, int numVerticalLines);

        protected virtual void GeneratePlotArea(out T gridYUnits, out int lowestHorizontalLine, out int numHorizontalLines, out int numVerticalLines, out double horizontalSpace, out double verticalSpace)
        {
            SetBounds();
            ComputeUnitsAndNumberOfLines(out gridYUnits, out numHorizontalLines, out lowestHorizontalLine, out numVerticalLines);

            var horizontalLines = IsOverlayChart ? SharedData!.Value.HorizontalLineCount - 1 : numHorizontalLines;

            horizontalSpace = _boundWidth - HorizontalStartSpace - HorizontalEndSpace;
            verticalSpace = (_boundHeight - VerticalStartSpace - VerticalEndSpace) / Math.Max(1, horizontalLines);
            var tickWidth = horizontalSpace / numVerticalLines;

            ComputeBarDimensions(tickWidth);
            GenerateValueAxisGridLines(numHorizontalLines, lowestHorizontalLine, gridYUnits, verticalSpace);
            GenerateCategoryAxisGridLines(numVerticalLines, horizontalSpace);
        }

        protected virtual void ComputeUnitsAndNumberOfLines(out T gridYUnits, out int numHorizontalLines, out int lowestHorizontalLine, out int numVerticalLines)
        {
            var yAxisTicks = ChartOptions?.YAxisTicks;
            if (yAxisTicks.HasValue && yAxisTicks.Value > 0)
                gridYUnits = T.CreateSaturating(yAxisTicks.Value);
            else
                gridYUnits = T.CreateSaturating(20);

            var allValues = Series.SelectMany(series => series.Data.Values);

            if (allValues.Any())
            {
                var minY = allValues.Min();
                var maxY = ChartOptions?.YAxisSuggestedMax is null
                    ? allValues.Max()
                    : T.Max(T.CreateSaturating(ChartOptions.YAxisSuggestedMax.Value), allValues.Max());

                lowestHorizontalLine = Math.Min((int)Math.Floor(double.CreateSaturating(minY / gridYUnits)), 0);
                var highestHorizontalLine = Math.Max((int)Math.Ceiling(double.CreateSaturating(maxY / gridYUnits)), 0);
                numHorizontalLines = highestHorizontalLine - lowestHorizontalLine + 1;

                // Safeguard against too many gridlines
                var maxYTicks = ChartOptions?.MaxNumYAxisTicks ?? 20;

                while (numHorizontalLines > maxYTicks)
                {
                    gridYUnits *= T.CreateSaturating(2);
                    lowestHorizontalLine = Math.Min((int)Math.Floor(double.CreateSaturating(minY / gridYUnits)), 0);
                    highestHorizontalLine = Math.Max((int)Math.Ceiling(double.CreateSaturating(maxY / gridYUnits)), 0);

                    numHorizontalLines = highestHorizontalLine - lowestHorizontalLine + 1;
                }

                numVerticalLines = Series.Max(series => series.Data.Values.Count);
            }
            else
            {
                numHorizontalLines = 1;
                lowestHorizontalLine = 0;
                numVerticalLines = 1;
            }
        }

        protected virtual void GenerateCategoryAxisGridLines(int numVerticalLines, double horizontalSpace)
        {
            VerticalLines.Clear();
            VerticalValues.Clear();

            var spaces = Series.Count - 1;
            var leftShift = spaces switch
            {
                0 or 2 => _barWidth / 2,
                1 => 0,
                _ => _barWidth * ((spaces - 1) / 2.0)
            };

            var barGroupPositions = CalculateBarGroupPositions(horizontalSpace, numVerticalLines);

            for (var i = 0; i < numVerticalLines; i++)
            {
                var x = barGroupPositions.Length == 0 ? 0 : barGroupPositions[i];
                var line = new SvgPath()
                {
                    Index = i,
                    Data = $"M {ToS(x)} {ToS(_boundHeight - VerticalStartSpace)} L {ToS(x)} {ToS(VerticalEndSpace)}"
                };
                VerticalLines.Add(line);

                var xLabels = i < ChartLabels.Length ? ChartLabels[i] : "";
                var lineValue = new SvgText()
                {
                    X = x + (_barGroupWidth / 2) - ((_barGap * spaces) / 2) - leftShift,
                    Y = _boundHeight - 10,
                    Value = xLabels
                };
                VerticalValues.Add(lineValue);
            }
        }

        protected double[] CalculateBarGroupPositions(double availableSpace, int columnsPerDataSet)
        {
            var dataSetCount = Series.Count;

            if (dataSetCount == 0) return [];

            var context = new BarGroupContext
            {
                ColumnsPerDataSet = columnsPerDataSet,
                DataSetCount = dataSetCount,
                AvailableSpace = availableSpace,
                BarWidth = _barWidth,
                BarGap = _barGap,
                BarGroupWidth = _barGroupWidth,
                StartSpaceBuffer = HorizontalStartSpace,
                EndSpaceBuffer = HorizontalEndSpace,
                SeriesSpacingRatio = ChartOptions!.SeriesSpacingRatio,
                CalculateSpaceWidth = CalculateSpaceWidth
            };

            var strategy = BarGroupStrategyFactory.GetStrategy(ChartOptions.Justify);

            return strategy.CalculatePositions(context);
        }

        private int CalculateSpaceWidth(double availableSpace, int groupCount)
        {
            if (groupCount <= 1) return 0;

            var spaceCount = groupCount - 1;
            var remainingWidth = availableSpace - HorizontalStartSpace - HorizontalEndSpace - ((_barGroupWidth + (_barWidth / 2)) * groupCount);
            var spaceWidth = remainingWidth * ChartOptions!.SeriesSpacingRatio.EnsureRange(0.01, 1.0);
            var spaceBetweenGroups = spaceWidth / spaceCount;

            return (int)Math.Max(0, spaceBetweenGroups);
        }

        protected void ComputeBarDimensions(double tickWidth)
        {
            var seriesCount = Series.Count;

            var fixedWidth = ChartOptions?.FixedBarWidth;

            if (fixedWidth.HasValue)
            {
                _barWidth = fixedWidth.Value;
                _barGap = _barWidth * 0.25;
                _barGroupWidth = (seriesCount * _barWidth) + ((seriesCount - 1) * _barGap);
                return;
            }

            var groupWidthRatio = ChartOptions!.BarWidthRatio.EnsureRange(0.01, 1.0);
            var totalGapRatio = seriesCount > 1 ? ChartOptions!.BarSpacingRatio * (seriesCount - 1) : 1;
            var barWidthRelative = 1.0 / (seriesCount + totalGapRatio);
            var groupWidthRelative = tickWidth * groupWidthRatio;

            _barWidth = Math.Max(MinBarWidth, groupWidthRelative * barWidthRelative);
            _barGap = seriesCount > 1 ? groupWidthRelative * barWidthRelative * ChartOptions!.BarSpacingRatio : 0;
            _barGroupWidth = Math.Max(MinBarWidth * seriesCount - 2, groupWidthRelative - _barWidth);
        }

        protected void OnBarMouseOver(MouseEventArgs _, SvgPath bar)
        {
            _hoveredBar = bar;

            if (IsOverlayChart && ChartReference is IMudStateHasChanged chart)
                chart.StateHasChanged();
        }

        protected void OnBarMouseOut()
        {
            _hoveredBar = null;

            if (IsOverlayChart && ChartReference is IMudStateHasChanged chart)
                chart.StateHasChanged();
        }
    }
}

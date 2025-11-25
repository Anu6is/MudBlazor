using System.Numerics;
using MudBlazor.Charts.Base;

#nullable enable
namespace MudBlazor.Charts
{
    partial class HorizontalBar<T> : MudBarChartBase<T, HorizontalBarChartOptions> where T : struct, INumber<T>, IMinMaxValue<T>, IFormattable
    {
        protected override void OnInitialized()
        {
            ChartType = ChartType.HorizontalBar;
            base.OnInitialized();
        }

        protected override void GenerateBars(int lowestHorizontalLine, T gridXUnits, double horizontalSpace, double verticalSpace, int numVerticalLines)
        {
            _bars.Clear();

            var barGroupPositions = CalculateBarGroupPositions(verticalSpace, numVerticalLines);

            for (var i = 0; i < Series.Count; i++)
            {
                var series = Series[i];
                var data = series.Data;

                for (var j = 0; j < data.Values.Count && j < barGroupPositions.Length; j++)
                {
                    var dataValue = data.GetValue(j);

                    var groupStartY = barGroupPositions[j] - (_barGroupWidth / 2);
                    var gridValueY = groupStartY + (i * (_barWidth + _barGap)) + (_barWidth / 2);

                    var gridValueX = HorizontalStartSpace - (lowestHorizontalLine * horizontalSpace);
                    var barWidth = (double.CreateSaturating((dataValue / gridXUnits)) - lowestHorizontalLine) * horizontalSpace;
                    var gridValue = HorizontalStartSpace + double.CreateSaturating(barWidth);

                    var bar = new SvgPath()
                    {
                        Index = i,
                        Data = $"M {ToS(gridValueX)} {ToS(gridValueY)} L {ToS(gridValue)} {ToS(gridValueY)}",
                        LabelXValue = dataValue.ToString(series.TooltipXValueFormat, null),
                        LabelYValue = ChartLabels.Length > j ? ChartLabels[j] : string.Empty,
                        LabelY = gridValueY,
                        LabelX = dataValue <= T.Zero ? gridValueX : gridValue
                    };
                    _bars.Add(bar);
                }
            }
        }

        /// <summary>
        /// Overridden to handle the horizontal orientation.
        /// </summary>
        protected override void GeneratePlotArea(out T gridXUnits, out int lowestHorizontalLine, out int numHorizontalLines, out int numVerticalLines, out double horizontalSpace, out double verticalSpace)
        {
            SetBounds();
            ComputeUnitsAndNumberOfLines(out gridXUnits, out numHorizontalLines, out lowestHorizontalLine, out numVerticalLines);

            var horizontalLines = IsOverlayChart ? SharedData!.Value.HorizontalLineCount - 1 : numHorizontalLines;

            horizontalSpace = (_boundWidth - HorizontalStartSpace - HorizontalEndSpace) / Math.Max(1, horizontalLines);
            verticalSpace = _boundHeight - VerticalStartSpace - VerticalEndSpace;
            var tickHeight = verticalSpace / numVerticalLines;

            ComputeBarDimensions(tickHeight);
            GenerateValueAxisGridLines(numHorizontalLines, lowestHorizontalLine, gridXUnits, horizontalSpace);
            GenerateCategoryAxisGridLines(numVerticalLines, verticalSpace);
        }

        /// <summary>
        /// Overridden to handle the horizontal orientation.
        /// </summary>
        protected override void ComputeUnitsAndNumberOfLines(out T gridXUnits, out int numHorizontalLines, out int lowestHorizontalLine, out int numVerticalLines)
        {
            var xAxisTicks = ChartOptions?.XAxisTicks;
            if (xAxisTicks.HasValue && xAxisTicks.Value > 0)
                gridXUnits = T.CreateSaturating(xAxisTicks.Value);
            else
                gridXUnits = T.CreateSaturating(20);

            var allValues = Series.SelectMany(series => series.Data.Values);

            if (allValues.Any())
            {
                var minX = allValues.Min();
                var maxX = ChartOptions?.XAxisSuggestedMax is null
                    ? allValues.Max()
                    : T.Max(T.CreateSaturating(ChartOptions.XAxisSuggestedMax.Value), allValues.Max());

                lowestHorizontalLine = Math.Min((int)Math.Floor(double.CreateSaturating(minX / gridXUnits)), 0);
                var highestHorizontalLine = Math.Max((int)Math.Ceiling(double.CreateSaturating(maxX / gridXUnits)), 0);
                numHorizontalLines = highestHorizontalLine - lowestHorizontalLine + 1;

                var maxXTicks = ChartOptions?.MaxNumXAxisTicks ?? 20;

                while (numHorizontalLines > maxXTicks)
                {
                    gridXUnits *= T.CreateSaturating(2);
                    lowestHorizontalLine = Math.Min((int)Math.Floor(double.CreateSaturating(minX / gridXUnits)), 0);
                    highestHorizontalLine = Math.Max((int)Math.Ceiling(double.CreateSaturating(maxX / gridXUnits)), 0);

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

        /// <summary>
        /// Overridden to handle the horizontal orientation. This method generates the vertical grid lines for the X-axis.
        /// </summary>
        protected override void GenerateValueAxisGridLines(int numHorizontalLines, int lowestHorizontalLine, T gridXUnits, double horizontalSpace)
        {
            VerticalLines.Clear();
            VerticalValues.Clear();

            for (var i = 0; i < numHorizontalLines; i++)
            {
                var x = HorizontalStartSpace + (i * horizontalSpace);
                var line = new SvgPath()
                {
                    Index = i,
                    Data = $"M {ToS(x)} {ToS(VerticalStartSpace)} L {ToS(x)} {ToS(_boundHeight - VerticalEndSpace)}"
                };
                VerticalLines.Add(line);

                var startGridX = T.CreateSaturating(lowestHorizontalLine + i) * gridXUnits;
                var lineValue = new SvgText()
                {
                    X = x,
                    Y = _boundHeight - 10,
                    Value = BuildXAxisValueString(startGridX)
                };
                VerticalValues.Add(lineValue);
            }
        }

        /// <summary>
        /// Overridden to handle the horizontal orientation. This method generates the horizontal grid lines for the Y-axis.
        /// </summary>
        protected override void GenerateCategoryAxisGridLines(int numVerticalLines, double verticalSpace)
        {
            HorizontalLines.Clear();
            HorizontalValues.Clear();

            var spaces = Series.Count - 1;
            var topShift = spaces switch
            {
                0 or 2 => _barWidth / 2,
                1 => 0,
                _ => _barWidth * ((spaces - 1) / 2.0)
            };

            var barGroupPositions = CalculateBarGroupPositions(verticalSpace, numVerticalLines);

            for (var i = 0; i < numVerticalLines; i++)
            {
                var y = barGroupPositions.Length == 0 ? 0 : barGroupPositions[i];
                var line = new SvgPath()
                {
                    Index = i,
                    Data = $"M {ToS(HorizontalStartSpace)} {ToS(y)} L {ToS(_boundWidth - HorizontalEndSpace)} {ToS(y)}"
                };
                HorizontalLines.Add(line);

                var yLabels = i < ChartLabels.Length ? ChartLabels[i] : "";
                var lineValue = new SvgText()
                {
                    X = 10,
                    Y = y + (_barGroupWidth / 2) - ((_barGap * spaces) / 2) - topShift,
                    Value = yLabels
                };
                HorizontalValues.Add(lineValue);
            }
        }

        private string BuildXAxisValueString(T value)
        {
            var doubleValue = double.CreateSaturating(value);

            return ChartOptions?.XAxisToStringFunc is null
                ? ToS(doubleValue, ChartOptions?.XAxisFormat)
                : ChartOptions.XAxisToStringFunc(doubleValue);
        }
    }
}

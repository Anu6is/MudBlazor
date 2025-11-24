using System.Numerics;
using MudBlazor.Charts.Base;

#nullable enable
namespace MudBlazor.Charts
{
    /// <summary>
    /// Represents a chart which displays series values as rectangular bars.
    /// </summary>
    /// <seealso cref="Donut{T}"/>
    /// <seealso cref="Line{T}"/>
    /// <seealso cref="Pie{T}"/>
    /// <seealso cref="StackedBar{T}"/>
    /// <seealso cref="TimeSeries{T}"/>
    partial class Bar<T> : MudBarChartBase<T, BarChartOptions> where T : struct, INumber<T>, IMinMaxValue<T>, IFormattable
    {
        protected override void OnInitialized()
        {
            ChartType = ChartType.Bar;
            base.OnInitialized();
        }

        protected override void GenerateBars(int lowestHorizontalLine, T gridYUnits, double horizontalSpace, double verticalSpace, int numVerticalLines)
        {
            _bars.Clear();

            var barGroupPositions = CalculateBarGroupPositions(horizontalSpace, numVerticalLines);

            for (var i = 0; i < Series.Count; i++)
            {
                var series = Series[i];
                var data = series.Data;

                for (var j = 0; j < data.Values.Count && j < barGroupPositions.Length; j++)
                {
                    var dataValue = data.GetValue(j);

                    var groupStartX = barGroupPositions[j] - (_barGroupWidth / 2);
                    var gridValueX = groupStartX + (i * (_barWidth + _barGap)) + (_barWidth / 2);

                    var gridValueY = _boundHeight - VerticalStartSpace + (lowestHorizontalLine * verticalSpace);
                    var barHeight = (double.CreateSaturating((dataValue / gridYUnits)) - lowestHorizontalLine) * verticalSpace;
                    var gridValue = _boundHeight - VerticalStartSpace - double.CreateSaturating(barHeight);

                    var bar = new SvgPath()
                    {
                        Index = i,
                        Data = $"M {ToS(gridValueX)} {ToS(gridValueY)} L {ToS(gridValueX)} {ToS(gridValue)}",
                        LabelXValue = ChartLabels.Length > j ? ChartLabels[j] : string.Empty,
                        LabelYValue = dataValue.ToString(series.TooltipYValueFormat, null),
                        LabelX = gridValueX,
                        LabelY = dataValue <= T.Zero ? gridValueY : gridValue
                    };
                    _bars.Add(bar);
                }
            }
        }
    }
}

using System.Globalization;
using System.Numerics;
using System.Text;
using MudBlazor.Extensions;

namespace MudBlazor.Charts
{
    /// <summary>
    /// Represents a chart which displays values as a percentage of a circle.
    /// </summary>
    /// <seealso cref="Bar{T}"/>
    /// <seealso cref="Donut{T}"/>
    /// <seealso cref="Line{T}"/>
    /// <seealso cref="StackedBar{T}"/>
    /// <seealso cref="TimeSeries{T}"/>
    partial class Pie<T> : MudRadialChartBase<T, PieChartOptions> where T : struct, INumber<T>, IMinMaxValue<T>, IFormattable
    {
        protected override double Radius => NormalizedRadius;

        protected override string GetViewBox()
        {
            var startAngle = ChartOptions?.StartAngle ?? 270.0;
            var sweepAngle = ChartOptions?.SweepAngle ?? 360.0;

            return ComputeArcViewBox(startAngle, sweepAngle);
        }

        protected override void OnInitialized()
        {
            ChartType = ChartType.Pie;
            ChartOptions ??= new PieChartOptions();
            base.OnInitialized();
        }

        public override void RebuildChart()
        {
            _paths.Clear();
            _legends.Clear();

            SetBounds();

            var chartData = AggregateSeriesData(ChartOptions!.AggregationOption);
            var normalizedData = GetNormalizedData();
            var startAngle = ChartOptions?.StartAngle ?? 270.0;
            var sweepAngle = ChartOptions?.SweepAngle ?? 360.0;
            var cumulativeRadians = startAngle * Math.PI / 180.0;
            var chartLabels = GetChartLabels();

            for (var i = 0; i < normalizedData.Length; i++)
            {
                if (normalizedData[i] == 0.0)
                    continue;

                var data = normalizedData[i];
                var value = T.Max(T.Zero, chartData[i]);
                var segmentRadians = (sweepAngle * Math.PI / 180.0) * data;
                var half = segmentRadians / 2;

                var coords = GetSegmentCoordinates(cumulativeRadians, half, segmentRadians);
                cumulativeRadians += segmentRadians;

                var pathData = BuildSvgPath(coords, Radius, segmentRadians);

                var midAngle = cumulativeRadians - half;
                var (x, y) = GetLabelPosition(midAngle, Radius, segmentRadians);

                _paths.Add(new SvgPetal
                {
                    Index = i,
                    Data = pathData,
                    LabelX = x,
                    LabelY = y,
                    LabelXValue = ChartOptions!.ShowAsPercentage
                        ? $"{Math.Round(data * 100, 1).ToInvariantString()}%"
                        : value.ToString(null, CultureInfo.InvariantCulture),
                    LabelYValue = chartLabels.Length > i ? chartLabels[i] : string.Empty,
                    SegmentRadius = Radius,
                    AngleRadians = segmentRadians,
                    LabelOffset = 0.5,
                });
            }

            BuildLegends(chartLabels);
        }

        private static SegmentCoordinates GetSegmentCoordinates(double startAngle, double halfAngle, double fullAngle)
        {
            return new SegmentCoordinates
            {
                StartX = Math.Cos(startAngle),
                StartY = Math.Sin(startAngle),
                MidX = Math.Cos(startAngle + halfAngle),
                MidY = Math.Sin(startAngle + halfAngle),
                EndX = Math.Cos(startAngle + fullAngle),
                EndY = Math.Sin(startAngle + fullAngle),
                LargeArcFlag = fullAngle > Math.PI ? 1 : 0
            };
        }

        private static string BuildSvgPath(SegmentCoordinates c, double radius, double radians)
        {
            var sb = new StringBuilder();

            sb.Append($"M {ToS(c.StartX * radius)} {ToS(c.StartY * radius)} ");

            if (Math.Abs(radians - 2 * Math.PI) < 1e-6)
            {
                sb.Append($"A {ToS(radius)} {ToS(radius)} 0 0 1 {ToS(c.MidX * radius)} {ToS(c.MidY * radius)} ");
                sb.Append($"A {ToS(radius)} {ToS(radius)} 0 0 1 {ToS(c.EndX * radius)} {ToS(c.EndY * radius)} ");
            }
            else
            {
                sb.Append($"A {ToS(radius)} {ToS(radius)} 0 {c.LargeArcFlag} 1 {ToS(c.EndX * radius)} {ToS(c.EndY * radius)} ");
            }

            sb.Append("L 0 0 Z");

            return sb.ToString();
        }

        private static (double X, double Y) GetLabelPosition(double angle, double radius, double radians)
        {
            if (Math.Abs(radians - 2 * Math.PI) < 1e-6)
                return (0, 0);

            var r = radius * 0.5;
            return (Math.Cos(angle) * r, Math.Sin(angle) * r);
        }
    }
}

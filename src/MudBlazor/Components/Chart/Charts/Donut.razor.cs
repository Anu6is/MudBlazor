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
    /// <seealso cref="Pie{T}"/>
    /// <seealso cref="Line{T}"/>
    /// <seealso cref="StackedBar{T}"/>
    /// <seealso cref="TimeSeries{T}"/>
    public partial class Donut<T> : MudRadialChartBase<T, DonutChartOptions> where T : struct, INumber<T>, IMinMaxValue<T>, IFormattable
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
            ChartType = ChartType.Donut;
            ChartOptions ??= new DonutChartOptions();
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
            var donutRatio = ChartOptions!.DonutRingRatio.EnsureRange(0.1, 1);
            var chartLabels = GetChartLabels();

            for (var i = 0; i < normalizedData.Length; i++)
            {
                if (normalizedData[i] == 0.0) continue;

                var data = normalizedData[i];
                var actualValue = T.Max(T.Zero, chartData[i]);
                var radians = sweepAngle * Math.PI / 180.0 * data;
                var coords = GetSegmentCoordinates(cumulativeRadians, radians);
                cumulativeRadians += radians;

                var geometry = new PathGeometry { Coords = coords, OuterRadius = Radius, InnerRadius = Radius * (1 - donutRatio), Data = data, SegmentRadians = radians };

                var pathData = BuildSvgPath(geometry);
                var midAngle = cumulativeRadians - (radians / 2);
                var (x, y) = GetLabelPosition(midAngle, Radius, donutRatio, radians);

                _paths.Add(new SvgPath
                {
                    Index = i,
                    Data = pathData,
                    LabelX = x,
                    LabelY = y,
                    LabelXValue = ChartOptions!.ShowAsPercentage
                        ? $"{Math.Round(data * 100, 1).ToInvariantString()}%"
                        : actualValue.ToString(null, CultureInfo.InvariantCulture),
                    LabelYValue = chartLabels.Length > i ? chartLabels[i] : string.Empty
                });
            }

            BuildLegends(chartLabels);
        }

        private static SegmentCoordinates GetSegmentCoordinates(double startRadians, double segmentRadians)
        {
            var half = segmentRadians / 2;
            return new SegmentCoordinates
            {
                StartX = Math.Cos(startRadians),
                StartY = Math.Sin(startRadians),
                MidX = Math.Cos(startRadians + half),
                MidY = Math.Sin(startRadians + half),
                EndX = Math.Cos(startRadians + segmentRadians),
                EndY = Math.Sin(startRadians + segmentRadians)
            };
        }

        private static string BuildSvgPath(PathGeometry g)
        {
            var sb = new StringBuilder();
            var arcFlag = g.SegmentRadians > Math.PI ? 1 : 0;

            static double ToR(double value, double radius) => value * radius;

            sb.Append($"M {ToS(ToR(g.Coords.StartX, g.OuterRadius))} {ToS(ToR(g.Coords.StartY, g.OuterRadius))} ");

            if (Math.Abs(g.SegmentRadians - (2 * Math.PI)) < 1e-6)
            {
                sb.Append($"A {ToS(g.OuterRadius)} {ToS(g.OuterRadius)} 0 0 1 {ToS(ToR(g.Coords.MidX, g.OuterRadius))} {ToS(ToR(g.Coords.MidY, g.OuterRadius))} ");
                sb.Append($"A {ToS(g.OuterRadius)} {ToS(g.OuterRadius)} 0 0 1 {ToS(ToR(g.Coords.EndX, g.OuterRadius))} {ToS(ToR(g.Coords.EndY, g.OuterRadius))} ");
            }
            else
            {
                sb.Append($"A {ToS(g.OuterRadius)} {ToS(g.OuterRadius)} 0 {arcFlag} 1 {ToS(ToR(g.Coords.EndX, g.OuterRadius))} {ToS(ToR(g.Coords.EndY, g.OuterRadius))} ");
            }

            sb.Append($"L {ToS(ToR(g.Coords.EndX, g.InnerRadius))} {ToS(ToR(g.Coords.EndY, g.InnerRadius))} ");

            if (Math.Abs(g.SegmentRadians - (2 * Math.PI)) < 1e-6)
            {
                sb.Append($"A {ToS(g.InnerRadius)} {ToS(g.InnerRadius)} 0 0 0 {ToS(ToR(g.Coords.MidX, g.InnerRadius))} {ToS(ToR(g.Coords.MidY, g.InnerRadius))} ");
                sb.Append($"A {ToS(g.InnerRadius)} {ToS(g.InnerRadius)} 0 0 0 {ToS(ToR(g.Coords.StartX, g.InnerRadius))} {ToS(ToR(g.Coords.StartY, g.InnerRadius))} ");
            }
            else
            {
                sb.Append($"A {ToS(g.InnerRadius)} {ToS(g.InnerRadius)} 0 {arcFlag} 0 {ToS(ToR(g.Coords.StartX, g.InnerRadius))} {ToS(ToR(g.Coords.StartY, g.InnerRadius))} ");
            }

            sb.Append("Z");

            return sb.ToString();
        }

        private static (double X, double Y) GetLabelPosition(double angle, double outerRadius, double donutRatio, double radians)
        {
            if (donutRatio >= 1 && Math.Abs(radians - (2 * Math.PI)) < 1e-6)
                return (0, 0);

            var radius = outerRadius * (1 - (donutRatio / 2));
            return (Math.Cos(angle) * radius, Math.Sin(angle) * radius);
        }

        private readonly struct PathGeometry
        {
            public SegmentCoordinates Coords { get; init; }
            public double OuterRadius { get; init; }
            public double InnerRadius { get; init; }
            public double Data { get; init; }
            public double SegmentRadians { get; init; }
        }
    }
}

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Extensions;
using MudBlazor.Interfaces;

#nullable enable
namespace MudBlazor.Charts
{
    /// <summary>
    /// Represents a chart which displays series values as rectangular bars.
    /// </summary>
    /// <seealso cref="Donut"/>
    /// <seealso cref="Line"/>
    /// <seealso cref="Pie"/>
    /// <seealso cref="StackedBar"/>
    /// <seealso cref="TimeSeries"/>
    partial class HorizontalBar : MudAxisChartBase<HorizontalBarChartOptions>
    {
        public static new ChartType ChartType => ChartType.HorizontalBar;

        public override RenderFragment? OverlayContent { get; set; }

        private readonly List<SvgPath> _bars = [];
        private SvgPath? _hoveredBar;

        private double _barGroupHeight; // Changed from _barGroupWidth
        private double _barHeight; // Changed from _barWidth
        private double _barGap;

        private const double MinBarHeight = 6; // Changed from MinBarWidth

        protected override void OnInitialized()
        {
            ChartOptions ??= new HorizontalBarChartOptions();

            if (ChartReference is IMudAxisChart axisChart)
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

            Series = (ChartContainer != null && ChartReference is MudChart)
                ? ChartContainer.ChartSeries
                : ChartSeries;

            GeneratePlotArea(out var gridXUnits, out var lowestVerticalLine, out var numVerticalLines, out var numHorizontalLines, out var valueAxisSpacePerUnit, out var categoryAxisSpace); // Renamed for clarity

            if (!IsOverlayChart)
            {
                // If this is not an overlay chart, we generate the shared plot points if an overlay exists
                // For HorizontalBar, X-axis is value, Y-axis is category.
                // AxisGridData: lowestHorizontalLine refers to the 'bottom' of the chart visually, which is min X for HorizontalBar.
                // numHorizontalLines refers to lines parallel to X-axis, so these are category lines for HorizontalBar.
                // gridYUnits refers to step of Y-axis, which is step of X-axis (value axis) for HorizontalBar.
                SharedData = OverlayChart is IMudAxisChart ? new AxisGridData(lowestVerticalLine, numHorizontalLines, gridXUnits, _boundWidth, _boundHeight) : null;
            }
            else
            {
                // If this is an overlay chart, we use the shared plot points from the main chart
                var area = SharedData!.Value;

                // Apply shared data according to HorizontalBar's perspective
                lowestVerticalLine = SharedData.Value.LowestHorizontalLine; // Min X value
                numHorizontalLines = SharedData.Value.HorizontalLineCount; // Number of category lines
                gridXUnits = SharedData.Value.YAxisTicks; // Step of X-axis (value)

                _boundWidth = area.BoundWidth;
                _boundHeight = area.BoundHeight;
            }

            GenerateBars(lowestVerticalLine, gridXUnits, valueAxisSpacePerUnit, categoryAxisSpace, numHorizontalLines);
            GenerateLegends();

            if (OverlayChart is IMudAxisChart overlay)
            {
                overlay.SharedData = SharedData;
                overlay.RebuildChart();
                StateHasChanged();
            }
        }

        // X-axis is Value axis, Y-axis is Category axis
        private void GeneratePlotArea(out double gridXUnits, out int lowestValueLine, out int numValueLines, out int numCategoryLines, out double valueAxisSpacePerUnit, out double categoryAxisSpace)
        {
            SetBounds();
            // numValueLines = number of lines perpendicular to bars (X-axis grid)
            // numCategoryLines = number of lines parallel to bars (Y-axis grid)
            ComputeUnitsAndNumberOfLines(out gridXUnits, out numValueLines, out lowestValueLine, out numCategoryLines);

            // space for each value unit on X-axis
            valueAxisSpacePerUnit = (_boundWidth - HorizontalStartSpace - HorizontalEndSpace) / Math.Max(1, numValueLines -1);
            // total space for categories on Y-axis
            categoryAxisSpace = _boundHeight - VerticalStartSpace - VerticalEndSpace; 
            // height available for each category group
            var tickHeight = categoryAxisSpace / Math.Max(1, numCategoryLines); 

            ComputeBarDimensions(tickHeight);
            // Generates X-axis grid lines and labels (VerticalValue GridLines)
            GenerateValueGridLines(numValueLines, lowestValueLine, gridXUnits, valueAxisSpacePerUnit);
            // Generates Y-axis category lines and labels (HorizontalValue GridLines)
            GenerateCategoryGridLines(numCategoryLines, categoryAxisSpace);
        }

        // Computes units and number of lines for X (value) and Y (category) axes
        private void ComputeUnitsAndNumberOfLines(out double gridXUnits, out int numValueLines, out int lowestValueLine, out int numCategoryLines)
        {
            gridXUnits = ChartOptions?.XAxisTicks ?? 20; // Value step for X-axis
            if (gridXUnits <= 0)
                gridXUnits = 20;

            var allDataValues = Series.SelectMany(series => series.Data.Values);

            if (allDataValues.Any())
            {
                var minX = allDataValues.Min();
                var maxX = ChartOptions?.XAxisSuggestedMax is null
                    ? allDataValues.Max()
                    : Math.Max(ChartOptions.XAxisSuggestedMax.Value, allDataValues.Max());

                lowestValueLine = Math.Min((int)Math.Floor(minX / gridXUnits), 0); // Lowest value on X-axis (e.g., 0 or negative if data has it)
                var highestValueLine = Math.Max((int)Math.Ceiling(maxX / gridXUnits), 0); // Highest value on X-axis
                numValueLines = highestValueLine - lowestValueLine + 1; // Number of grid lines for X-axis

                var maxXAxisTicks = ChartOptions?.MaxNumXAxisTicks ?? 20;
                while (numValueLines > maxXAxisTicks)
                {
                    gridXUnits *= 2;
                    lowestValueLine = Math.Min((int)Math.Floor(minX / gridXUnits), 0);
                    highestValueLine = Math.Max((int)Math.Ceiling(maxX / gridXUnits), 0);
                    numValueLines = highestValueLine - lowestValueLine + 1;
                }

                numCategoryLines = ChartLabels.Length > 0 ? ChartLabels.Length : Series.Max(series => series.Data.Values.Length); // Number of categories on Y-axis
            }
            else
            {
                numValueLines = 1;
                lowestValueLine = 0;
                numCategoryLines = 1;
            }
        }

        // Generates Y-axis category lines and labels. (Visually horizontal lines, data-wise category separators)
        private void GenerateCategoryGridLines(int numCategoryLines, double categoryAxisSpace)
        {
            HorizontalLines.Clear(); // Stores category separator lines
            HorizontalValues.Clear(); // Stores Y-axis category labels

            var categoryPositions = CalculateBarGroupPositions(categoryAxisSpace, numCategoryLines);

            for (var i = 0; i < numCategoryLines; i++)
            {
                // Ensure categoryPositions are valid and provide a fallback if necessary
                var y = (categoryPositions.Length > i && numCategoryLines > 0) ? categoryPositions[i] - _barGroupHeight / 2 : VerticalStartSpace + (i * (categoryAxisSpace / Math.Max(1,numCategoryLines))) ;
                 if(ChartOptions.Justify != Justify.FlexStart) // Adjust for center alignment of label
                    y = (categoryPositions.Length > i && numCategoryLines > 0) ? categoryPositions[i] : VerticalStartSpace + (categoryAxisSpace / Math.Max(1,numCategoryLines)) * (i + 0.5);


                // This line represents the start of a category group, not the center.
                // Or it can be the center depending on CalculateBarGroupPositions.
                // For labels, we want them centered with the group.
                var line = new SvgPath()
                {
                    Index = i,
                    Data = $"M {ToS(HorizontalStartSpace)} {ToS(y)} L {ToS(_boundWidth - HorizontalEndSpace)} {ToS(y)}"
                };
                 // Option: Don't draw lines if user doesn't want them for categories
                if(ChartOptions.ShowCategoryLines)
                    HorizontalLines.Add(line);


                var yLabelText = i < ChartLabels.Length ? ChartLabels[i] : $"Category {i + 1}";
                var labelYPos = (categoryPositions.Length > i && numCategoryLines > 0) ? categoryPositions[i] : VerticalStartSpace + (categoryAxisSpace / Math.Max(1,numCategoryLines)) * (i + 0.5);
                if(ChartOptions.Justify == Justify.FlexStart && categoryPositions.Length > i)
                     labelYPos = categoryPositions[i]; // categoryPositions[i] is already the center for FlexStart


                var lineValue = new SvgText()
                {
                    X = HorizontalStartSpace - (ChartOptions.YAxisLabelSpacing), // Position Y-axis labels to the left
                    Y = labelYPos, // Center label text vertically within the category group
                    Value = yLabelText,
                    TextAnchor = "end" // Align text to the end (right before the axis line)
                };
                HorizontalValues.Add(lineValue);
            }
        }

        // Generates X-axis value grid lines and labels. (Visually vertical lines, data-wise value markers)
        private void GenerateValueGridLines(int numValueLines, int lowestValueLine, double gridXUnits, double valueAxisSpacePerUnit)
        {
            VerticalLines.Clear(); // Stores value marker lines
            VerticalValues.Clear(); // Stores X-axis value labels

            for (var i = 0; i < numValueLines; i++)
            {
                var x = HorizontalStartSpace + (i * valueAxisSpacePerUnit);
                var line = new SvgPath()
                {
                    Index = i,
                    Data = $"M {ToS(x)} {ToS(VerticalStartSpace)} L {ToS(x)} {ToS(_boundHeight - VerticalEndSpace)}"
                };
                VerticalLines.Add(line);

                var value = (lowestValueLine + i) * gridXUnits;
                var lineValue = new SvgText()
                {
                    X = x,
                    Y = _boundHeight - VerticalEndSpace + (ChartOptions.XAxisLabelSpacing), // Position X-axis labels below the axis
                    Value = ToS(value, ChartOptions?.XAxisFormat),
                    TextAnchor = "middle" // Center text below the tick mark
                };
                VerticalValues.Add(lineValue);
            }
        }

        // Generates the SVG paths for the bars.
        private void GenerateBars(int lowestValueLine, double gridXUnits, double valueAxisSpacePerUnit, double categoryAxisSpace, int numCategoryLines)
        {
            _bars.Clear();

            // Get positions for each category group along the Y-axis
            var categoryGroupPositionsY = CalculateBarGroupPositions(categoryAxisSpace, numCategoryLines);

            // Calculate the X-coordinate of the zero line (or the minimum value line)
            var zeroLineX = HorizontalStartSpace - (lowestValueLine * valueAxisSpacePerUnit);
            if (lowestValueLine > 0) // if all values are positive, lowestValueLine might be >0, zero line should be at HorizontalStartSpace
                zeroLineX = HorizontalStartSpace;


            for (var seriesIndex = 0; seriesIndex < Series.Count; seriesIndex++)
            {
                var series = Series[seriesIndex];
                var data = series.Data;

                for (var categoryIndex = 0; categoryIndex < data.Values.Length && categoryIndex < categoryGroupPositionsY.Length; categoryIndex++)
                {
                    var dataValue = data[categoryIndex];

                    // Y position for the center of the current bar group (category)
                    var groupCenterY = categoryGroupPositionsY[categoryIndex];
                    
                    // Calculate the Y position for the current bar within its group
                    // This considers multiple series in the same category.
                    var barCenterY = groupCenterY - (_barGroupHeight / 2) + (seriesIndex * (_barHeight + _barGap)) + (_barHeight / 2);

                    // Calculate the width of the bar based on its data value
                    var barActualWidth = (dataValue / gridXUnits) * valueAxisSpacePerUnit;

                    var barStartX = dataValue >= 0 ? zeroLineX : zeroLineX + barActualWidth; // barActualWidth is negative for negative values
                    var barEndX = dataValue >= 0 ? zeroLineX + barActualWidth : zeroLineX;


                    var bar = new SvgPath()
                    {
                        Index = seriesIndex,
                        Data = $"M {ToS(barStartX)} {ToS(barCenterY)} L {ToS(barEndX)} {ToS(barCenterY)}",
                        LabelXValue = dataValue.ToString(series.TooltipYValueFormat), // Value is on X-axis
                        LabelYValue = ChartLabels.Length > categoryIndex ? ChartLabels[categoryIndex] : string.Empty, // Category is on Y-axis
                        LabelX = barEndX, // Tooltip X at the end of the bar
                        LabelY = barCenterY // Tooltip Y at the vertical center of the bar
                    };
                    _bars.Add(bar);
                }
            }
        }

        // Calculates Y positions for each category group.
        private double[] CalculateBarGroupPositions(double categoryAxisSpace, int categoriesCount)
        {
            var seriesCount = Series.Count; // Number of series (bars) per category group

            if (categoriesCount == 0) return [];

            var positions = new double[categoriesCount];
            var totalEffectiveBarHeightPerGroup = (seriesCount * _barHeight) + (Math.Max(0, seriesCount - 1) * _barGap); // Total height taken by bars in one category

            switch (ChartOptions.Justify)
            {
                case Justify.FlexStart:
                    var currentY = VerticalStartSpace + ChartOptions.BarGroupGap + totalEffectiveBarHeightPerGroup / 2.0;
                    for (var i = 0; i < categoriesCount; i++)
                    {
                        positions[i] = currentY;
                        currentY += totalEffectiveBarHeightPerGroup + ChartOptions.BarGroupGap;
                    }
                    break;

                case Justify.FlexEnd:
                    var totalHeightNeeded = (categoriesCount * totalEffectiveBarHeightPerGroup) + (Math.Max(0, categoriesCount + 1) * ChartOptions.BarGroupGap);
                    currentY = _boundHeight - VerticalEndSpace - totalHeightNeeded + ChartOptions.BarGroupGap + totalEffectiveBarHeightPerGroup / 2.0;
                     for (var i = 0; i < categoriesCount; i++)
                    {
                        positions[i] = currentY;
                        currentY += totalEffectiveBarHeightPerGroup + ChartOptions.BarGroupGap;
                    }
                    break;

                case Justify.Center:
                    totalHeightNeeded = (categoriesCount * totalEffectiveBarHeightPerGroup) + (Math.Max(0, categoriesCount -1) * ChartOptions.BarGroupGap);
                    currentY = VerticalStartSpace + (categoryAxisSpace - totalHeightNeeded) / 2.0 + totalEffectiveBarHeightPerGroup / 2.0;
                    for (var i = 0; i < categoriesCount; i++)
                    {
                        positions[i] = currentY;
                        currentY += totalEffectiveBarHeightPerGroup + ChartOptions.BarGroupGap;
                    }
                    break;

                case Justify.SpaceBetween:
                    if (categoriesCount <= 1)
                    {
                        if (categoriesCount == 1) positions[0] = VerticalStartSpace + categoryAxisSpace / 2.0;
                        return positions;
                    }
                    var spaceBetween = (categoryAxisSpace - (categoriesCount * totalEffectiveBarHeightPerGroup)) / (categoriesCount - 1);
                    currentY = VerticalStartSpace + totalEffectiveBarHeightPerGroup / 2.0;
                    for (var i = 0; i < categoriesCount; i++)
                    {
                        positions[i] = currentY;
                        currentY += totalEffectiveBarHeightPerGroup + spaceBetween;
                    }
                    break;

                case Justify.SpaceAround:
                    var spaceAround = (categoryAxisSpace - (categoriesCount * totalEffectiveBarHeightPerGroup)) / categoriesCount;
                    currentY = VerticalStartSpace + spaceAround / 2.0 + totalEffectiveBarHeightPerGroup / 2.0;
                    for (var i = 0; i < categoriesCount; i++)
                    {
                        positions[i] = currentY;
                        currentY += totalEffectiveBarHeightPerGroup + spaceAround;
                    }
                    break;

                case Justify.SpaceEvenly:
                default: // Default to SpaceEvenly
                    var spaceEvenly = (categoryAxisSpace - (categoriesCount * totalEffectiveBarHeightPerGroup)) / (categoriesCount + 1);
                    currentY = VerticalStartSpace + spaceEvenly + totalEffectiveBarHeightPerGroup / 2.0;
                    for (var i = 0; i < categoriesCount; i++)
                    {
                        positions[i] = currentY;
                        currentY += totalEffectiveBarHeightPerGroup + spaceEvenly;
                    }
                    break;
            }
            return positions;
        }


        // This method is not directly used by CalculateBarGroupPositions anymore with the new Justify logic.
        // It might be useful if a different spacing strategy is adopted later.
        // For now, it's effectively replaced by ChartOptions.BarGroupGap.
        private int CalculateSpaceHeight(double verticalSpace, int groupCount)
        {
            if (groupCount <= 1) return 0; // No space needed for 0 or 1 group

            // This calculates space based on SeriesSpacingRatio, which might conflict with Justify logic.
            // The Justify logic in CalculateBarGroupPositions now uses ChartOptions.BarGroupGap for explicit spacing.
            var spaceCount = groupCount -1; // Number of gaps between groups
            var totalBarGroupStructureHeight = _barGroupHeight * groupCount; // Total height of all bar groups themselves
            var remainingHeight = verticalSpace - totalBarGroupStructureHeight; // Remaining space for gaps

            // Ensure SeriesSpacingRatio is within a sensible range if used.
            var effectiveRatio = ChartOptions!.SeriesSpacingRatio.EnsureRange(0.0, 1.0);
            
            // Distribute a portion of the remaining height according to the ratio.
            // This interpretation of SeriesSpacingRatio might not be what's intended for group spacing.
            // ChartOptions.BarGroupGap is likely more direct.
            var spaceToDistribute = remainingHeight * effectiveRatio; 
            var spaceBetweenGroups = spaceCount > 0 ? spaceToDistribute / spaceCount : 0;

            return (int)Math.Max(0, spaceBetweenGroups);
        }


        private void ComputeBarDimensions(double categoryTickHeight)
        {
            var seriesCount = Series.Count; // How many bars per category group

            var fixedHeight = ChartOptions?.FixedBarHeight;

            if (fixedHeight.HasValue && fixedHeight.Value > 0)
            {
                _barHeight = fixedHeight.Value; // Thickness of the bar
                // Gap is a ratio of the bar height, or a fixed value from options
                _barGap = ChartOptions.FixedBarGap ?? _barHeight * ChartOptions.BarSpacingRatio;
                _barGroupHeight = (seriesCount * _barHeight) + (Math.Max(0, seriesCount - 1) * _barGap);
                return;
            }

            // groupHeightRatio determines how much of the available category tick height is used by the bar group
            var groupHeightRatio = ChartOptions!.BarHeightRatio.EnsureRange(0.01, 1.0);
            var actualGroupHeight = categoryTickHeight * groupHeightRatio; // The total height allocated to the bar group for this category

            if (seriesCount == 0)
            {
                _barHeight = 0;
                _barGap = 0;
                _barGroupHeight = 0;
                return;
            }
            
            // From the actualGroupHeight, allocate space to bars and gaps based on BarSpacingRatio
            // Total parts = seriesCount (for bars) + (seriesCount - 1) * BarSpacingRatio (for gaps)
            var totalParts = seriesCount + (Math.Max(0, seriesCount - 1) * ChartOptions.BarSpacingRatio);
            
            _barHeight = actualGroupHeight / totalParts;
            _barGap = _barHeight * ChartOptions.BarSpacingRatio;

            // Ensure minimum bar height
            if (_barHeight < MinBarHeight)
            {
                // If calculated bar height is too small, try to adjust.
                // This might involve reducing gaps or scaling. For now, just cap it.
                // A more sophisticated approach might be needed if this is common.
                 var oldBarHeight = _barHeight;
                _barHeight = MinBarHeight;
                 // If we cap bar height, recalculate gap based on original ratio, but don't exceed group height.
                 _barGap = _barHeight * ChartOptions.BarSpacingRatio;
                 if ( (seriesCount * _barHeight) + (Math.Max(0, seriesCount - 1) * _barGap) > actualGroupHeight)
                 {
                    // If it exceeds, then we must shrink gaps, or bars, or both.
                    // Simplest: shrink gaps, potentially to 0.
                    _barGap = seriesCount > 1 ? (actualGroupHeight - seriesCount * _barHeight) / (seriesCount -1) : 0;
                    if(_barGap < 0) _barGap = 0; // Gap cannot be negative
                    _barHeight = (actualGroupHeight - Math.Max(0, seriesCount -1) * _barGap) / seriesCount; // re-calc bar height with new gap
                 }

            }
            
            _barGroupHeight = (seriesCount * _barHeight) + (Math.Max(0, seriesCount - 1) * _barGap);
            // Sanity check: _barGroupHeight should not exceed actualGroupHeight significantly due to MinBarHeight enforcement.
            // If it does, it means MinBarHeight * seriesCount + minimal gaps > actualGroupHeight.
            // In such a case, bars will overflow or overlap, which might be unavoidable if categoryTickHeight is too small.
        }
        private void OnBarMouseOver(MouseEventArgs _, SvgPath bar)
        {
            _hoveredBar = bar;

            if (IsOverlayChart && ChartReference is IMudStateHasChanged chart)
                chart.StateHasChanged();
        }

        private void OnBarMouseOut()
        {
            _hoveredBar = null;

            if (IsOverlayChart && ChartReference is IMudStateHasChanged chart)
                chart.StateHasChanged();
        }
    }
}

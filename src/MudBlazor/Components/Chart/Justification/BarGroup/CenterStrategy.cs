// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MudBlazor.Justification.BarGroup;

internal class CenterStrategy : IBarGroupPositionStrategy
{
    public double[] CalculatePositions(BarGroupContext ctx)
    {
        var positions = new double[ctx.ColumnsPerDataSet];

        var totalBarWidth = ctx.ColumnsPerDataSet * ctx.BarGroupWidth;
        var totalGapsWidth = (ctx.HorizontalSpace - totalBarWidth) * ctx.SeriesSpacingRatio;
        var gap = ctx.ColumnsPerDataSet > 1 ? totalGapsWidth / (ctx.ColumnsPerDataSet - 1) : 0;

        var totalWidth = totalBarWidth + totalGapsWidth;
        var start = ctx.HorizontalStartSpace + (ctx.HorizontalSpace - totalWidth) / 2.0;

        var currentPos = start + ctx.BarGroupWidth / 2.0;

        for (var i = 0; i < ctx.ColumnsPerDataSet; i++)
        {
            positions[i] = currentPos;
            currentPos += ctx.BarGroupWidth + gap;
        }

        return positions;
    }
}

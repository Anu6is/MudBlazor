// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MudBlazor.Charts;

/// <summary>
/// A comparer for chart data series.
/// </summary>
internal sealed class ChartDataSetComparer : IEqualityComparer<IChartSeries>
{
    /// <summary>
    /// The singleton instance of the comparer.
    /// </summary>
    public static readonly ChartDataSetComparer Instance = new();

    private ChartDataSetComparer() { }

    /// <summary>
    /// Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>True if the specified objects are equal; otherwise, false.</returns>
    public bool Equals(IChartSeries? x, IChartSeries? y)
        => x?.Name == y?.Name && x?.Visible == y?.Visible;

    /// <summary>
    /// Returns a hash code for the specified object.
    /// </summary>
    /// <param name="obj">The object for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    public int GetHashCode(IChartSeries obj)
        => obj.Name?.GetHashCode() ?? 0;
}

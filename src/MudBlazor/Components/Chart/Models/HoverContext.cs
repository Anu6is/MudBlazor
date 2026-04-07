// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace MudBlazor.Charts;

#nullable enable
/// <summary>
/// Represents the context of a hover interaction on a chart.
/// </summary>
public sealed class HoverContext
{
    /// <summary>
    /// The index of the hovered data point on the X-axis.
    /// </summary>
    public int XIndex { get; init; }

    /// <summary>
    /// The label of the hovered X-axis index.
    /// </summary>
    public string? XLabel { get; init; }

    /// <summary>
    /// The X-coordinate in pixels, potentially snapped to the index.
    /// </summary>
    public double PixelX { get; init; }

    /// <summary>
    /// The raw Y-coordinate of the pointer in pixels.
    /// </summary>
    public double PixelY { get; init; }

    /// <summary>
    /// The values of all series at the hovered index.
    /// </summary>
    public IReadOnlyList<SeriesHoverValue> Values { get; init; } = [];
}

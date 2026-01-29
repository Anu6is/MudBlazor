// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MudBlazor.Charts;

#nullable enable
/// <summary>
/// Represents the value of a single series at a hovered X-axis index.
/// </summary>
public sealed class SeriesHoverValue
{
    /// <summary>
    /// The name of the series.
    /// </summary>
    public string? SeriesName { get; init; }

    /// <summary>
    /// The data value.
    /// </summary>
    public double? Value { get; init; }

    /// <summary>
    /// The color of the series.
    /// </summary>
    public string? Color { get; init; }
}

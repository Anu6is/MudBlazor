// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MudBlazor;

#nullable enable
/// <summary>
/// Specifies how tooltips are displayed in the chart.
/// </summary>
public enum TooltipMode
{
    /// <summary>
    /// No tooltips are displayed.
    /// </summary>
    None,

    /// <summary>
    /// A single tooltip is displayed for the hovered data point.
    /// </summary>
    Single,

    /// <summary>
    /// A shared tooltip is displayed for all series at the hovered X-axis index.
    /// </summary>
    Shared
}

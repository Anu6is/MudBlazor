// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MudBlazor;

#nullable enable
/// <summary>
/// Specifies the visibility of crosshair lines in the chart.
/// </summary>
public enum CrosshairMode
{
    /// <summary>
    /// No crosshair lines are displayed.
    /// </summary>
    None,

    /// <summary>
    /// Only a vertical crosshair line is displayed.
    /// </summary>
    Vertical,

    /// <summary>
    /// Only a horizontal crosshair line is displayed.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Both vertical and horizontal crosshair lines are displayed.
    /// </summary>
    Both
}

// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using MudBlazor.Utilities;

namespace MudBlazor.Charts;

public partial class CrosshairRenderer : ComponentBase
{
    [Parameter] public HoverContext? HoverContext { get; set; }
    [Parameter] public CrosshairMode Mode { get; set; }
    [Parameter] public double BoundWidth { get; set; }
    [Parameter] public double BoundHeight { get; set; }
    [Parameter] public double HorizontalStartSpace { get; set; }
    [Parameter] public double HorizontalEndSpace { get; set; }
    [Parameter] public double VerticalStartSpace { get; set; }
    [Parameter] public double VerticalEndSpace { get; set; }
}

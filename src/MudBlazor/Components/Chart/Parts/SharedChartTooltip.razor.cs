// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using MudBlazor.Utilities;

namespace MudBlazor.Charts;

public partial class SharedChartTooltip : ComponentBase
{
    [Parameter] public HoverContext? HoverContext { get; set; }
    [Parameter] public double VerticalEndSpace { get; set; }
}

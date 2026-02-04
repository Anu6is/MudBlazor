// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Docs.Services;
using MudBlazor.Docs.Services.Notifications;

namespace MudBlazor.Docs.Shared;

public partial class AppbarButtons
{
    private IDictionary<NotificationMessage, bool> _messages = null;
    private bool _newNotificationsAvailable;

    [Inject]
    private INotificationService NotificationService { get; set; } = null!;

    [Inject]
    private LayoutService LayoutService { get; set; } = null!;

    /// <summary>
    /// Gets the text for the RTL toggle button, indicating the next state.
    /// </summary>
    public string RtlButtonText => LayoutService.IsRTL ? "Left-to-right" : "Right-to-left";

    /// <summary>
    /// Gets the icon for the RTL toggle button.
    /// </summary>
    public string RtlButtonIcon => LayoutService.IsRTL ? @Icons.Material.Filled.FormatTextdirectionLToR : @Icons.Material.Filled.FormatTextdirectionRToL;

    /// <summary>
    /// Gets the text for the color scheme toggle button, indicating the next mode.
    /// </summary>
    public string ColorSchemeButtonText => LayoutService.CurrentColorScheme switch
    {
        ColorScheme.Dark => "Auto mode",
        ColorScheme.Light => "Dark mode",
        _ => "Light mode"
    };

    /// <summary>
    /// Gets the icon for the color scheme toggle button.
    /// </summary>
    public string ColorSchemeButtonIcon => LayoutService.CurrentColorScheme switch
    {
        ColorScheme.Dark => Icons.Material.Rounded.AutoMode,
        ColorScheme.Light => Icons.Material.Outlined.DarkMode,
        _ => Icons.Material.Filled.LightMode
    };

    private async Task MarkNotificationAsReadAsync()
    {
        await NotificationService.MarkNotificationsAsRead();
        _newNotificationsAvailable = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _newNotificationsAvailable = await NotificationService.AreNewNotificationsAvailable();
            _messages = await NotificationService.GetNotifications();
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}

// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MudBlazor;
using MudBlazor.Docs.Models;
using MudBlazor.Docs.Services.UserPreferences;

namespace MudBlazor.Docs.Services;

public class LayoutService
{
    private readonly IUserPreferencesService _userPreferencesService;
    private UserPreferences.UserPreferences _userPreferences;
    private bool _systemDarkMode;

    /// <summary>
    /// Displays the layout right to left.
    /// </summary>
    public bool IsRTL { get; private set; }

    /// <summary>
    /// The user's preferred color scheme setting.
    /// </summary>
    public ColorScheme CurrentColorScheme { get; private set; }

    /// <summary>
    /// Dark mode is currently active.
    /// </summary>
    public bool IsDarkMode { get; private set; }

    /// <summary>
    /// The currently active MudBlazor theme.
    /// </summary>
    public MudTheme CurrentTheme { get; private set; }

    public LayoutService(IUserPreferencesService userPreferencesService)
    {
        _userPreferencesService = userPreferencesService;
    }

    /// <summary>
    /// Occurs when a change happens that requires a UI refresh.
    /// </summary>
    public event EventHandler MajorUpdateOccurred;

    private void OnMajorUpdateOccurred() => MajorUpdateOccurred?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Updates the dark mode state based on user preference and, optionally, the system's dark mode setting.
    /// </summary>
    /// <param name="systemMode">The current system dark mode setting. If <c>null</c>, the existing known system mode is used.</param>
    public void UpdateDarkModeState(bool? systemMode = null)
    {
        if (systemMode.HasValue)
        {
            _systemDarkMode = systemMode.Value;
        }

        IsDarkMode = CurrentColorScheme switch
        {
            ColorScheme.Dark => true,
            ColorScheme.Light => false,
            _ => _systemDarkMode,
        };
    }

    public async Task ApplyUserPreferencesAsync()
    {
        _userPreferences = await _userPreferencesService.LoadUserPreferences();

        if (_userPreferences is null)
        {
            _userPreferences = new()
            {
                RightToLeft = false,
                DarkLightTheme = ColorScheme.System,
            };
            await _userPreferencesService.SaveUserPreferences(_userPreferences);
        }
        else
        {
            IsRTL = _userPreferences.RightToLeft;
            CurrentColorScheme = _userPreferences.DarkLightTheme;
            UpdateDarkModeState();
        }
    }

    /// <summary>
    /// Handles changes in the system's dark mode setting.
    /// </summary>
    /// <param name="isSystemDarkMode"><c>true</c> if the system is in dark mode, otherwise <c>false</c>.</param>
    public Task OnSystemModeChangedAsync(bool isSystemDarkMode)
    {
        _systemDarkMode = isSystemDarkMode;
        UpdateDarkModeState();
        OnMajorUpdateOccurred();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Cycles through the available color scheme options (System, Light, Dark) and saves the new preference.
    /// </summary>
    public async Task CycleColorSchemeAsync()
    {
        CurrentColorScheme = CurrentColorScheme switch
        {
            ColorScheme.System => ColorScheme.Light,
            ColorScheme.Light => ColorScheme.Dark,
            ColorScheme.Dark => ColorScheme.System,
            _ => ColorScheme.System, // Default case, should not happen.
        };

        UpdateDarkModeState();

        _userPreferences.DarkLightTheme = CurrentColorScheme;
        await _userPreferencesService.SaveUserPreferences(_userPreferences);
        OnMajorUpdateOccurred();
    }

    /// <summary>
    /// Toggles the right-to-left (RTL) layout setting and saves the new preference.
    /// </summary>
    public async Task ToggleRightToLeftAsync()
    {
        IsRTL = !IsRTL;
        _userPreferences.RightToLeft = IsRTL;
        await _userPreferencesService.SaveUserPreferences(_userPreferences);
        OnMajorUpdateOccurred();
    }

    public void SetBaseTheme(MudTheme theme)
    {
        CurrentTheme = theme;
        OnMajorUpdateOccurred();
    }

    public DocsBasePage GetDocsBasePage(string uri)
    {
        if (uri.Contains("/docs/") || uri.Contains("/api/") || uri.Contains("/components/") ||
            uri.Contains("/features/") || uri.Contains("/customization/") || uri.Contains("/utilities/"))
        {
            return DocsBasePage.Docs;
        }

        if (uri.Contains("/getting-started/"))
        {
            return DocsBasePage.GettingStarted;
        }

        if (uri.Contains("/mud/"))
        {
            return DocsBasePage.DiscoverMore;
        }

        return DocsBasePage.None;
    }
}

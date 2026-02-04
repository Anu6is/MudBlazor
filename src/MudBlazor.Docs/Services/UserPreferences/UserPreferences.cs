// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using MudBlazor;

namespace MudBlazor.Docs.Services.UserPreferences
{
    public class UserPreferences
    {
        /// <summary>
        /// The direction of the layout.
        /// </summary>
        public bool RightToLeft { get; set; }

        /// <summary>
        /// The preferred dark mode configuration.
        /// </summary>
        public ColorScheme DarkLightTheme { get; set; }
    }
}

using System.Threading.Tasks;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor.UnitTests.Mocks;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class ColorSchemeTests : BunitTest
    {
        [Test]
        public void ColorScheme_Light_ForcesLightMode()
        {
            var comp = Context.Render<MudThemeProvider>(parameters => parameters
                .Add(p => p.ColorScheme, ColorScheme.Light));

            comp.Instance.ResolvedIsDarkMode.Should().BeFalse();
        }

        [Test]
        public void ColorScheme_Dark_ForcesDarkMode()
        {
            var comp = Context.Render<MudThemeProvider>(parameters => parameters
                .Add(p => p.ColorScheme, ColorScheme.Dark));

            comp.Instance.ResolvedIsDarkMode.Should().BeTrue();
        }

        [Test]
        public async Task ColorScheme_System_ResolvesFromJS()
        {
            Context.JSInterop.Setup<bool>("mudThemeProvider.isDarkMode").SetResult(true);

            var comp = Context.Render<MudThemeProvider>(parameters => parameters
                .Add(p => p.ColorScheme, ColorScheme.System));

            // Resolution happens after render
            await comp.InvokeAsync(() => { }); // trigger render/afterrender

            comp.Instance.ResolvedIsDarkMode.Should().BeTrue();
        }

        [Test]
        public async Task ColorScheme_Switching_UpdatesState()
        {
            var comp = Context.Render<MudThemeProvider>(parameters => parameters
                .Add(p => p.ColorScheme, ColorScheme.Light));

            comp.Instance.ResolvedIsDarkMode.Should().BeFalse();

            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.ColorScheme, ColorScheme.Dark));
            comp.Instance.ResolvedIsDarkMode.Should().BeTrue();
        }

        [Test]
        public async Task ResolvedIsDarkModeChanged_Fires()
        {
            bool? resolvedValue = null;
            var comp = Context.Render<MudThemeProvider>(parameters => parameters
                .Add(p => p.ColorScheme, ColorScheme.Light)
                .Add(p => p.ResolvedIsDarkModeChanged, EventCallback.Factory.Create<bool>(this, (bool val) => resolvedValue = val)));

            resolvedValue.Should().BeFalse();

            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.ColorScheme, ColorScheme.Dark));
            resolvedValue.Should().BeTrue();
        }

        [Test]
        public async Task ColorScheme_System_StartsWatching()
        {
            Context.JSInterop.SetupVoid("mudThemeProvider.watchDarkMode");

            var comp = Context.Render<MudThemeProvider>(parameters => parameters
                .Add(p => p.ColorScheme, ColorScheme.System));

            await comp.InvokeAsync(() => { }); // ensure afterrender

            Context.JSInterop.VerifyInvoke("mudThemeProvider.watchDarkMode", 1);
        }

        [Test]
        public async Task ColorScheme_Forced_StopsWatching()
        {
            Context.JSInterop.SetupVoid("mudThemeProvider.watchDarkMode");
            Context.JSInterop.SetupVoid("mudThemeProvider.stopWatchingDarkMode");

            var comp = Context.Render<MudThemeProvider>(parameters => parameters
                .Add(p => p.ColorScheme, ColorScheme.System));

            await comp.InvokeAsync(() => { });
            Context.JSInterop.VerifyInvoke("mudThemeProvider.watchDarkMode", 1);

            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.ColorScheme, ColorScheme.Dark));

            Context.JSInterop.VerifyInvoke("mudThemeProvider.stopWatchingDarkMode", 1);
        }
    }
}

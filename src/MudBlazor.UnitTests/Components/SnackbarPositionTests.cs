using AwesomeAssertions;
using Bunit;
using MudBlazor.UnitTests.TestComponents.Snackbar;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    [NonParallelizable]
    public class SnackbarPositionTests : BunitTest
    {
        private IRenderedComponent<MudSnackbarProvider> _provider;
        private ISnackbar _service;

        [SetUp]
        public void SnackbarSetUp()
        {
            _service = Context.Services.GetService<ISnackbar>();
            _provider = Context.Render<MudSnackbarProvider>();
        }

        [TearDown]
        public async Task SnackbarTearDown()
        {
            await _provider.InvokeAsync(() => _service.Clear());
        }

        [Test]
        public async Task PerSnackbarPosition()
        {
            _service.Configuration.PositionClass = Defaults.Classes.Position.TopRight;

            await _provider.InvokeAsync(() => _service.Add("Default position"));
            await _provider.InvokeAsync(() => _service.Add("Custom position", Severity.Info, config =>
            {
                config.PositionClass = Defaults.Classes.Position.BottomLeft;
            }));

            // We should now have two containers.
            var containers = _provider.FindAll(".mud-snackbar-container");
            containers.Count.Should().Be(2);

            var topRightContainer = containers.FirstOrDefault(c => c.ClassName.Contains(Defaults.Classes.Position.TopRight));
            topRightContainer.Should().NotBeNull();
            topRightContainer.InnerHtml.Should().Contain("Default position");

            var bottomLeftContainer = containers.FirstOrDefault(c => c.ClassName.Contains(Defaults.Classes.Position.BottomLeft));
            bottomLeftContainer.Should().NotBeNull();
            bottomLeftContainer.InnerHtml.Should().Contain("Custom position");
        }

        [Test]
        public async Task PerSnackbarPositionDynamicFallback()
        {
            _service.Configuration.PositionClass = Defaults.Classes.Position.TopRight;

            await _provider.InvokeAsync(() => _service.Add("Follows global"));

            var containers = _provider.FindAll(".mud-snackbar-container");
            containers.Count.Should().Be(1);
            containers[0].ClassName.Should().Contain(Defaults.Classes.Position.TopRight);

            // Change global position
            await _provider.InvokeAsync(() => _service.Configuration.PositionClass = Defaults.Classes.Position.BottomRight);

            containers = _provider.FindAll(".mud-snackbar-container");
            containers.Count.Should().Be(1);
            containers[0].ClassName.Should().Contain(Defaults.Classes.Position.BottomRight);
            containers[0].InnerHtml.Should().Contain("Follows global");
        }

        [Test]
        public async Task PerSnackbarPositionNoDynamicFallbackIfOverridden()
        {
            _service.Configuration.PositionClass = Defaults.Classes.Position.TopRight;

            await _provider.InvokeAsync(() => _service.Add("Overridden", Severity.Success, config =>
            {
                config.PositionClass = Defaults.Classes.Position.BottomLeft;
            }));

            var containers = _provider.FindAll(".mud-snackbar-container");
            containers.Count.Should().Be(1);
            containers[0].ClassName.Should().Contain(Defaults.Classes.Position.BottomLeft);

            // Change global position - should not affect the overridden snackbar
            await _provider.InvokeAsync(() => _service.Configuration.PositionClass = Defaults.Classes.Position.TopCenter);

            // We still have the BottomLeft container because the snackbar is still there
            containers = _provider.FindAll(".mud-snackbar-container");
            containers.Count.Should().Be(1);
            containers[0].ClassName.Should().Contain(Defaults.Classes.Position.BottomLeft);
        }
    }
}

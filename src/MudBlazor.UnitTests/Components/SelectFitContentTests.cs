using AwesomeAssertions;
using Bunit;
using MudBlazor.UnitTests.TestComponents.Select;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class SelectFitContentTests : BunitTest
    {
        [Test]
        public void SelectFitContent_Should_CalculateLongestItemOnInit()
        {
            // Initial render with FitContent=true
            var comp = Context.Render<SelectFitContentTest>(parameters => parameters.Add(x => x.FitContent, true));

            var filler = comp.Find(".mud-select-filler");
            filler.TextContent.Trim().Should().Be("Federated States of Micronesia");
        }

        [Test]
        public async Task SelectFitContent_Should_UpdateLongestItemWhenItemsChange()
        {
            var comp = Context.Render<MudSelect<string>>(parameters => parameters
                .Add(x => x.FitContent, true)
                .Add(x => x.ChildContent, builder =>
                {
                    builder.OpenComponent<MudSelectItem<string>>(0);
                    builder.AddAttribute(1, "Value", "Short");
                    builder.CloseComponent();
                }));

            var filler = comp.Find(".mud-select-filler");
            filler.TextContent.Trim().Should().Be("Short");

            // Add a longer item
            await comp.SetParametersAndRenderAsync(parameters => parameters
                .Add(x => x.ChildContent, builder =>
                {
                    builder.OpenComponent<MudSelectItem<string>>(0);
                    builder.AddAttribute(1, "Value", "Short");
                    builder.CloseComponent();
                    builder.OpenComponent<MudSelectItem<string>>(2);
                    builder.AddAttribute(3, "Value", "This is a much longer item");
                    builder.CloseComponent();
                }));

            await comp.WaitForAssertionAsync(() =>
            {
                filler = comp.Find(".mud-select-filler");
                filler.TextContent.Trim().Should().Be("This is a much longer item");
            });
        }
    }
}

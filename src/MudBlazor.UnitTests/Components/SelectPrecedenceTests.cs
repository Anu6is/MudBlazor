using AwesomeAssertions;
using Bunit;
using MudBlazor.UnitTests.TestComponents.Select;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using System;

#nullable enable

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class SelectPrecedenceTests : BunitTest
    {
        [Test]
        public async Task Select_ToStringFunc_ShouldTakePrecedenceOverChildContent()
        {
            var comp = Context.Render<SelectPrecedenceTest>();
            var select = comp.Instance.Select;

            // 1. Initially item1 is selected. ToStringFunc returns null for item1.
            // Should fall back to RenderFragment.
            var displaySlots = comp.FindAll("div.mud-input-slot");
            var displaySlot = displaySlots.FirstOrDefault(x => x.GetAttribute("style")?.Contains("display:inline") == true || x.GetAttribute("style")?.Contains("display: inline") == true);
            displaySlot.Should().NotBeNull("initially it should fall back to RenderFragment");
            displaySlot.InnerHtml.Should().Contain("custom-render");
            displaySlot.TextContent.Trim().Should().Be("Item 1 Rendered");

            // 2. Select item2. ToStringFunc returns "ITEM2" (not null).
            // Should use ToStringFunc and NOT RenderFragment.
            await comp.InvokeAsync(() => select!.SelectOption("item2"));
            comp.Render();

            displaySlots = comp.FindAll("div.mud-input-slot");
            displaySlot = displaySlots.FirstOrDefault(x => x.GetAttribute("style")?.Contains("display:inline") == true || x.GetAttribute("style")?.Contains("display: inline") == true);
            displaySlot.Should().BeNull("because ToStringFunc should take precedence over RenderFragment when it returns a non-null value");

            var input = comp.Find("input");
            input.GetAttribute("value").Should().Be("ITEM2");
        }

        [Test]
        public async Task Select_FitContent_ShouldPrioritizeToStringFunc()
        {
            var comp = Context.Render<SelectPrecedenceTest>();
            var selectComponent = comp.FindComponent<MudSelect<string>>();

            // Remove label to avoid it being the longest
            await selectComponent.SetParametersAndRenderAsync(parameters => parameters
                .Add(x => x.FitContent, true)
                .Add(x => x.Label, null)
                .Add(x => x.ToStringFunc, new Func<string, string?>(x => x == "item2" ? "VERY LONG ITEM 2" : null)));

            // item1 -> null -> "Item 1 Rendered" (15 chars)
            // item2 -> "VERY LONG ITEM 2" (16 chars)

            // item2 is longest. ToStringFunc is NOT null for item2.
            // filler should use "VERY LONG ITEM 2" and NOT RenderFragment.

            var filler = comp.Find(".mud-select-filler");
            filler.TextContent.Should().Contain("VERY LONG ITEM 2");
            filler.InnerHtml.Should().NotContain("custom-render");

            // Now make item1 longest via ToStringFunc
            await selectComponent.SetParametersAndRenderAsync(parameters => parameters.Add(x => x.ToStringFunc, new Func<string, string?>(x => x == "item1" ? "EXTREMELY LONG ITEM 1" : "ITEM 2")));

            // Trigger recalculation of _longestItem by toggling FitContent
            await selectComponent.SetParametersAndRenderAsync(parameters => parameters.Add(x => x.FitContent, false));
            await selectComponent.SetParametersAndRenderAsync(parameters => parameters.Add(x => x.FitContent, true));

            filler = comp.Find(".mud-select-filler");
            filler.TextContent.Should().Contain("EXTREMELY LONG ITEM 1");
            filler.InnerHtml.Should().NotContain("custom-render");
        }
    }
}

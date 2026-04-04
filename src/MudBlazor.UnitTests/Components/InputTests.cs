using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components;

#nullable enable
[TestFixture]
public class InputTests : BunitTest
{
    [Test]
    public async Task ReadOnlyShouldNotHaveClearButton()
    {
        var comp = Context.Render<MudInput<string>>(p => p
            .Add(x => x.Text, "some value")
            .Add(x => x.Clearable, true)
            .Add(x => x.ReadOnly, false));

        comp.FindAll(".mud-input-clear-button").Count.Should().Be(1);

        await comp.SetParametersAndRenderAsync(p => p.Add(x => x.ReadOnly, true)); //no clear button when readonly
        comp.FindAll(".mud-input-clear-button").Count.Should().Be(0);
    }

    [TestCase(InputSizing.Auto, "mud-input-sizing-auto")]
    [TestCase(InputSizing.Fixed, "mud-input-sizing-fixed")]
    public void InputSizingHasClass(InputSizing sizing, string expectedClass)
    {
        var comp = Context.Render<MudInput<string>>(parameters => parameters
            .Add(p => p.Sizing, sizing));

        comp.Find("div.mud-input").ClassList.Should().Contain(expectedClass);
    }

    [Test]
    public async Task Input_HandleMouseWheelAsync_ShouldBlurWhenDisabledAndNumberType()
    {
        var comp = Context.Render<MudInput<int>>(p => p
            .Add(x => x.InputType, InputType.Number)
            .Add(x => x.DisableMouseWheel, true));

        await comp.Find("input").WheelAsync(new WheelEventArgs());

        // Verify that blur was called on the element.
        // Since bUnit doesn't directly track JS interop calls unless we set up the mock,
        // we can check if the JS interop was invoked.
        Context.JSInterop.Invocations.Should().Contain(x => x.Identifier == "mudElementRef.blur");
    }

    [Test]
    public async Task Input_HandleMouseWheelAsync_ShouldNotBlurWhenNotDisabled()
    {
        var comp = Context.Render<MudInput<int>>(p => p
            .Add(x => x.InputType, InputType.Number)
            .Add(x => x.DisableMouseWheel, false));

        await comp.Find("input").WheelAsync(new WheelEventArgs());

        Context.JSInterop.Invocations.Should().NotContain(x => x.Identifier == "mudElementRef.blur");
    }

    [Test]
    public async Task Input_HandleMouseWheelAsync_ShouldNotBlurWhenNotNumberType()
    {
        var comp = Context.Render<MudInput<string>>(p => p
            .Add(x => x.InputType, InputType.Text)
            .Add(x => x.DisableMouseWheel, true));

        await comp.Find("input").WheelAsync(new WheelEventArgs());

        Context.JSInterop.Invocations.Should().NotContain(x => x.Identifier == "mudElementRef.blur");
    }
}

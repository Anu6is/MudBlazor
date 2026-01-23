using AwesomeAssertions;
using Bunit;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class LabelHoverTests : BunitTest
    {
        [Test]
        public void MudTextField_ShowLabelOnHover_ShouldApplyClass()
        {
            var comp = Context.Render<MudTextField<string>>(parameters => parameters
                .Add(x => x.Label, "Test Label")
                .Add(x => x.ShowLabelOnHover, true));

            var label = comp.Find("label.mud-input-label");
            label.ClassList.Should().Contain("mud-input-label-show-on-hover");
        }

        [Test]
        public void MudSelect_ShowLabelOnHover_ShouldApplyClass()
        {
            var comp = Context.Render<MudSelect<string>>(parameters => parameters
                .Add(x => x.Label, "Test Label")
                .Add(x => x.ShowLabelOnHover, true));

            var label = comp.Find("label.mud-input-label");
            label.ClassList.Should().Contain("mud-input-label-show-on-hover");
        }

        [Test]
        public void MudAutocomplete_ShowLabelOnHover_ShouldApplyClass()
        {
            var comp = Context.Render<MudAutocomplete<string>>(parameters => parameters
                .Add(x => x.Label, "Test Label")
                .Add(x => x.ShowLabelOnHover, true));

            var label = comp.Find("label.mud-input-label");
            label.ClassList.Should().Contain("mud-input-label-show-on-hover");
        }

        [Test]
        public void MudNumericField_ShowLabelOnHover_ShouldApplyClass()
        {
            var comp = Context.Render<MudNumericField<int>>(parameters => parameters
                .Add(x => x.Label, "Test Label")
                .Add(x => x.ShowLabelOnHover, true));

            var label = comp.Find("label.mud-input-label");
            label.ClassList.Should().Contain("mud-input-label-show-on-hover");
        }

        [Test]
        public void MudDatePicker_ShowLabelOnHover_ShouldApplyClass()
        {
            var comp = Context.Render<MudDatePicker>(parameters => parameters
                .Add(x => x.Label, "Test Label")
                .Add(x => x.ShowLabelOnHover, true));

            var label = comp.Find("label.mud-input-label");
            label.ClassList.Should().Contain("mud-input-label-show-on-hover");
        }

        [Test]
        public void MudDateRangePicker_ShowLabelOnHover_ShouldApplyClass()
        {
            var comp = Context.Render<MudDateRangePicker>(parameters => parameters
                .Add(x => x.Label, "Test Label")
                .Add(x => x.ShowLabelOnHover, true));

            var label = comp.Find("label.mud-input-label");
            label.ClassList.Should().Contain("mud-input-label-show-on-hover");
        }

        [Test]
        public void MudField_ShowLabelOnHover_ShouldApplyClass()
        {
            var comp = Context.Render<MudField>(parameters => parameters
                .Add(x => x.Label, "Test Label")
                .Add(x => x.ShowLabelOnHover, true));

            var label = comp.Find("label.mud-input-label");
            label.ClassList.Should().Contain("mud-input-label-show-on-hover");
        }
    }
}

using AwesomeAssertions;
using Bunit;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class CollapseHorizontalTests : BunitTest
    {
        [Test]
        public void Collapse_Horizontal_Direction_Classes()
        {
            var comp = Context.Render<MudCollapse>(parameters => parameters
                .Add(p => p.Direction, Direction.Right));

            comp.Find(".mud-collapse-container").ClassList.Should().Contain("mud-collapse-horizontal");
            comp.Find(".mud-collapse-container").ClassList.Should().Contain("mud-collapse-direction-right");
        }

        [Test]
        public void Collapse_Vertical_Direction_Classes()
        {
            var comp = Context.Render<MudCollapse>(parameters => parameters
                .Add(p => p.Direction, Direction.Top));

            comp.Find(".mud-collapse-container").ClassList.Should().NotContain("mud-collapse-horizontal");
            comp.Find(".mud-collapse-container").ClassList.Should().Contain("mud-collapse-direction-top");
        }

        [Test]
        public void Collapse_MaxWidth_Style()
        {
            var comp = Context.Render<MudCollapse>(parameters => parameters
                .Add(p => p.MaxWidth, 500));

            comp.Find(".mud-collapse-container").GetAttribute("style").Should().Contain("max-width:500px");
        }

        [Test]
        public void Collapse_IsHorizontal_Start_End()
        {
            var compStart = Context.Render<MudCollapse>(parameters => parameters
                .Add(p => p.Direction, Direction.Start));
            compStart.Find(".mud-collapse-container").ClassList.Should().Contain("mud-collapse-horizontal");

            var compEnd = Context.Render<MudCollapse>(parameters => parameters
                .Add(p => p.Direction, Direction.End));
            compEnd.Find(".mud-collapse-container").ClassList.Should().Contain("mud-collapse-horizontal");
        }

        [Test]
        public void Collapse_Both_Max_Styles()
        {
            var comp = Context.Render<MudCollapse>(parameters => parameters
                .Add(p => p.MaxHeight, 300)
                .Add(p => p.MaxWidth, 500));

            var style = comp.Find(".mud-collapse-container").GetAttribute("style");
            style.Should().Contain("max-height:300px");
            style.Should().Contain("max-width:500px");
        }
    }
}

using Microsoft.AspNetCore.Components;
using MudBlazor.State;
using MudBlazor.Utilities;

namespace MudBlazor
{
    /// <summary>
    /// A container for content which can be collapsed and expanded.
    /// </summary>
    /// <seealso cref="MudExpansionPanels"/>
    /// <seealso cref="MudExpansionPanel"/>
    public partial class MudCollapse : MudComponentBase
    {
        private enum CollapseState
        {
            Entering, Entered, Exiting, Exited
        }

        private readonly ParameterState<bool> _expandedState;
        private CollapseState _state = CollapseState.Exited;

        protected string Classname => new CssBuilder("mud-collapse-container")
            .AddClass("mud-collapse-entering", _state == CollapseState.Entering)
            .AddClass("mud-collapse-entered", _state == CollapseState.Entered)
            .AddClass("mud-collapse-exiting", _state == CollapseState.Exiting)
            .AddClass("invisible", _state == CollapseState.Exited)
            .AddClass("mud-collapse-horizontal", IsHorizontal)
            .AddClass($"mud-collapse-direction-{Direction.ToStringFast(true)}")
            .AddClass(Class)
            .Build();

        protected string Stylename => new StyleBuilder()
            .AddStyle("max-height", MaxHeight.ToPx(), MaxHeight != null)
            .AddStyle("max-width", MaxWidth.ToPx(), MaxWidth != null)
            .AddStyle(Style)
            .Build();

        private bool IsHorizontal => Direction is Direction.Left or Direction.Right or Direction.Start or Direction.End;

        /// <summary>
        /// Displays content within this panel.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Collapse.Behavior)]
        public bool Expanded { get; set; }

        /// <summary>
        /// The direction the panel expands.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Direction.Bottom"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Collapse.Appearance)]
        public Direction Direction { get; set; } = Direction.Bottom;

        /// <summary>
        /// The maximum allowed height of this panel, in pixels.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Collapse.Appearance)]
        public int? MaxHeight { get; set; }

        /// <summary>
        /// The maximum allowed width of this panel, in pixels.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Collapse.Appearance)]
        public int? MaxWidth { get; set; }

        /// <summary>
        /// The content within this panel.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Collapse.Behavior)]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Occurs when the collapse or expand animation has finished.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Collapse.Behavior)]
        public EventCallback OnAnimationEnd { get; set; }

        /// <summary>
        /// Occurs when the <see cref="Expanded"/> property has changed.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.Collapse.Behavior)]
        public EventCallback<bool> ExpandedChanged { get; set; }

        public MudCollapse()
        {
            using var register = CreateRegisterScope();
            _expandedState = register.RegisterParameter<bool>(nameof(Expanded))
                .WithParameter(() => Expanded)
                .WithEventCallback(() => ExpandedChanged)
                .WithChangeHandler(OnExpandedParameterChangedAsync);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender && _expandedState.Value)
            {
                _state = CollapseState.Entered;
                StateHasChanged();
            }
        }

        private Task OnExpandedParameterChangedAsync(ParameterChangedEventArgs<bool> args)
        {
            _state = args.Value ? CollapseState.Entering : CollapseState.Exiting;

            return Task.CompletedTask;
        }

        private Task AnimationEndAsync()
        {
            if (_state == CollapseState.Entering)
            {
                _state = CollapseState.Entered;
                StateHasChanged();
                return OnAnimationEnd.InvokeAsync(_expandedState.Value);
            }

            if (_state == CollapseState.Exiting)
            {
                _state = CollapseState.Exited;
                StateHasChanged();
                return OnAnimationEnd.InvokeAsync(_expandedState.Value);
            }

            return Task.CompletedTask;
        }
    }
}

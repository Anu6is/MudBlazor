//Copyright(c) Alessandro Ghidini.All rights reserved.
//Changes and improvements Copyright (c) The MudBlazor Team.

namespace MudBlazor
{
    public class SnackbarConfiguration : CommonSnackbarOptions
    {
        private bool _newestOnTop;
        private bool _preventDuplicates;
        private int _maxDisplayedSnackbars;
        private bool _clearAfterNavigation;

        internal event Action? OnUpdate;

        public bool NewestOnTop
        {
            get => _newestOnTop;
            set
            {
                _newestOnTop = value;
                OnUpdate?.Invoke();
            }
        }

        public bool PreventDuplicates
        {
            get => _preventDuplicates;
            set
            {
                _preventDuplicates = value;
                OnUpdate?.Invoke();
            }
        }

        public int MaxDisplayedSnackbars
        {
            get => _maxDisplayedSnackbars;
            set
            {
                _maxDisplayedSnackbars = value;
                OnUpdate?.Invoke();
            }
        }

        /// <summary>
        /// The CSS classes used to position the snackbar.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Defaults.Classes.Position.TopRight"/>.
        /// </remarks>
        public new string PositionClass
        {
            get => base.PositionClass ?? Defaults.Classes.Position.TopRight;
            set
            {
                base.PositionClass = value;
                OnUpdate?.Invoke();
            }
        }

        public bool ClearAfterNavigation
        {
            get => _clearAfterNavigation;
            set
            {
                _clearAfterNavigation = value;
                OnUpdate?.Invoke();
            }
        }

        public SnackbarConfiguration()
        {
            PositionClass = Defaults.Classes.Position.TopRight;
            NewestOnTop = false;
            PreventDuplicates = true;
            MaxDisplayedSnackbars = 5;
        }
    }
}

// Copyright (c) MudBlazor 2022
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using MudBlazor.Utilities;

namespace MudBlazor
{
    /// <summary>
    /// A field for numeric values from users. 
    /// </summary>
    /// <typeparam name="T">The type of number being collected.</typeparam>
    public partial class MudNumericField<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicMethods)] T> : MudDebouncedInput<T>
    {
        private T? _step;
        private T? _max;
        private T? _min;
        private bool _maxHasValue = false;
        private bool _minHasValue = false;
        private bool _stepHasValue = false;
        private MudInput<string> _elementReference = null!;
        private readonly string _elementId = Identifier.Create("numericField");
        private readonly INumericOperations<T> _ops = NumericOperationsFactory.Get<T>();

        [Inject]
        private IKeyInterceptorService KeyInterceptorService { get; set; } = null!;

        public MudNumericField()
        {
            Validation = new Func<T, Task<bool>>(ValidateInput);

            if (_ops.IsDecimal)
            {
                InputMode = InputMode.@decimal;
            }
        }

        protected string Classname =>
            new CssBuilder("mud-input-input-control mud-input-number-control")
                .AddClass(HideSpinButtons ? "mud-input-nospin" : "mud-input-showspin")
                .AddClass(Class)
                .Build();

        private bool IsNumberMode => InputMode == InputMode.numeric || InputMode == InputMode.@decimal;

        private bool IsFormatted => HasExplicitPattern || HasExplicitFormat || HasNonDefaultCulture;

        private bool HasExplicitPattern => Pattern is not null;

        private bool HasExplicitFormat => GetFormat() is not null;

        // <input type="number"> only handles InvariantCulture and CurrentUICulture correctly.
        // Any other culture requires falling back to <input type="text"> with explicit formatting.
        private bool HasNonDefaultCulture =>
            GetCulture() is { } culture &&
            !culture.Equals(CultureInfo.CurrentUICulture) &&
            !culture.Equals(CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns the effective pattern: the user-supplied <see cref="Pattern"/> when set,
        /// or a type-derived character-class default when rendering as <c>input[type=text]</c>.
        /// Returns <c>null</c> when in <c>input[type=number]</c> mode (no pattern needed).
        /// </summary>
        private string? GetEffectivePattern()
        {
            // In number mode the browser handles input natively — no pattern required.
            if (!IsFormatted)
            {
                return null;
            }

            // Consumer explicitly opted in to their own pattern.
            if (Pattern is not null)
            {
                return Pattern;
            }

            var numericType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            // Unsigned integers — only digits.
            if (!_ops.IsDecimal && _ops.Compare(_ops.MinValue, default!) >= 0)
            {
                return "[0-9]*";
            }

            // Signed integers — digits and a leading minus.
            if (!_ops.IsDecimal)
            {
                return @"[0-9\-]*";
            }

            // Floating-point — digits, minus, and the culture-specific decimal separator.
            // Regex.Escape handles '.' → '\.' so it isn't treated as a wildcard.
            var sep = Regex.Escape((GetCulture() ?? CultureInfo.InvariantCulture).NumberFormat.NumberDecimalSeparator);

            return $@"[0-9{sep}\-]*";
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override ValueTask FocusAsync()
        {
            return _elementReference.FocusAsync();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override ValueTask BlurAsync()
        {
            return _elementReference.BlurAsync();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override ValueTask SelectAsync()
        {
            return _elementReference.SelectAsync();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override ValueTask SelectRangeAsync(int pos1, int pos2)
        {
            return _elementReference.SelectRangeAsync(pos1, pos2);
        }

        /// <inheritdoc />
        protected override Task SetValueAndUpdateTextAsync(T? value, bool updateText = true, bool force = false)
        {
            (value, var valueChanged) = ConstrainBoundaries(value);
            return base.SetValueAndUpdateTextAsync(value, valueChanged || updateText, force);
        }

        /// <inheritdoc />
        protected internal override async Task OnBlurredAsync(FocusEventArgs obj)
        {
            await base.OnBlurredAsync(obj);
            await UpdateValuePropertyAsync(true); //Required to set the value after a blur before the debounce period has elapsed
            await UpdateTextPropertyAsync(false); //Required to update the string formatting after a blur before the debounce period has elapsed
        }

        protected async Task<bool> ValidateInput(T? value)
        {
            (value, var valueChanged) = ConstrainBoundaries(value);
            if (valueChanged)
                await SetValueAndUpdateTextAsync(value, true);
            return true; //Don't show errors
        }

        /// <summary>
        /// Shows a button to clear the value.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool Clearable { get; set; } = false;

        /// <summary>
        /// The icon of the clear button when <see cref="Clearable"/> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.Clear"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public string ClearIcon { get; set; } = Icons.Material.Filled.Clear;

        /// <summary>
        /// Decrements or increments depending on factor
        /// </summary>
        /// <param name="factor">Multiplication factor (1 or -1) will be applied to the step</param>
        private async Task Change(double factor = 1)
        {
            T? nextValue;
            try
            {
                nextValue = GetNextValue(factor);
            }
            catch (OverflowException)
            {
                // if next value overflows the primitive type, lets set it to Min or Max depending on if factor is positive or negative
                await SetValueAndUpdateTextAsync(factor > 0 ? Max : Min, true);
                return;
            }

            // Detect arithmetic wraparound (e.g. int.MaxValue + 1 → int.MinValue).
            if (ReadValue is not null && nextValue is not null)
            {
                if (factor > 0 && _ops.Compare(nextValue, ReadValue) < 0)
                {
                    nextValue = Max;
                }
                else if (factor < 0 && _ops.Compare(nextValue, ReadValue) > 0)
                {
                    nextValue = Min;
                }
            }

            await SetValueAndUpdateTextAsync(ConstrainBoundaries(nextValue).value);
            await _elementReference.SetText(ReadText);
        }

        private T? GetNextValue(double factor)
        {
            var value = ReadValue ?? _ops.Zero;
            var step = Step ?? _ops.One;
            if (value is null || step is null)
                return default;
            return _ops.Add(value, step, factor);
        }

        /// <summary>
        /// Increases the current value by <see cref="Step"/>.
        /// </summary>
        public Task Increment() => Change(factor: 1);

        /// <summary>
        /// Decreases the current value by <see cref="Step"/>.
        /// </summary>
        public Task Decrement() => Change(factor: -1);

        /// <summary>
        /// Checks if the value respects the boundaries set for this instance.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>Returns a valid value and if it has been changed.</returns>
        protected (T? value, bool changed) ConstrainBoundaries(T? value)
        {
            if (value is null)
            {
                return (default(T), false);
            }

            if (Max is not null && _ops.Compare(value, Max) > 0)
            {
                return (Max, true);
            }

            if (Min is not null && _ops.Compare(value, Min) < 0)
            {
                return (Min, true);
            }

            return (value, false);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var keyOptions = new List<KeyOptions>
                {
                    // prevent scrolling page, instead increment
                    new("ArrowUp", preventDown: "key+none"),
                    // prevent scrolling page, instead decrement
                    new("ArrowDown", preventDown: "key+none"),
                     // prevent dead keys like ^ ` ´ etc
                    new("Dead", preventDown: "key+any"),
                };

                var effectivePattern = GetEffectivePattern();
                if (effectivePattern != null)
                {
                    //prevent inputs that do not match the pattern
                    keyOptions.Add(new($"/^(?!{effectivePattern.TrimEnd('*')}).$/", preventDown: "key+none|key+shift|key+alt"));
                }

                var options = new KeyInterceptorOptions("mud-input-slot", keyOptions.ToArray());

                await KeyInterceptorService.SubscribeAsync(_elementId, options, keys => keys
                    .When(CanHandleKeys, builder => builder
                        .OnKeyDown("ArrowUp", Increment)
                        .OnKeyDown("ArrowDown", Decrement)));
            }

            await base.OnAfterRenderAsync(firstRender);

            if (!firstRender)
            {
                return;
            }

            // Overrides the browser's culture since <input type="number"> does not consider culture.
            // If a specific Culture, Pattern, or Format is defined, <input type="text"> will be used 
            // with the corresponding attributes applied.
            if (!IsFormatted)
            {
                await SetCultureAsync(CultureInfo.InvariantCulture);
            }
        }

        private bool CanHandleKeys() => !GetDisabledState() && !GetReadOnlyState();

        protected async Task HandleKeyDownAsync(KeyboardEventArgs obj)
        {
            await KeyInterceptorService.DispatchAsync(_elementId, KeyEventKind.Down, obj);
            await OnKeyDown.InvokeAsync(obj);
        }

        protected Task HandleKeyUpAsync(KeyboardEventArgs obj)
        {
            if (GetDisabledState() || GetReadOnlyState())
                return Task.CompletedTask;

            return OnKeyUp.InvokeAsync(obj);
        }

        protected async Task OnMouseWheelAsync(WheelEventArgs obj)
        {
            if (!obj.ShiftKey || GetDisabledState() || GetReadOnlyState())
                return;
            if (obj.DeltaY < 0)
            {
                if (InvertMouseWheel == false)
                    await Increment();
                else
                    await Decrement();
            }
            else if (obj.DeltaY > 0)
            {
                if (InvertMouseWheel == false)
                    await Decrement();
                else
                    await Increment();
            }
        }

        /// <summary>
        /// Reverses the mouse wheel direction.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  
        /// When <c>true</c>, moving the mouse wheel up will decrease the value, and down will increase the value.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public bool InvertMouseWheel { get; set; } = false;

        /// <summary>
        /// The minimum allowed value.
        /// </summary>
        /// <remarks>
        /// Defaults to the minimum value of the numeric type, such as <see cref="int.MinValue"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Validation)]
        public T? Min
        {
            get => _minHasValue ? _min : _ops.MinValue;
            set
            {
                _minHasValue = value is not null;
                _min = value;
            }
        }

        /// <summary>
        /// The maximum allowed value.
        /// </summary>
        /// <remarks>
        /// Defaults to the maximum value of the numeric type, such as <see cref="int.MaxValue"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Validation)]
        public T? Max
        {
            get => _maxHasValue ? _max : _ops.MaxValue;
            set
            {
                _maxHasValue = value is not null;
                _max = value;
            }
        }

        /// <summary>
        /// The amount added or subtracted when changing values.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>.  
        /// This affects changing values via spin buttons or the keyboard.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Behavior)]
        public T? Step
        {
            get => _stepHasValue ? _step : _ops.One;
            set
            {
                _stepHasValue = value is not null;
                _step = value;
            }
        }

        /// <summary>
        /// Hides the up and down buttons.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  When <c>false</c>, the user can still change values with the keyboard arrows and by typing values.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.FormComponent.Appearance)]
        public bool HideSpinButtons { get; set; }

        /// <summary>
        /// The type of value collected by this field.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="InputMode.numeric"/>.
        /// </remarks>
        [Parameter]
        public override InputMode InputMode { get; set; } = InputMode.numeric;

        /// <summary>
        /// The regular expression used to constrain values.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>, which will show a numerical keyboard on Safari.  Must be a valid JavaScript regular expression.  To allow only numbers (with no signs or commas), you can use <c>[0-9.]</c>.
        /// </remarks>
        [Parameter]
        public override string? Pattern { get; set; } = null;

        private string GetCounterText() => Counter switch
        {
            null => string.Empty,
            0 => string.IsNullOrEmpty(ReadText) ? "0" : $"{ReadText.Length}",
            _ => (string.IsNullOrEmpty(ReadText) ? "0" : $"{ReadText.Length}") + $" / {Counter}"
        };

        private Task OnInputValueChanged(string text)
        {
            return SetTextAndUpdateValueAsync(text);
        }

        //avoids the format to use scientific notation for large or small number in floating points types, while covering all options
        //https://stackoverflow.com/questions/1546113/double-to-string-conversion-without-scientific-notation
        private const string TagFormat = "0.###################################################################################################################################################################################################################################################################################################################################################";

        private static string? FormatParam(T? value)
        {
            if (value is IFormattable f)
                return f.ToString(TagFormat, CultureInfo.InvariantCulture.NumberFormat);
            return null;
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeAsyncCore()
        {
            await base.DisposeAsyncCore();

            if (IsJSRuntimeAvailable)
            {
                await KeyInterceptorService.UnsubscribeAsync(_elementId);
            }
        }
    }

    internal interface INumericOperations<T>
    {
        T Zero { get; }
        T MinValue { get; }
        T MaxValue { get; }
        T One { get; }
        bool IsDecimal { get; }

        /// <summary>
        /// Returns <c>a + (step * factor)</c>, or <c>null</c> if either operand is null.
        /// </summary>
        T Add(T a, T step, double factor);

        /// <summary>
        /// Compares two values. Consistent with <see cref="IComparable{T}"/>.
        /// Returns null-is-less-than-value semantics (matching the current Comparer behaviour).
        /// </summary>
        int Compare(T x, T y);

        double ToDouble(T value);
    }

    internal sealed class NumericOperations<TNumber> : INumericOperations<TNumber>
        where TNumber : struct, INumber<TNumber>, IMinMaxValue<TNumber>
    {
        public static readonly NumericOperations<TNumber> Instance = new();

        public TNumber Zero => TNumber.Zero;
        public TNumber MinValue => TNumber.MinValue;
        public TNumber MaxValue => TNumber.MaxValue;
        public TNumber One => TNumber.One;
        public bool IsDecimal => !TNumber.IsInteger(TNumber.Zero);

        public TNumber Add(TNumber a, TNumber step, double factor)
        {
            // CreateChecked throws OverflowException on out-of-range —
            // preserving the existing catch in Change() with no behaviour change.
            if (factor >= 0)
            {
                return checked(a + step * TNumber.CreateChecked(factor));
            }

            return checked(a - step * TNumber.CreateChecked(-factor));
        }

        public int Compare(TNumber x, TNumber y)
        {
            return x.CompareTo(y);
        }

        public double ToDouble(TNumber value) => double.CreateSaturating(value);
    }

    internal sealed class NullableNumericOperations<TNumber> : INumericOperations<TNumber?>
        where TNumber : struct, INumber<TNumber>, IMinMaxValue<TNumber>
    {
        public static readonly NullableNumericOperations<TNumber> Instance = new();

        private static readonly NumericOperations<TNumber> _inner = NumericOperations<TNumber>.Instance;

        public TNumber? Zero => _inner.Zero;
        public TNumber? MinValue => _inner.MinValue;
        public TNumber? MaxValue => _inner.MaxValue;
        public TNumber? One => _inner.One;
        public bool IsDecimal => _inner.IsDecimal;

        public TNumber? Add(TNumber? a, TNumber? step, double factor)
        {
            if (a is null || step is null)
                return null;
            return _inner.Add(a.Value, step.Value, factor);
        }

        public int Compare(TNumber? x, TNumber? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return _inner.Compare(x.Value, y.Value);
        }

        public double ToDouble(TNumber? value) => value is null ? 0.0 : _inner.ToDouble(value.Value);
    }

    internal static class NumericOperationsFactory
    {
        private static readonly ConcurrentDictionary<Type, object> _cache = new();

        /// <summary>
        /// Returns the <see cref="INumericOperations{T}"/> singleton for <typeparamref name="T"/>,
        /// which may be a non-nullable numeric type (e.g. <c>int</c>) or a nullable one (e.g. <c>int?</c>).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <typeparamref name="T"/> is not a supported numeric type.
        /// </exception>
        public static INumericOperations<T> Get<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicMethods)] T>()
        {
            return (INumericOperations<T>)_cache.GetOrAdd(typeof(T), static _ => CreateOperations<T>());
        }

        private static object CreateOperations<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicMethods)] T>()
        {
            var type = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(type);
            var isNullable = underlyingType is not null;
            var numericType = underlyingType ?? type;

            // Validate up front with a meaningful error rather than a cryptic reflection failure.
            var isSupported = numericType.IsValueType
                && typeof(INumber<>).MakeGenericType(numericType).IsAssignableFrom(numericType)
                && typeof(IMinMaxValue<>).MakeGenericType(numericType).IsAssignableFrom(numericType);

            if (!isSupported)
            {
                throw new InvalidOperationException(
                    $"MudNumericField does not support type '{type.Name}'. " +
                    $"T must be a numeric value type implementing INumber<T> and IMinMaxValue<T>, " +
                    $"or the nullable equivalent.");
            }

            var operationsType = isNullable
                ? typeof(NullableNumericOperations<>).MakeGenericType(numericType)
                : typeof(NumericOperations<>).MakeGenericType(numericType);

            // Each implementation exposes a static readonly Instance field.
            return operationsType.GetField("Instance",
                BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
        }
    }
}

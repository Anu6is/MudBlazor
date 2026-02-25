using System.Collections.Concurrent;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Maps CLR property types to their corresponding <see cref="ITypeFilterStrategy"/>
/// implementations.
/// </summary>
/// <remarks>
/// <para>
/// <b>AOT safety:</b> All strategy instances are pre-constructed in the static constructor
/// using <c>new ConcreteStrategy()</c> — no <c>Activator.CreateInstance</c>, no
/// <c>MakeGenericType</c>, no reflection-based instantiation.
/// </para>
/// <para>
/// <b>Nullable&lt;T&gt; promotion:</b> When <see cref="ResolveByType"/> is called with a
/// <c>Nullable&lt;T&gt;</c> type and <c>T</c> has a registered strategy, a
/// <see cref="NullableStrategy"/> wrapper is created on first access and cached permanently.
/// </para>
/// <para>
/// <b>Enum promotion:</b> When <see cref="ResolveByType"/> is called with any <c>enum</c>
/// type (or <c>Nullable&lt;TEnum&gt;</c>), <see cref="EnumStrategy.Instance"/> is returned
/// (wrapped in <see cref="NullableStrategy"/> for nullable enums).
/// </para>
/// </remarks>
internal static class TypeStrategyRegistry
{
    // ConcurrentDictionary so that lazy Nullable/Enum promotions from concurrent first-access
    // calls don't race. All initial registrations happen in the static constructor (single-threaded).
    private static readonly ConcurrentDictionary<Type, ITypeFilterStrategy> _strategies;

    static TypeStrategyRegistry()
    {
        _strategies = new ConcurrentDictionary<Type, ITypeFilterStrategy>
        {
            // ── String ────────────────────────────────────────────────────────
            [typeof(string)] = StringStrategy.Instance,

            // ── Integer types ─────────────────────────────────────────────────
            [typeof(byte)]   = ComparableStrategy<byte>.Instance,
            [typeof(sbyte)]  = ComparableStrategy<sbyte>.Instance,
            [typeof(short)]  = ComparableStrategy<short>.Instance,
            [typeof(ushort)] = ComparableStrategy<ushort>.Instance,
            [typeof(int)]    = ComparableStrategy<int>.Instance,
            [typeof(uint)]   = ComparableStrategy<uint>.Instance,
            [typeof(long)]   = ComparableStrategy<long>.Instance,
            [typeof(ulong)]  = ComparableStrategy<ulong>.Instance,

            // ── Floating-point / decimal ───────────────────────────────────────
            [typeof(float)]   = ComparableStrategy<float>.Instance,
            [typeof(double)]  = ComparableStrategy<double>.Instance,
            [typeof(decimal)] = ComparableStrategy<decimal>.Instance,

            // ── Date / time ────────────────────────────────────────────────────
            [typeof(DateTime)]       = DateStrategy.Instance,
            [typeof(DateTimeOffset)] = DateStrategy.Instance,
            [typeof(DateOnly)]       = DateStrategy.Instance,

            // ── Other scalars ──────────────────────────────────────────────────
            [typeof(bool)] = BoolStrategy.Instance,
            [typeof(Guid)] = GuidStrategy.Instance,
        };
    }

    // ── Lookup ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the <see cref="ITypeFilterStrategy"/> for <paramref name="propertyType"/>,
    /// or <see langword="null"/> if no strategy is registered for that type.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <c>Nullable&lt;T&gt;</c> types are promoted automatically: if <c>T</c> has a
    ///       registered strategy, a <see cref="NullableStrategy"/> wrapper is returned and cached.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <c>enum</c> types return <see cref="EnumStrategy.Instance"/> (wrapped in
    ///       <see cref="NullableStrategy"/> for nullable enums); they do not require
    ///       individual registration.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    public static ITypeFilterStrategy? ResolveByType(Type propertyType)
    {
        // Fast path: exact match.
        if (_strategies.TryGetValue(propertyType, out var strategy))
            return strategy;

        // Nullable<T> promotion.
        var underlying = Nullable.GetUnderlyingType(propertyType);
        if (underlying is not null)
        {
            ITypeFilterStrategy inner;

            if (underlying.IsEnum)
            {
                inner = EnumStrategy.Instance;
            }
            else if (!_strategies.TryGetValue(underlying, out inner!))
            {
                return null;
            }

            var wrapped = new NullableStrategy(inner);
            _strategies.TryAdd(propertyType, wrapped);
            return wrapped;
        }

        // Enum promotion (non-nullable).
        if (propertyType.IsEnum)
        {
            _strategies.TryAdd(propertyType, EnumStrategy.Instance);
            return EnumStrategy.Instance;
        }

        return null;
    }
}

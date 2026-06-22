namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Metadata descriptor for a single filter operator.
/// Bridges the public <see cref="FilterOperator"/> enum value to an operator key string,
/// a <see cref="FilterValueRequirement"/>, and the set of property types the operator
/// supports.
/// </summary>
/// <remarks>
/// <para>
/// The enum value is kept on the public <see cref="GridFilter"/> contract so that
/// <see cref="GridSnapshot"/> serialises with <c>System.Text.Json</c> defaults.
/// <see cref="FilterOperatorDescriptor"/> is an adapter-internal concept only —
/// it is never exposed through the grid's public API.
/// </para>
/// <para>
/// <see cref="Key"/> is a stable, human-readable string (e.g. <c>"string.contains"</c>)
/// used for remote DSL mapping and as part of the <see cref="FilterCacheKey"/>.
/// It is intentionally decoupled from the enum member name so that remote providers
/// can map it directly without caring about C# naming conventions.
/// </para>
/// <para>
/// <see cref="SupportedTypes"/> drives the filter UI — the operator dropdown for a
/// column can be populated by querying <see cref="BuiltInOperators.For(Type)"/> without
/// any hardcoded type-to-operator lists in rendering code.
/// </para>
/// </remarks>
internal sealed class FilterOperatorDescriptor
{
    /// <summary>Gets the public enum value this descriptor represents.</summary>
    public FilterOperator EnumValue { get; }

    /// <summary>
    /// Gets the stable operator key used for DSL mapping and cache keying.
    /// Format: <c>"category.operation"</c> e.g. <c>"string.contains"</c>, <c>"date.before"</c>.
    /// </summary>
    public string Key { get; }

    /// <summary>Gets whether a non-null filter value is required to apply this operator.</summary>
    public FilterValueRequirement ValueRequirement { get; }

    /// <summary>
    /// Gets the property types this operator applies to.
    /// Empty means the operator applies to all types (e.g. <c>IsNull</c>).
    /// </summary>
    public IReadOnlyList<Type> SupportedTypes { get; }

    internal FilterOperatorDescriptor(FilterOperator enumValue, string key, FilterValueRequirement valueRequirement, params Type[] supportedTypes)
    {
        EnumValue = enumValue;
        Key = key;
        ValueRequirement = valueRequirement;
        SupportedTypes = supportedTypes;
    }

    /// <summary>Returns <see langword="true"/> when this operator supports <paramref name="type"/>.</summary>
    public bool Supports(Type type)
    {
        if (SupportedTypes.Count == 0) return true;
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return SupportedTypes.Any(t => t == underlying || (t.IsGenericTypeDefinition && underlying.IsGenericType && underlying.GetGenericTypeDefinition() == t));
    }
}

// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace MudBlazor.Components.QuickGrid;

public sealed class DefaultGridItemCloner<T> : IGridItemCloner<T>
{
    private readonly JsonTypeInfo<T> _typeInfo;

    /// <summary>
    /// Creates a JSON-based deep cloner using source-generated metadata.
    /// </summary>
    /// <param name="typeInfo">
    /// The <see cref="JsonTypeInfo{T}"/> describing <typeparamref name="T"/>.
    /// Must come from a source-generated <see cref="JsonSerializerContext"/>.
    /// </param>
    public DefaultGridItemCloner(JsonTypeInfo<T> typeInfo)
    {
        _typeInfo = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <typeparamref name="T"/> cannot be round-tripped through
    /// <see cref="JsonSerializer"/>.
    /// </exception>
    public T Clone(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var json = JsonSerializer.Serialize(item, _typeInfo);
        var clone = JsonSerializer.Deserialize(json, _typeInfo);

        return clone ?? throw new InvalidOperationException(
            $"JSON round-trip produced null for type '{typeof(T).FullName}'. " +
            "Register a custom IGridItemCloner<T> via DI for types that require custom clone logic.");
    }
}

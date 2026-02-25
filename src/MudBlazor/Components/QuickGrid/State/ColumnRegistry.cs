using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Maintains the ordered collection of <see cref="ColumnDescriptor{T}"/> instances
/// registered by child column components during their Blazor lifecycle.
/// </summary>
/// <remarks>
/// <para>
/// <c>MudQuickGrid&lt;T&gt;</c> creates a single <see cref="ColumnRegistry{T}"/> and
/// cascades it alongside <c>QuickGridState&lt;T&gt;</c> using
/// <c>CascadingValue IsFixed="true"</c>.  Column components call
/// <see cref="Register"/> in their <c>OnInitialized</c> and
/// <see cref="Unregister"/> in their <c>Dispose</c>.
/// </para>
/// <para>
/// After all columns have registered, the grid calls
/// <see cref="IColumnRegistryAware{T}.Initialize"/> on the data source so that adapters
/// can compile delegate caches or expression caches once before query time.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
internal sealed class ColumnRegistry<T>
{
    private readonly Dictionary<string, ColumnDescriptor<T>> _byKey = new(StringComparer.Ordinal);

    // Preserves markup declaration order for column-loop rendering.
    private readonly List<string> _order = [];

    /// <summary>
    /// Registers <paramref name="descriptor"/> and compiles its
    /// <see cref="ColumnDescriptor{T}.ValueSelector"/> from
    /// <see cref="ColumnDescriptor{T}.PropertyExpression"/> if not already provided.
    /// </summary>
    /// <param name="descriptor">The descriptor to register.  Must have a unique <see cref="ColumnDescriptor{T}.Key"/>.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a descriptor with the same key is already registered.
    /// </exception>
    public void Register(ColumnDescriptor<T> descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (_byKey.ContainsKey(descriptor.Key))
            throw new InvalidOperationException(
                $"A column with key '{descriptor.Key}' is already registered. "
                + "Ensure each column has a unique, stable Key.");

        // Compile ValueSelector once at registration time if the caller did not
        // pre-compile it. This is the only place in the grid where expression
        // compilation occurs — never inside the hot query path.
        var compiled = descriptor with
        {
            ValueSelector = descriptor.ValueSelector ?? CompileSelector(descriptor.PropertyExpression),
        };

        _byKey[descriptor.Key] = compiled;
        _order.Add(descriptor.Key);
    }

    /// <summary>Removes the descriptor with the given key.  No-op if not registered.</summary>
    public void Unregister(string key)
    {
        if (_byKey.Remove(key))
            _order.Remove(key);
    }

    /// <summary>Returns the descriptor for <paramref name="key"/>, or <see langword="null"/> if not found.</summary>
    public ColumnDescriptor<T>? TryGet(string key)
        => _byKey.GetValueOrDefault(key);

    /// <summary>Returns descriptors in markup declaration order.</summary>
    public IEnumerable<ColumnDescriptor<T>> All
        => _order.Select(k => _byKey[k]);

    /// <summary>Returns the number of registered columns.</summary>
    public int Count => _order.Count;

    // ── Private helpers ───────────────────────────────────────────────────────

    private static Func<T, object?>? CompileSelector(LambdaExpression? expression)
    {
        if (expression is null)
            return null;

        // Rewrite the lambda to return object? so we get a single
        // Func<T, object?> regardless of the original TValue.
        var param = Expression.Parameter(typeof(T), "x");
        var body = new ParameterReplacer(expression.Parameters[0], param)
            .Visit(expression.Body);

        var boxed = expression.ReturnType.IsValueType
            ? Expression.Convert(body, typeof(object))
            : body;

        return Expression.Lambda<Func<T, object?>>(boxed, param).Compile();
    }

    private sealed class ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}

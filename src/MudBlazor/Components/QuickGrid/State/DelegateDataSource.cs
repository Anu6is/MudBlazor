namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// An <see cref="IGridDataSource{T}"/> adapter that wraps a user-provided asynchronous
/// delegate, for remote API, gRPC, OData, or CQRS integrations.
/// </summary>
/// <remarks>
/// <para>
/// The delegate receives the raw <see cref="GridQuery"/> and is fully responsible for
/// translating it into the appropriate remote call.  The grid performs no
/// sorting, filtering, or expression compilation.
/// </para>
/// <para>
/// <b>Usage:</b>
/// <code>
/// var source = new DelegateDataSource&lt;PersonDto&gt;(async (query, ct) =>
/// {
///     var response = await httpClient.GetFromJsonAsync&lt;PersonPageResponse&gt;(
///         $"/api/people?offset={query.Offset}&amp;limit={query.Limit}",
///         cancellationToken: ct);
///
///     return new GridDataPage&lt;PersonDto&gt;
///     {
///         Items = response!.Items,
///         TotalItemCount = response.TotalCount,
///     };
/// });
/// </code>
/// </para>
/// <para>
/// Because no expression compilation occurs in the adapter, <c>DelegateDataSource</c>
/// is fully NativeAOT-safe.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class DelegateDataSource<T> : IGridDataSource<T>
{
    private readonly Func<GridQuery, CancellationToken, ValueTask<GridDataPage<T>>> _delegate;

    /// <summary>
    /// Initialises the adapter wrapping <paramref name="queryDelegate"/>.
    /// </summary>
    /// <param name="queryDelegate">
    /// The delegate that executes the query.  Must not be <see langword="null"/>.
    /// </param>
    public DelegateDataSource(Func<GridQuery, CancellationToken, ValueTask<GridDataPage<T>>> queryDelegate)
    {
        ArgumentNullException.ThrowIfNull(queryDelegate);
        _delegate = queryDelegate;
    }

    /// <summary>
    /// Convenience constructor accepting a delegate that does not observe cancellation.
    /// </summary>
    public DelegateDataSource(Func<GridQuery, ValueTask<GridDataPage<T>>> queryDelegate)
    {
        ArgumentNullException.ThrowIfNull(queryDelegate);
        _delegate = (query, _) => queryDelegate(query);
    }

    /// <inheritdoc/>
    public ValueTask<GridDataPage<T>> QueryAsync(GridQuery query, CancellationToken cancellationToken = default)
        => _delegate(query, cancellationToken);
}

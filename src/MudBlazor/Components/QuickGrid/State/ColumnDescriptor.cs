using System.Linq.Expressions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Captures the metadata a registered column exposes to <see cref="IGridDataSource{T}"/>
/// adapter implementations.
/// </summary>
/// <remarks>
/// <para>
/// Column Blazor components (<c>PropertyColumn&lt;T, TValue&gt;</c>, etc.) create and
/// register a <see cref="ColumnDescriptor{T}"/> when they initialise.  Adapters receive
/// the populated <see cref="ColumnRegistry{T}"/> via <see cref="IColumnRegistryAware{T}"/>
/// after all columns have registered.
/// </para>
/// <para>
/// <see cref="PropertyExpression"/> and <see cref="ValueSelector"/> are optional —
/// <c>TemplateColumn</c> columns have neither and are not sortable or filterable
/// through the automatic adapter paths.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed record ColumnDescriptor<T>
{
    /// <summary>
    /// Gets the stable column key.  Must match <see cref="GridSort.ColumnKey"/>,
    /// <see cref="GridFilter.ColumnKey"/>, and <see cref="GridGroupBy.ColumnKey"/>.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>Gets the human-readable column title.</summary>
    public string Title { get; init; } = "";

    /// <summary>Gets whether this column supports sorting.</summary>
    public bool Sortable { get; init; }

    /// <summary>Gets whether this column supports filtering.</summary>
    public bool Filterable { get; init; }

    /// <summary>
    /// Gets whether this column supports grouping.
    /// </summary>
    public bool Groupable { get; init; }

    /// <summary>
    /// Gets the runtime <see cref="Type"/> of the column's property value.
    /// Used by <see cref="IGridDataSource{T}"/> adapters to parse
    /// <see cref="GridFilter.Value"/> strings into the correct type before
    /// building predicates.
    /// <see langword="null"/> for template columns with no property expression.
    /// </summary>
    public Type? PropertyType { get; init; }

    /// <summary>
    /// Gets the non-generic property selector expression (<c>Expression&lt;Func&lt;T, TValue&gt;&gt;</c>
    /// stored as <see cref="LambdaExpression"/>).
    /// Used by <c>QueryableDataSource&lt;T&gt;</c> to compose EF Core-translatable
    /// sort and filter expressions.
    /// <see langword="null"/> for template columns.
    /// </summary>
    public LambdaExpression? PropertyExpression { get; init; }

    /// <summary>
    /// Gets a pre-compiled value accessor delegate.
    /// Compiled <em>once</em> at column registration time by
    /// <c>ColumnRegistry&lt;T&gt;</c> from <see cref="PropertyExpression"/>.
    /// Used by <c>InMemoryDataSource&lt;T&gt;</c> in its hot query path with
    /// no per-query compilation cost.
    /// <see langword="null"/> when <see cref="PropertyExpression"/> is null.
    /// </summary>
    public Func<T, object?>? ValueSelector { get; init; }
}

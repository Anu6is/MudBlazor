namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Represents a single node in the group tree produced by
/// <c>IGridGroupingService&lt;T&gt;.BuildGroupTree</c> and stored in
/// <c>QuickGridState&lt;T&gt;.GroupNodes</c>.
/// </summary>
/// <remarks>
/// <para>
/// Leaf nodes have non-empty <see cref="Items"/> and empty <see cref="Children"/>.
/// Interior nodes have non-empty <see cref="Children"/> and empty <see cref="Items"/>.
/// </para>
/// <para>
/// <see cref="KeyPath"/> is used as the <c>@key</c> value in recursive
/// <c>GridGroupNode&lt;T&gt;</c> rendering — it is unique across the entire tree so
/// that Blazor's diffing algorithm can track nodes correctly across expand/collapse cycles.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed record GroupNode<T>
{
    /// <summary>
    /// Gets or initialises the group key value for this node (the raw value returned by
    /// the column's <c>GroupBy</c> function).
    /// </summary>
    public object Key { get; init; } = default!;

    /// <summary>
    /// Gets or initialises the pipe-delimited path of ancestor keys ending at this node
    /// (e.g. <c>"Engineering|Backend"</c>).  Used as a stable <c>@key</c> in Razor rendering.
    /// </summary>
    public string KeyPath { get; init; } = "";

    /// <summary>Gets or initialises the zero-based depth of this node in the group tree.</summary>
    public int Depth { get; init; }

    /// <summary>
    /// Gets or initialises whether this group node is currently expanded.
    /// Derived from <c>QuickGridState&lt;T&gt;.GroupExpansionState</c> at build time.
    /// </summary>
    public bool IsExpanded { get; init; }

    /// <summary>
    /// Gets or initialises the child group nodes.  Non-empty only for interior nodes.
    /// </summary>
    public IReadOnlyList<GroupNode<T>> Children { get; init; } = [];

    /// <summary>
    /// Gets or initialises the row items belonging directly to this leaf node.
    /// Non-empty only for leaf nodes.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// Gets or initialises a dictionary of pre-computed aggregate values for this group node,
    /// keyed by column ID.  Populated by <c>IGridGroupingService&lt;T&gt;.BuildGroupTree</c>
    /// when columns have <c>AggregateDefinition</c> set.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Aggregates { get; init; } =
        new Dictionary<string, object?>();
}

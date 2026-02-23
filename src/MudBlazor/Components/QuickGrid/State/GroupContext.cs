namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Provides data to a column's <c>GroupTemplate</c> render fragment.
/// Used to customise the display of a group row header.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class GroupContext<T>
{
    /// <summary>Gets the group node whose header row is being rendered.</summary>
    public GroupNode<T> GroupNode { get; init; } = new();

    /// <summary>
    /// Gets the raw group key value (the value returned by the column's <c>GroupBy</c> function).
    /// </summary>
    public object Key => GroupNode.Key;

    /// <summary>Gets the zero-based depth of this group in the tree.</summary>
    public int Depth => GroupNode.Depth;

    /// <summary>Gets whether this group is currently expanded.</summary>
    public bool IsExpanded => GroupNode.IsExpanded;

    /// <summary>
    /// Gets the aggregate values computed for this group node, keyed by column ID.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Aggregates => GroupNode.Aggregates;

    /// <summary>Gets the total number of leaf items within this group (all depths).</summary>
    public int ItemCount { get; init; }

    /// <summary>
    /// Gets a delegate that toggles the expanded/collapsed state of this group.
    /// </summary>
    public Func<Task> ToggleExpansionAsync { get; init; } = () => Task.CompletedTask;
}

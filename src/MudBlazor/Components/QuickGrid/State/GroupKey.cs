namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// A lightweight value type that wraps a <c>KeyPath</c> string for use as a
/// dictionary key in <c>QuickGridState&lt;T&gt;.GroupExpansionState</c>.
/// </summary>
/// <remarks>
/// <c>KeyPath</c> is a pipe-delimited concatenation of ancestor group keys down to and
/// including the current node (e.g. <c>"Engineering|Backend"</c>), providing a unique
/// and stable identity for each node regardless of key type or value.
/// </remarks>
/// <param name="KeyPath">
/// The pipe-delimited path uniquely identifying this group node within the tree.
/// </param>
public readonly record struct GroupKey(string KeyPath)
{
    /// <summary>Returns the <see cref="KeyPath"/> string representation.</summary>
    public override string ToString() => KeyPath;
}

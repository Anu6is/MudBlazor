// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// A serialisable snapshot of column presentation state: order, visibility, and widths.
/// Stored as part of <see cref="GridSnapshot"/> and also persisted independently via
/// <c>IColumnStatePersistenceProvider</c>.
/// </summary>
public sealed record ColumnLayout
{
    /// <summary>
    /// Gets or initialises the ordered list of column IDs representing the user-configured
    /// display order.  Columns not present are appended at the end in markup order.
    /// </summary>
    public IReadOnlyList<string> ColumnOrder { get; init; } = [];

    /// <summary>
    /// Gets or initialises the set of column IDs that are currently hidden.
    /// Columns not present in this set are visible.
    /// </summary>
    public IReadOnlySet<string> HiddenColumns { get; init; } = new HashSet<string>();

    /// <summary>
    /// Gets or initialises the map of column IDs to their widths in pixels.
    /// Columns not present use their CSS <c>Width</c> parameter or browser default.
    /// </summary>
    public IReadOnlyDictionary<string, double> ColumnWidths { get; init; } = new Dictionary<string, double>();
}

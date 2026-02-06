// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace MudBlazor;

/// <summary>
/// Represents the information related to a <see cref="MudDataGrid{T}.SortChanged"/> event.
/// </summary>
/// <typeparam name="T">The item managed by the <see cref="MudDataGrid{T}"/>.</typeparam>
public class DataGridSortChangedEventArgs<T> : EventArgs
{
    /// <summary>
    /// The column whose sorting was changed.
    /// </summary>
    public Column<T>? Column { get; }

    /// <summary>
    /// The field name of the column whose sorting was changed.
    /// </summary>
    public string Field { get; }

    /// <summary>
    /// The new sort direction.
    /// </summary>
    public SortDirection SortDirection { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="column">The column whose sorting was changed.</param>
    /// <param name="field">The field name of the column whose sorting was changed.</param>
    /// <param name="sortDirection">The new sort direction.</param>
    public DataGridSortChangedEventArgs(Column<T>? column, string field, SortDirection sortDirection)
    {
        Column = column;
        Field = field;
        SortDirection = sortDirection;
    }
}

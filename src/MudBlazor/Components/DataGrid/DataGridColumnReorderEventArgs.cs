// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace MudBlazor;

/// <summary>
/// Represents the information related to a <see cref="MudDataGrid{T}.ColumnReordered"/> event.
/// </summary>
/// <typeparam name="T">The item managed by the <see cref="MudDataGrid{T}"/>.</typeparam>
public class DataGridColumnReorderEventArgs<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : EventArgs
{
    /// <summary>
    /// The column that was moved.
    /// </summary>
    public Column<T> Column { get; }

    /// <summary>
    /// The original index of the column.
    /// </summary>
    public int OldIndex { get; }

    /// <summary>
    /// The new index of the column.
    /// </summary>
    public int NewIndex { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="column">The column that was moved.</param>
    /// <param name="oldIndex">The original index of the column.</param>
    /// <param name="newIndex">The new index of the column.</param>
    public DataGridColumnReorderEventArgs(Column<T> column, int oldIndex, int newIndex)
    {
        Column = column;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }
}

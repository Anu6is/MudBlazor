// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace MudBlazor;

/// <summary>
/// Represents the information related to a <see cref="MudDataGrid{T}.ColumnResized"/> event.
/// </summary>
/// <typeparam name="T">The item managed by the <see cref="MudDataGrid{T}"/>.</typeparam>
public class DataGridColumnResizeEventArgs<T> : EventArgs
{
    /// <summary>
    /// The column that was resized.
    /// </summary>
    public Column<T> Column { get; }

    /// <summary>
    /// The new width of the column in pixels.
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="column">The column that was resized.</param>
    /// <param name="width">The new width of the column in pixels.</param>
    public DataGridColumnResizeEventArgs(Column<T> column, double width)
    {
        Column = column;
        Width = width;
    }
}

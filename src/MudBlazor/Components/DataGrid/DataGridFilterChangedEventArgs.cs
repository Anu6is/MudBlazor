// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace MudBlazor;

/// <summary>
/// Represents the information related to a <see cref="MudDataGrid{T}.FilterChanged"/> event.
/// </summary>
/// <typeparam name="T">The item managed by the <see cref="MudDataGrid{T}"/>.</typeparam>
public class DataGridFilterChangedEventArgs<T> : EventArgs
{
    /// <summary>
    /// The column whose filter was changed.
    /// </summary>
    public Column<T>? Column { get; }

    /// <summary>
    /// The filter definition that was changed.
    /// </summary>
    public IFilterDefinition<T> FilterDefinition { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="column">The column whose filter was changed.</param>
    /// <param name="filterDefinition">The filter definition that was changed.</param>
    public DataGridFilterChangedEventArgs(Column<T>? column, IFilterDefinition<T> filterDefinition)
    {
        Column = column;
        FilterDefinition = filterDefinition;
    }
}

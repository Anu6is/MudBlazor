// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// A do-nothing implementation of <see cref="IColumnStatePersistenceProvider"/> that
/// returns <see langword="null"/> from <see cref="LoadAsync"/> and ignores all
/// <see cref="SaveAsync"/> calls.
/// </summary>
/// <remarks>
/// Registered by default via <c>services.AddMudQuickGrid()</c>. Consumers who need
/// real persistence replace this with their own implementation in DI:
/// <code>
/// services.AddScoped&lt;IColumnStatePersistenceProvider, LocalStorageColumnStatePersistenceProvider&gt;();
/// </code>
/// </remarks>
public sealed class NoOpColumnStatePersistenceProvider : IColumnStatePersistenceProvider
{
    /// <inheritdoc/>
    /// <returns>Always <see langword="null"/> — no layout has been persisted.</returns>
    public Task<ColumnLayout?> LoadAsync(string persistenceKey, CancellationToken ct = default)
        => Task.FromResult<ColumnLayout?>(null);

    /// <inheritdoc/>
    public Task SaveAsync(string persistenceKey, ColumnLayout layout, CancellationToken ct = default)
        => Task.CompletedTask;
}

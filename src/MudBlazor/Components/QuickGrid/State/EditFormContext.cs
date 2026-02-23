namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Provides the item being edited and control delegates to the
/// <c>MudQuickGrid&lt;T&gt;.EditFormContent</c> render fragment
/// used in <c>DataGridEditMode.Form</c>.
/// </summary>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class EditFormContext<T>
{
    /// <summary>
    /// Gets the working copy of the item being edited.
    /// This is a deep clone of the original item; mutations to this instance do not
    /// affect the grid data source until <see cref="CommitAsync"/> is called.
    /// </summary>
    public T Item { get; init; } = default!;

    /// <summary>
    /// Gets a delegate that commits the edited item and closes the edit dialog.
    /// Fires <c>MudQuickGrid&lt;T&gt;.RowEditCommit</c> with the mutated <see cref="Item"/>.
    /// </summary>
    public Func<Task> CommitAsync { get; init; } = () => Task.CompletedTask;

    /// <summary>
    /// Gets a delegate that cancels the edit and closes the dialog without persisting changes.
    /// Fires <c>MudQuickGrid&lt;T&gt;.RowEditCancel</c>.
    /// </summary>
    public Func<Task> CancelAsync { get; init; } = () => Task.CompletedTask;

    /// <summary>Gets whether the item being edited is a newly-added row.</summary>
    public bool IsNewRow { get; init; }
}

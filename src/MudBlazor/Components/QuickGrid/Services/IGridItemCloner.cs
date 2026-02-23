namespace MudBlazor.QuickGrid.Services;

/// <summary>
/// Creates a deep clone of a row item so that <c>QuickGridState&lt;T&gt;</c> can store an
/// unmodified backup before entering edit mode.  On cancel, the backup replaces the
/// working copy, restoring the original values.
/// </summary>
/// <remarks>
/// <para>
/// The default implementation (<c>DefaultGridItemCloner&lt;T&gt;</c>, Phase 2B) uses
/// <c>System.Text.Json.JsonSerializer</c> for a round-trip deep clone.  This works
/// correctly for all JSON-serialisable types but does not preserve non-public state,
/// circular references, or types that lack a parameterless constructor.
/// </para>
/// <para>
/// Replace via DI to use a custom cloning strategy:
/// <code>
/// services.AddScoped&lt;IGridItemCloner&lt;MyEntity&gt;, MyEntityCloner&gt;();
/// </code>
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public interface IGridItemCloner<T>
{
    /// <summary>
    /// Returns a deep clone of <paramref name="item"/>.
    /// The clone must be a fully independent copy — mutations to the clone must not
    /// affect <paramref name="item"/> and vice-versa.
    /// </summary>
    /// <param name="item">The item to clone.  Must not be <see langword="null"/>.</param>
    /// <returns>A deep copy of <paramref name="item"/>.</returns>
    T Clone(T item);
}

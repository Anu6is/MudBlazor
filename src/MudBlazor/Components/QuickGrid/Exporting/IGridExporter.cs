namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Produces an export file in a specific format from a
/// <see cref="GridExportSnapshot{T}"/> and writes it to a stream.
/// </summary>
/// <remarks>
/// <para>
/// Implementations are registered in DI and discovered at runtime by
/// <c>MudQuickGrid&lt;T&gt;.ExportAsync(name?)</c>.  Multiple exporters can be
/// registered simultaneously; the toolbar export menu lists them by <see cref="Name"/>.
/// </para>
/// <para>
/// The built-in <c>GridCsvExporter&lt;T&gt;</c> is registered automatically by
/// <c>services.AddMudQuickGrid()</c>.  Optional Excel and PDF exporters are available
/// as separate NuGet packages (<c>MudBlazor.DataGrid.Exporters.Excel</c> and
/// <c>MudBlazor.DataGrid.Exporters.Pdf</c>).
/// </para>
/// <para>
/// The <see cref="GridExportSnapshot{T}.Items"/> collection contains the
/// <em>full</em> filtered dataset — not just the current page — so that exports
/// are never truncated by pagination.
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public interface IGridExporter<T>
{
    /// <summary>
    /// Gets the human-readable format name shown in the toolbar export menu,
    /// e.g. <c>"CSV"</c> or <c>"Excel"</c>.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the file extension (including the leading dot) used when the browser
    /// downloads the exported file, e.g. <c>".csv"</c> or <c>".xlsx"</c>.
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Gets the MIME content type of the produced file,
    /// e.g. <c>"text/csv"</c> or <c>"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"</c>.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Exports <paramref name="snapshot"/> to <paramref name="outputStream"/>.
    /// </summary>
    /// <param name="snapshot">
    /// A point-in-time snapshot of the grid's data and metadata.
    /// <see cref="GridExportSnapshot{T}.Items"/> contains the full filtered dataset
    /// (unpaginated).
    /// </param>
    /// <param name="outputStream">
    /// The stream to write the export bytes to.  The caller owns the stream's lifecycle.
    /// The exporter should write to the stream but must not close or dispose it.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task ExportAsync(GridExportSnapshot<T> snapshot, Stream outputStream, CancellationToken ct);
}

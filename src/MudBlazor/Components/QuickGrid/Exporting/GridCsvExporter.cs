// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Text;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Built-in <see cref="IGridExporter{T}"/> that produces a UTF-8 CSV file conforming
/// to <see href="https://www.rfc-editor.org/rfc/rfc4180">RFC 4180</see>.
/// </summary>
/// <remarks>
/// <para>
/// Registered automatically by <c>services.AddMudQuickGrid()</c>.
/// </para>
/// <para>
/// RFC 4180 escaping rules applied:
/// <list type="bullet">
///   <item><description>Fields containing commas, double-quotes, or CRLF are enclosed in double-quotes.</description></item>
///   <item><description>Embedded double-quote characters are escaped by doubling them (<c>""</c>).</description></item>
///   <item><description>Records are separated by CRLF (<c>\r\n</c>).</description></item>
///   <item><description>The file begins with a UTF-8 BOM so Excel auto-detects the encoding.</description></item>
/// </list>
/// </para>
/// </remarks>
/// <typeparam name="T">The grid row type.</typeparam>
public sealed class GridCsvExporter<T> : IGridExporter<T>
{
    /// <inheritdoc/>
    public string Name => "CSV";

    /// <inheritdoc/>
    public string FileExtension => ".csv";

    /// <inheritdoc/>
    public string ContentType => "text/csv;charset=utf-8";

    /// <inheritdoc/>
    public async Task ExportAsync(
        GridExportSnapshot<T> snapshot,
        Stream outputStream,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(outputStream);

        // UTF-8 with BOM so that Excel opens the file correctly without a manual import step.
        await using var writer = new StreamWriter(outputStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true);

        var columns = snapshot.Columns;

        // ── Header row ────────────────────────────────────────────────────────
        await WriteRowAsync(writer, columns.Select(c => c.Title), ct);

        // ── Data rows ─────────────────────────────────────────────────────────
        foreach (var item in snapshot.Items)
        {
            ct.ThrowIfCancellationRequested();

            var fields = columns.Select(col => FormatValue(col, item));
            await WriteRowAsync(writer, fields, ct);
        }

        await writer.FlushAsync(ct);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task WriteRowAsync(
        TextWriter writer,
        IEnumerable<string> fields,
        CancellationToken ct)
    {
        var sb = new StringBuilder();
        bool first = true;

        foreach (var field in fields)
        {
            if (!first)
                sb.Append(',');

            sb.Append(EscapeField(field));
            first = false;
        }

        // RFC 4180 §2: each record terminated by CRLF.
        sb.Append("\r\n");

        await writer.WriteAsync(sb, ct);
    }

    private static string FormatValue(ExportColumn<T> column, T item)
    {
        var raw = column.ValueSelector(item);

        if (raw is null)
            return string.Empty;

        if (column.Format is not null && raw is IFormattable formattable)
            return formattable.ToString(column.Format, CultureInfo.CurrentCulture);

        return raw.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Applies RFC 4180 field escaping. A field is enclosed in double-quotes whenever
    /// it contains a comma, a double-quote, a carriage-return, or a line-feed.
    /// Any embedded double-quote character is doubled.
    /// </summary>
    internal static string EscapeField(string value)
    {
        if (value.Length == 0)
            return string.Empty;

        bool needsQuoting =
            value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\r') ||
            value.Contains('\n');

        if (!needsQuoting)
            return value;

        // Double all embedded quote characters, then wrap in outer quotes.
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}

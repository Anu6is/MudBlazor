namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Configures case-sensitivity and culture behaviour for string filter comparisons.
/// Passed to <see cref="IFilterDefinition{T}"/> during expression generation.
/// </summary>
public sealed record FilterOptions
{
    /// <summary>
    /// Gets or initialises the <see cref="StringComparison"/> used for string filter operators.
    /// Defaults to <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    public StringComparison StringComparison { get; init; } = StringComparison.OrdinalIgnoreCase;

    /// <summary>
    /// A shared, immutable instance with the default (case-insensitive ordinal) settings.
    /// </summary>
    public static readonly FilterOptions Default = new();

    /// <summary>
    /// A shared, immutable instance that performs case-sensitive ordinal comparisons.
    /// </summary>
    public static readonly FilterOptions CaseSensitive = new()
    {
        StringComparison = StringComparison.Ordinal,
    };
}

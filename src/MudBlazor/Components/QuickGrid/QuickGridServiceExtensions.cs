using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MudBlazor.Components.QuickGrid;

/// <summary>
/// Extension methods for registering <c>MudQuickGrid&lt;T&gt;</c> services with
/// the .NET dependency injection container.
/// </summary>
public static class QuickGridServiceExtensions
{
    /// <summary>
    /// Registers the default <c>MudQuickGrid&lt;T&gt;</c> service implementations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The following registrations are made:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <see cref="IColumnStatePersistenceProvider"/> →
    ///       <see cref="NoOpColumnStatePersistenceProvider"/> (scoped, replaceable).
    ///       Replace with a custom implementation to persist column layout across sessions.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="IGridExporter{T}"/> →
    ///       <see cref="GridCsvExporter{T}"/> (scoped, open-generic).
    ///       Additional exporters (Excel, PDF) are available as separate NuGet packages.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="IGridSortService{T}"/> →
    ///       <see cref="GridSortService{T}"/> (scoped, open-generic).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="IGridFilterService{T}"/> →
    ///       <see cref="GridFilterService{T}"/> (scoped, open-generic).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="IGridGroupingService{T}"/> →
    ///       <see cref="GridGroupingService{T}"/> (scoped, open-generic).
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="IGridItemCloner{T}"/> →
    ///       <see cref="DefaultGridItemCloner{T}"/> (scoped, open-generic).
    ///       Replace with a custom implementation for types that are not JSON-serialisable.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddMudQuickGrid(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Column layout persistence — no-op by default; replace in DI for real storage.
        services.TryAddScoped<IColumnStatePersistenceProvider, NoOpColumnStatePersistenceProvider>();

        // Core data-pipeline services (open-generic — one instance per T per scope).
        services.TryAdd(ServiceDescriptor.Scoped(typeof(IGridSortService<>), typeof(GridSortService<>)));
        services.TryAdd(ServiceDescriptor.Scoped(typeof(IGridFilterService<>), typeof(GridFilterService<>)));
        services.TryAdd(ServiceDescriptor.Scoped(typeof(IGridGroupingService<>), typeof(GridGroupingService<>)));

        // Item cloner for edit-mode backup/restore.
        services.TryAdd(ServiceDescriptor.Scoped(typeof(IGridItemCloner<>), typeof(DefaultGridItemCloner<>)));

        // Built-in CSV exporter (registered as IGridExporter<T> using open-generic).
        // Multiple exporters can coexist; the grid toolbar lists all IGridExporter<T>
        // registrations. Additional exporters are registered via the same interface.
        services.TryAdd(ServiceDescriptor.Scoped(typeof(IGridExporter<>), typeof(GridCsvExporter<>)));

        return services;
    }

    public static IServiceCollection AddJsonGridItemCloner<T>(this IServiceCollection services, JsonTypeInfo<T> typeInfo)
    {
        services.AddScoped<IGridItemCloner<T>>(_ => new DefaultGridItemCloner<T>(typeInfo));

        return services;
    }
}

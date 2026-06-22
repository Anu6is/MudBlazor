using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Components.QuickGrid;

namespace MudBlazor.QuickGrid.Registration;

/// <summary>
/// Extension methods for registering <c>MudQuickGrid&lt;T&gt;</c> services.
/// </summary>
public static class QuickGridServiceExtensions
{
    /// <summary>
    /// Registers the default <c>MudQuickGrid&lt;T&gt;</c> service implementations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>What is registered:</b>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <see cref="IColumnStatePersistenceProvider"/> →
    ///       <see cref="NoOpColumnStatePersistenceProvider"/> (singleton).
    ///       Replace to persist column visibility, order, and widths across sessions.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="IGridGroupingService{T}"/> →
    ///       <see cref="GridGroupingService{T}"/> (scoped, open-generic).
    ///       Used for <c>BuildGroupTree</c> after page items are materialised.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <see cref="IGridExporter{T}"/> →
    ///       <see cref="GridCsvExporter{T}"/> (scoped, open-generic).
    ///       The built-in CSV exporter.  Additional exporters may be registered
    ///       alongside this one; all <see cref="IGridExporter{T}"/> registrations
    ///       appear in the grid toolbar's export menu.
    ///     </description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>What is NOT registered here:</b>
    /// <c>IGridSortService&lt;T&gt;</c> and <c>IGridFilterService&lt;T&gt;</c> are no longer
    /// part of the grid's DI surface.  Sorting and filtering are now entirely the
    /// responsibility of the <c>IGridDataSource&lt;T&gt;</c> implementation.
    /// <c>QueryableDataSource&lt;T&gt;</c> uses the internal <c>QueryableSortApplicator</c>
    /// and <c>QueryableFilterApplicator</c> directly; <c>InMemoryDataSource&lt;T&gt;</c>
    /// uses <c>InMemoryFilterEvaluator</c>.  Neither service is needed in DI.
    /// </para>
    /// <para>
    /// All registrations use <c>TryAdd</c> semantics — consumer-provided registrations
    /// that appear before this call are never overwritten.
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <returns><paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddMudQuickGrid(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Column layout persistence — no-op by default; replace for real storage.
        services.TryAddSingleton<IColumnStatePersistenceProvider, NoOpColumnStatePersistenceProvider>();

        // Grouping service — BuildGroupTree is a rendering concern, not an adapter concern.
        services.TryAdd(ServiceDescriptor.Scoped(
            typeof(IGridGroupingService<>),
            typeof(GridGroupingService<>)));

        // Built-in CSV exporter.
        services.TryAdd(ServiceDescriptor.Scoped(
            typeof(IGridExporter<>),
            typeof(GridCsvExporter<>)));

        return services;
    }

    /// <summary>
    /// Adds an additional <see cref="IGridExporter{T}"/> implementation to the service
    /// collection so that it appears alongside the built-in CSV exporter in the toolbar.
    /// </summary>
    /// <typeparam name="TExporter">
    /// The exporter implementation type.  Must implement <see cref="IGridExporter{T}"/>.
    /// </typeparam>
    /// <typeparam name="T">The grid row type.</typeparam>
    public static IServiceCollection AddMudQuickGridExporter<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TExporter>(this IServiceCollection services) where TExporter : class, IGridExporter<T>
    {
        services.AddScoped<IGridExporter<T>, TExporter>();
        return services;
    }

    public static IServiceCollection AddJsonGridItemCloner<T>(this IServiceCollection services, JsonTypeInfo<T> typeInfo)
    {
        services.AddScoped<IGridItemCloner<T>>(_ => new DefaultGridItemCloner<T>(typeInfo));

        return services;
    }
}

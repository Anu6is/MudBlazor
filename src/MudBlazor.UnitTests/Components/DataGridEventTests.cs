// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.UnitTests.TestComponents.DataGrid;
using NUnit.Framework;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class DataGridEventTests : BunitTest
    {
        [Test]
        public async Task DataGrid_ColumnResized_EventFires()
        {
            var items = new List<DataGridEventsTest.Model> { new("John", 30) };
            RenderFragment columns = builder =>
            {
                builder.OpenComponent<PropertyColumn<DataGridEventsTest.Model, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<DataGridEventsTest.Model, string>.Property), (System.Linq.Expressions.Expression<Func<DataGridEventsTest.Model, string>>)(x => x.Name));
                builder.CloseComponent();
            };

            var comp = Context.Render<DataGridEventsTest>(parameters => parameters
                .Add(p => p.Items, items)
                .Add(p => p.Columns, columns)
                .Add(p => p.ColumnResizeMode, ResizeMode.Container)
            );

            var dataGrid = comp.Instance.DataGrid;
            var headerCell = comp.FindComponent<HeaderCell<DataGridEventsTest.Model>>();

            // Mock mudElementRef.getBoundingClientRect for DataGrid and HeaderCell
            var gridElement = (ElementReference)dataGrid.GetType()
                .GetField("_gridElement", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(dataGrid)!;
            Context.JSInterop
              .Setup<Interop.BoundingClientRect>("mudElementRef.getBoundingClientRect", gridElement)
              .SetResult(new Interop.BoundingClientRect { Height = 100 });

            var headerElement = (ElementReference)headerCell.Instance.GetType()
                .GetField("_headerElement", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(headerCell.Instance)!;
            Context.JSInterop
                .Setup<Interop.BoundingClientRect>("mudElementRef.getBoundingClientRect", headerElement)
                .SetResult(new Interop.BoundingClientRect { Width = 100 });

            // Simulate resize
            var resizer = comp.Find(".mud-resizer");
            await resizer.PointerDownAsync(new PointerEventArgs { ClientX = 100, PointerId = 1, Detail = 1 });

            // Update mock to return new width when GetCurrentCellWidth is called during/after resize
            Context.JSInterop
                .Setup<Interop.BoundingClientRect>("mudElementRef.getBoundingClientRect", headerElement)
                .SetResult(new Interop.BoundingClientRect { Width = 150 });

            await resizer.PointerMoveAsync(new PointerEventArgs { ClientX = 150, PointerId = 1 });
            await resizer.PointerUpAsync(new PointerEventArgs { ClientX = 150, PointerId = 1 });

            comp.Instance.ResizeEvents.Should().HaveCount(1);
            comp.Instance.ResizeEvents[0].Column.PropertyName.Should().Be("Name");
            comp.Instance.ResizeEvents[0].Width.Should().Be(150);
        }

        [Test]
        public async Task DataGrid_ColumnReordered_DragDrop_EventFires()
        {
            var items = new List<DataGridEventsTest.Model> { new("John", 30) };
            RenderFragment columns = builder =>
            {
                builder.OpenComponent<PropertyColumn<DataGridEventsTest.Model, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<DataGridEventsTest.Model, string>.Property), (System.Linq.Expressions.Expression<Func<DataGridEventsTest.Model, string>>)(x => x.Name));
                builder.AddAttribute(2, nameof(PropertyColumn<DataGridEventsTest.Model, string>.Title), "Name");
                builder.CloseComponent();
                builder.OpenComponent<PropertyColumn<DataGridEventsTest.Model, int>>(3);
                builder.AddAttribute(4, nameof(PropertyColumn<DataGridEventsTest.Model, int>.Property), (System.Linq.Expressions.Expression<Func<DataGridEventsTest.Model, int>>)(x => x.Age));
                builder.AddAttribute(5, nameof(PropertyColumn<DataGridEventsTest.Model, int>.Title), "Age");
                builder.CloseComponent();
            };

            var comp = Context.Render<DataGridEventsTest>(parameters => parameters
                .Add(p => p.Items, items)
                .Add(p => p.Columns, columns)
                .Add(p => p.DragDropColumnReordering, true)
            );

            var dataGrid = comp.Instance.DataGrid;
            dataGrid.DropContainerHasChanged();

            var zone = comp.FindAll(".mud-drop-zone");
            zone.Count.Should().Be(2);

            var firstDropItem = zone[0].Children[0];
            var secondDropItem = zone[1].Children[0];

            await firstDropItem.DragStartAsync(new DragEventArgs());
            await secondDropItem.DropAsync(new DragEventArgs());

            // Drag and drop on headers uses ItemUpdatedAsync which is a swap, firing an event for each column.
            comp.Instance.ReorderEvents.Should().HaveCount(2);

            var nameEvent = comp.Instance.ReorderEvents.FirstOrDefault(e => e.Column.PropertyName == "Name");
            nameEvent.Should().NotBeNull();
            nameEvent.OldIndex.Should().Be(0);
            nameEvent.NewIndex.Should().Be(1);

            var ageEvent = comp.Instance.ReorderEvents.FirstOrDefault(e => e.Column.PropertyName == "Age");
            ageEvent.Should().NotBeNull();
            ageEvent.OldIndex.Should().Be(1);
            ageEvent.NewIndex.Should().Be(0);
        }

        [Test]
        public async Task DataGrid_ColumnReordered_UpDownButtons_EventFires()
        {
            var items = new List<DataGridEventsTest.Model> { new("John", 30) };
            RenderFragment columns = builder =>
            {
                builder.OpenComponent<PropertyColumn<DataGridEventsTest.Model, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<DataGridEventsTest.Model, string>.Property), (System.Linq.Expressions.Expression<Func<DataGridEventsTest.Model, string>>)(x => x.Name));
                builder.AddAttribute(2, nameof(PropertyColumn<DataGridEventsTest.Model, string>.Title), "Name");
                builder.CloseComponent();
                builder.OpenComponent<PropertyColumn<DataGridEventsTest.Model, int>>(3);
                builder.AddAttribute(4, nameof(PropertyColumn<DataGridEventsTest.Model, int>.Property), (System.Linq.Expressions.Expression<Func<DataGridEventsTest.Model, int>>)(x => x.Age));
                builder.AddAttribute(5, nameof(PropertyColumn<DataGridEventsTest.Model, int>.Title), "Age");
                builder.CloseComponent();
            };

            var comp = Context.Render<DataGridEventsTest>(parameters => parameters
                .Add(p => p.Items, items)
                .Add(p => p.Columns, columns)
                .Add(p => p.ColumnsPanelReordering, true)
            );

            var dataGrid = comp.Instance.DataGrid;

            // Open columns panel
            await comp.InvokeAsync(() => dataGrid.ShowColumnsPanel());

            // Find MoveDown button for Name (first column)
            // It's the first ArrowDropDown icon button
            var moveDownButton = comp.WaitForElement("button[title='Move down']");
            await moveDownButton.ClickAsync();

            comp.Instance.ReorderEvents.Should().HaveCount(1);
            comp.Instance.ReorderEvents[0].Column.PropertyName.Should().Be("Name");
            comp.Instance.ReorderEvents[0].OldIndex.Should().Be(0);
            comp.Instance.ReorderEvents[0].NewIndex.Should().Be(1);

            comp.Instance.ReorderEvents.Clear();

            // Find MoveUp button for Name (now second column)
            var moveUpButton = comp.WaitForElement("button[title='Move up']");
            await moveUpButton.ClickAsync();

            comp.Instance.ReorderEvents.Should().HaveCount(1);
            comp.Instance.ReorderEvents[0].Column.PropertyName.Should().Be("Name");
            comp.Instance.ReorderEvents[0].OldIndex.Should().Be(1);
            comp.Instance.ReorderEvents[0].NewIndex.Should().Be(0);
        }

        [Test]
        public async Task DataGrid_SortChanged_EventFires()
        {
            var items = new List<DataGridEventsTest.Model> { new("John", 30) };
            RenderFragment columns = builder =>
            {
                builder.OpenComponent<PropertyColumn<DataGridEventsTest.Model, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<DataGridEventsTest.Model, string>.Property), (System.Linq.Expressions.Expression<Func<DataGridEventsTest.Model, string>>)(x => x.Name));
                builder.CloseComponent();
            };

            var comp = Context.Render<DataGridEventsTest>(parameters => parameters
                .Add(p => p.Items, items)
                .Add(p => p.Columns, columns)
            );

            var headerCell = comp.FindComponent<HeaderCell<DataGridEventsTest.Model>>();

            // Click to sort
            await headerCell.InvokeAsync(() => headerCell.Instance.SortChangedAsync(new MouseEventArgs()));

            comp.Instance.SortEvents.Should().HaveCount(1);
            comp.Instance.SortEvents[0].Field.Should().Be("Name");
            comp.Instance.SortEvents[0].SortDirection.Should().Be(SortDirection.Ascending);

            comp.Instance.SortEvents.Clear();

            // Click again to change sort
            await headerCell.InvokeAsync(() => headerCell.Instance.SortChangedAsync(new MouseEventArgs()));

            comp.Instance.SortEvents.Should().HaveCount(1);
            comp.Instance.SortEvents[0].SortDirection.Should().Be(SortDirection.Descending);

            comp.Instance.SortEvents.Clear();

            // Alt+Click to remove sort
            await headerCell.InvokeAsync(() => headerCell.Instance.SortChangedAsync(new MouseEventArgs { AltKey = true }));

            comp.Instance.SortEvents.Should().HaveCount(1);
            comp.Instance.SortEvents[0].SortDirection.Should().Be(SortDirection.None);
        }

        [Test]
        public async Task DataGrid_FilterChanged_EventFires()
        {
            var items = new List<DataGridEventsTest.Model> { new("John", 30) };
            RenderFragment columns = builder =>
            {
                builder.OpenComponent<PropertyColumn<DataGridEventsTest.Model, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<DataGridEventsTest.Model, string>.Property), (System.Linq.Expressions.Expression<Func<DataGridEventsTest.Model, string>>)(x => x.Name));
                builder.AddAttribute(2, nameof(PropertyColumn<DataGridEventsTest.Model, string>.Filterable), true);
                builder.CloseComponent();
            };

            var comp = Context.Render<DataGridEventsTest>(parameters => parameters
                .Add(p => p.Items, items)
                .Add(p => p.Columns, columns)
            );

            var dataGrid = comp.Instance.DataGrid;

            // Add filter
            var filterDefinition = new FilterDefinition<DataGridEventsTest.Model>
            {
                Column = dataGrid.RenderedColumns.First(),
                Operator = FilterOperator.String.Contains,
                Value = "Jo"
            };

            await comp.InvokeAsync(() => dataGrid.AddFilterAsync(filterDefinition));

            comp.Instance.FilterEvents.Should().HaveCount(1);
            comp.Instance.FilterEvents[0].FilterDefinition.Value.Should().Be("Jo");

            comp.Instance.FilterEvents.Clear();

            // Clear filters
            await comp.InvokeAsync(() => dataGrid.ClearFiltersAsync());

            comp.Instance.FilterEvents.Should().HaveCount(1);
            comp.Instance.FilterEvents[0].FilterDefinition.Id.Should().Be(filterDefinition.Id);
        }

        [Test]
        public async Task DataGrid_FilterChanged_FilterPanel_EventFires()
        {
            var items = new List<DataGridEventsTest.Model> { new("John", 30) };
            RenderFragment columns = builder =>
            {
                builder.OpenComponent<PropertyColumn<DataGridEventsTest.Model, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<DataGridEventsTest.Model, string>.Property), (System.Linq.Expressions.Expression<Func<DataGridEventsTest.Model, string>>)(x => x.Name));
                builder.AddAttribute(2, nameof(PropertyColumn<DataGridEventsTest.Model, string>.Filterable), true);
                builder.CloseComponent();
            };

            var comp = Context.Render<DataGridEventsTest>(parameters => parameters
                .Add(p => p.Items, items)
                .Add(p => p.Columns, columns)
                .Add(p => p.FilterMode, DataGridFilterMode.Simple)
            );

            var dataGrid = comp.Instance.DataGrid;

            // Open filter menu and add filter
            await comp.InvokeAsync(() => dataGrid.AddFilter());

            comp.Instance.FilterEvents.Should().HaveCount(1);
            var filterDefinition = comp.Instance.FilterEvents[0].FilterDefinition;
            filterDefinition.Column.PropertyName.Should().Be("Name");

            comp.Instance.FilterEvents.Clear();

            // Simulate typing in the filter input
            var filter = new Filter<DataGridEventsTest.Model>(dataGrid, filterDefinition, (Column<DataGridEventsTest.Model>)filterDefinition.Column);
            await comp.InvokeAsync(() => filter.StringValueChanged("Jo"));

            comp.Instance.FilterEvents.Should().HaveCount(1);
            comp.Instance.FilterEvents[0].FilterDefinition.Value.Should().Be("Jo");
        }
    }
}

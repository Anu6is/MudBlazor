// Copyright (c) MudBlazor 2021
// MudBlazor licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Bunit;
using MudBlazor.UnitTests.TestComponents;
using NUnit.Framework;
using AwesomeAssertions;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MudBlazor.UnitTests.Components
{
    [TestFixture]
    public class DatePickerMaskTests : BunitTest
    {
        [Test]
        public async Task DatePicker_WithMask_ShouldClearText_WhenDateSetToNull()
        {
            var comp = Context.Render<MudDatePicker>(parameters => parameters
                .Add(p => p.Mask, new DateMask("dd/MM/yyyy"))
                .Add(p => p.Date, new DateTime(2021, 1, 1))
            );
            var picker = comp.Instance;

            picker.Text.Should().Be("01/01/2021");

            await comp.SetParametersAndRenderAsync(parameters => parameters.Add(p => p.Date, null));

            picker.Date.Should().BeNull();
            picker.Text.Should().BeNullOrEmpty();

            var input = comp.Find("input");
            input.GetAttribute("value").Should().BeNullOrEmpty();
        }

        [Test]
        public async Task DatePicker_WithMask_ShouldClearText_WhenClearAsyncCalled()
        {
            var comp = Context.Render<MudDatePicker>(parameters => parameters
                .Add(p => p.Mask, new DateMask("dd/MM/yyyy"))
                .Add(p => p.Date, new DateTime(2021, 1, 1))
            );
            var picker = comp.Instance;

            picker.Text.Should().Be("01/01/2021");

            await comp.InvokeAsync(() => picker.ClearAsync());

            picker.Date.Should().BeNull();
            picker.Text.Should().BeNullOrEmpty();

            var input = comp.Find("input");
            input.GetAttribute("value").Should().BeNullOrEmpty();
        }
    }
}

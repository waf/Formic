using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Formic.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        private static IReadOnlyList<string> entities;

        public NavigationViewComponent(DbContext context)
        {
            if(entities == null)
            {
                entities = context.Model
                    .GetEntityTypes()
                    .Select(type => type.Name)
                    .ToArray();
            }
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            string selectedTable = this.ViewContext.ModelState["table"]?.RawValue as string;
            var viewModel = new NavigationViewModel(entities, selectedTable);
            return ViewResult(viewModel);
        }

        private Task<IViewComponentResult> ViewResult<T>(T model) =>
            Task.FromResult(View(model) as IViewComponentResult);
    }

    public class NavigationViewModel
    {
        public NavigationViewModel(IReadOnlyList<string> tables, string selectedTable)
        {
            SelectedTable = selectedTable;
            Tables = tables;
        }
        public string SelectedTable { get; }
        public IReadOnlyList<string> Tables { get; }
    }
}
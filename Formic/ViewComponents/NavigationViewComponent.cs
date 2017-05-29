using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formic.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        private readonly DbContext dbContext;

        public NavigationViewComponent(DbContext context)
        {
            this.dbContext = context;
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            string dbName = dbContext.GetType().Name;
            string[] tables = dbContext.Model
                .GetEntityTypes()
                .Select(type => type.Name)
                .ToArray();
            var viewModel = new NavigationViewModel(dbName, tables);
            return ViewResult(viewModel);
        }

        private Task<IViewComponentResult> ViewResult<T>(T model) =>
            Task.FromResult(View(model) as IViewComponentResult);
    }

    public class NavigationViewModel
    {
        public NavigationViewModel(string dbContextName, string[] tables)
        {
            DbContextName = dbContextName;
            Tables = tables;
        }
        public string DbContextName { get; }
        public string[] Tables { get; }
    }
}
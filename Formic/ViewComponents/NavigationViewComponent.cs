﻿using Microsoft.AspNetCore.Mvc;
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
            string[] tables = dbContext.Model
                .GetEntityTypes()
                .Select(type => type.Name)
                .ToArray();
            string selectedTable = this.ViewContext.ModelState["table"]?.RawValue as string;
            var viewModel = new NavigationViewModel(tables, selectedTable);
            return ViewResult(viewModel);
        }

        private Task<IViewComponentResult> ViewResult<T>(T model) =>
            Task.FromResult(View(model) as IViewComponentResult);
    }

    public class NavigationViewModel
    {
        public NavigationViewModel(string[] tables, string selectedTable)
        {
            SelectedTable = selectedTable;
            Tables = tables;
        }
        public string SelectedTable { get; }
        public string[] Tables { get; }
    }
}
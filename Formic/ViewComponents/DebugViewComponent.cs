using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Formic.ViewComponents
{
    public class DebugViewComponent : ViewComponent
    {
        private readonly IHostingEnvironment hostingEnvironment;

        public DebugViewComponent(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        public Task<IViewComponentResult> InvokeAsync()
        {
            return ViewResult(hostingEnvironment.IsDevelopment());
        }

        private Task<IViewComponentResult> ViewResult<T>(T model) =>
            Task.FromResult(View(model) as IViewComponentResult);
    }
}
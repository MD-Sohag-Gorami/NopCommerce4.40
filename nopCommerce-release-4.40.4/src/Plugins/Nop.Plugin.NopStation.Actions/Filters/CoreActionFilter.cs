using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Infrastructure;
using Nop.Plugin.NopStation.Core.Helpers;
using Nop.Plugin.NopStation.Core.Services;

namespace Nop.Plugin.NopStation.Core.Filters
{
    public class CoreActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
                return;

            var descriptor = ((ControllerBase)context.Controller).ControllerContext?.ActionDescriptor;
            var assembly = descriptor.ControllerTypeInfo.Assembly;
            if (!assembly.GetName().Name.StartsWith("Nop.Plugin.NopStation.", StringComparison.InvariantCultureIgnoreCase))
                return;
            if (descriptor.ControllerName.Equals("NopStationLicense", StringComparison.InvariantCultureIgnoreCase) &&
                descriptor.ActionName.Equals("License", StringComparison.InvariantCultureIgnoreCase))
                return;

            if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(assembly).Result)
            {
                var areaName = string.Empty;
                var routeData = ((ControllerBase)context.Controller).ControllerContext.RouteData;
                if (routeData != null)
                    areaName = routeData.Values["area"] as string;

                if (areaName != null && areaName.Equals("admin", StringComparison.InvariantCultureIgnoreCase))
                    context.Result = new RedirectToActionResult("License", "NopStationLicense", null);
                else
                    context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}

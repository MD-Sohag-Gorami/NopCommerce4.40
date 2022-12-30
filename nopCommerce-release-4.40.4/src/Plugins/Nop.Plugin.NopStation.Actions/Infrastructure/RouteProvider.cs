using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.NopStation.Actions.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => 111;

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //endpointRouteBuilder.MapControllerRoute("CoreLicenseFile", "Plugins/NopStation.Core/{filename}.json",
            //    new { controller = "NopStationAction", action = "JsonFile" });
        }
    }
}

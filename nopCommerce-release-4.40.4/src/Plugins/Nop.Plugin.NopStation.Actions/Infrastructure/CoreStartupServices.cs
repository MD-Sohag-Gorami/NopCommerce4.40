using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Nop.Plugin.NopStation.Core.Filters;

namespace Nop.Plugin.NopStation.Core.Infrastructure
{
    public static class CoreStartupServices
    {
        public static void AddNopStationServices(this IServiceCollection services, string systemName, bool rootAdmin = false, bool excludepublicView = false)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander(systemName, rootAdmin, excludepublicView));
            });

            services.AddMvc(configure =>
            {
                var filters = configure.Filters;
                filters.Add<CoreActionFilter>();
            });
        }
    }
}

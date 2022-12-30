using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Filters;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });
            services.AddMvc(configure =>
            {
                var filters = configure.Filters;
                filters.Add<ZimzoneActionFilter>();
            });
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => int.MaxValue;
    }
}
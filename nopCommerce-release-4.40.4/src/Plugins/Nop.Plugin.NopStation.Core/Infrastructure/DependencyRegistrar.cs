using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.NopStation.Core.Services;

namespace Nop.Plugin.NopStation.Core.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<INopStationContext, NopStationContext>();
            services.AddScoped<INopStationPluginManager, NopStationPluginManager>();
            services.AddScoped<IWorkContextPluginManager, WorkContextPluginManager>();
            services.AddScoped<INopStationCoreService, NopStationCoreService>();
        }

        public int Order => 1;
    }
}

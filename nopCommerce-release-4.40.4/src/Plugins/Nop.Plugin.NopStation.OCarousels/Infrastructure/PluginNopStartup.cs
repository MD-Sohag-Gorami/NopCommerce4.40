using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.NopStation.Core.Infrastructure;

namespace Nop.Plugin.NopStation.OCarousels.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.OCarousels");
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}
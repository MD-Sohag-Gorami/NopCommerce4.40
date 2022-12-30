using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.NopStation.OCarousels.Areas.Admin.Factories;

namespace Nop.Plugin.NopStation.OCarousels.Areas.Admin.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IOCarouselModelFactory, OCarouselModelFactory>();
        }

        public int Order => 1;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.NopStation.Core.Factories;
using Nop.Plugin.NopStation.Core.Services;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.NopStation.Core.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IPluginModelFactory, PluginModelFactoryOverride>();

            services.AddScoped<ILicenseService, LicenseService>();
            services.AddScoped<IProductAttributeParserApi, ProductAttributeParserApi>();
        }

        public int Order => int.MaxValue;
    }
}

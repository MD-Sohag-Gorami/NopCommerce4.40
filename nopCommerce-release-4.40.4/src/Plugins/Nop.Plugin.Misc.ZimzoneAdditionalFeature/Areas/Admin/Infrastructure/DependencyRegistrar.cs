using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => int.MaxValue;

        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IZimzoneServiceModelFactory, ZimzoneServiceModelFactory>();
            services.AddScoped<IZimzoneServiceEntityService, ZimzoneServiceEntityService>();
            services.AddScoped<IServiceRequestModelFactory, ServiceRequestModelFactory>();
            services.AddScoped<IProductModelFactory, OverridenAdminProductModelFactory>();
            services.AddScoped<IGiftCardModelFactory, OverridenGiftCardModelFactory>();
            services.AddScoped<ICustomGiftCardFilterService, CustomGiftCardFilterService>();
            services.AddScoped<IElectrosalesVoucherModelFactory, ElectrosalesVoucherModelFactory>();
            services.AddScoped<IProductPriceAndStockUpdateModelFactory, ProductPriceAndStockUpdateModelFactory>();

            services.AddScoped<IOrderModelFactory, OverridenAdminOrderModelFactory>();

            services.AddScoped<IShipmentPackageSlipService, ShipmentPackageSlipService>();
            services.AddScoped<ICustomOrderStatusService, CustomOrderStatusService>();
            services.AddScoped<ICustomOrderStatusModelFactory, CustomOrderStatusModelFactory>();
            services.AddTransient<OrderController, OverridenOrderController>();
            services.AddTransient<GiftCardController, OverriddenGiftCardController>();

            services.AddScoped<IOrderItemPdfDownloadService, OrderItemPdfDownloadService>();

            services.AddScoped<IZimzoneProductModelFactory, ZimzoneProductModelFactory>();
            services.AddScoped<IZimzoneProductService, ZimzoneProductService>();
        }
    }
}

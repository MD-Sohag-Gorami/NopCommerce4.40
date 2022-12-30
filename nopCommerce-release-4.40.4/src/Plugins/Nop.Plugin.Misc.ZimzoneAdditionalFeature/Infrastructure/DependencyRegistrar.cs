using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Controllers;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.Erp;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.PriceUpdate;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.Query;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Factories;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IProductAttributeParser, OverridenProductAttributeParser>();
            services.AddScoped<IShoppingCartService, OverridenShoppingCartService>();
            services.AddScoped<IGiftCardAttributeParser, GiftCardAttributeParser>();
            services.AddScoped<IProductModelFactory, OverridenProductModelFactory>();
            services.AddScoped<IGiftCardService, OverridenGiftCardService>();
            services.AddScoped<IShoppingCartModelFactory, OverridenShoppingCartModelFactory>();
            services.AddScoped<IOrderModelFactory, OverridenOrderModelFactory>();
            services.AddScoped<IPdfService, OverridenPdfService>();
            services.AddScoped<IMessageTokenProvider, OverridenMessageTokenProvider>();
            services.AddScoped<IGiftCardHistoryModelFactory, GiftCardHistoryModelFactory>();
            services.AddScoped<IWorkflowMessageService, OverridenWorkflowMessageService>();


            //ERP
            services.AddScoped<IErpPictureService, ErpPictureService>();
            services.AddScoped<IZimZoneErpSyncService, ZimZoneErpSyncService>();
            services.AddScoped<IErpCategoryService, ErpCategoryService>();

            //Service
            services.AddScoped<IServiceRequestService, ServiceRequestService>();
            services.AddScoped<IServiceRequestNotificationService, ServiceRequestNotificationService>();

            //question
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IQueryModelFactory, QueryModelFactory>();

            //price and stock update
            services.AddScoped<IZimZonePriceAndStockUpdateService, ZimZonePriceAndStockUpdateService>();

            services.AddScoped<IOrderReportService, OverriddenOrderReportService>();
            services.AddScoped<IErpManufacturerService, ErpManufacturerService>();
            services.AddScoped<ICatalogModelFactory, OverriddenCatalogModelFactory>();

            services.AddTransient<CheckoutController, OverriddenCheckoutController>();
        }

        public int Order => int.MaxValue;
    }
}

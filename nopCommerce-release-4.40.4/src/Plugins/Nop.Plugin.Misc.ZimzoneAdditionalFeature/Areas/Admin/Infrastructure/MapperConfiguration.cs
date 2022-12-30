using System;
using AutoMapper;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.ErpSync.Areas.Admin.Models;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus;

namespace Nop.Plugin.Misc.ErpSync.Infrastructure
{
    public class MapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 100;

        public MapperConfiguration()
        {
            CreateMap<Product, ErpProductModel>();

            CreateMap<ErpProductModel, Product>()
                .ForMember(model => model.Id, options => options.Ignore())
                .ForMember(model => model.Name, options => options.MapFrom(x => x.ProductName))
                .ForMember(model => model.Height, options => options.MapFrom(x => x.Height.HasValue ? x.Height.Value : 0.0M))
                .ForMember(model => model.Width, options => options.MapFrom(x => x.Depth.HasValue ? x.Depth.Value : 0.0M))
                .ForMember(model => model.Length, options => options.MapFrom(x => x.Length.HasValue ? x.Length.Value : 0.0M))
                .ForMember(model => model.Weight, options => options.MapFrom(x => x.Weight.HasValue ? x.Weight.Value : 0.0M))
                .ForMember(model => model.Published, options => options.MapFrom(x => x.Published.HasValue ? x.Published.Value : false))
                .ForMember(model => model.Price, options => options.MapFrom(x => x.Price.HasValue ? x.Price.Value : 0.0M))
                .ForMember(model => model.Sku, options => options.MapFrom(x => x.Sku))
                .ForMember(model => model.UpdatedOnUtc, options => options.MapFrom(x => DateTime.UtcNow))
                .ForMember(model => model.MinStockQuantity, options => options.MapFrom(x => 0))
                .ForMember(model => model.StockQuantity, options => options.MapFrom(x => x.StockOnHand.HasValue ? x.StockOnHand.Value : 0))
                .ForMember(model => model.ProductType, options => options.MapFrom(x => ProductType.SimpleProduct))
                .ForMember(model => model.ShortDescription, options => options.MapFrom(x => x.ShortDescription))
                .ForMember(model => model.FullDescription, options => options.MapFrom(x => x.FullDescription))
                .ForMember(model => model.VisibleIndividually, options => options.MapFrom(x => true))

                .ForMember(model => model.AllowCustomerReviews, options => options.MapFrom(x => true))


                //.ForMember(model => model.BackorderModeId, options => options.MapFrom(x => (int)BackorderMode.AllowQtyBelow0AndNotifyCustomer))


                .ForMember(model => model.LowStockActivityId, options => options.MapFrom(x => (int)LowStockActivity.Nothing))
                .ForMember(model => model.OrderMinimumQuantity, options => options.MapFrom(x => 1))
                .ForMember(model => model.OrderMaximumQuantity, options => options.MapFrom(x => 1000000))
                .ForMember(model => model.DisplayStockAvailability, options => options.MapFrom(x => true))
                .ForMember(model => model.DisplayStockQuantity, options => options.MapFrom(x => false))
                //.ForMember(model => model.TaxCategoryId, options => options.MapFrom(x => 5))
                .ForMember(model => model.IsShipEnabled, options => options.MapFrom(x => true))
                .ForMember(model => model.MarkAsNew, options => options.Ignore())
                .ForMember(model => model.MarkAsNewStartDateTimeUtc, options => options.Ignore())
                .ForMember(model => model.MarkAsNewEndDateTimeUtc, options => options.Ignore())
                //.ForMember(model => model.ProductTemplateId, options => options.MapFrom(x => x.IsBoM.Value ? 1 : 1))
                .ForMember(model => model.ManageInventoryMethod, options => options.MapFrom(x => ManageInventoryMethod.ManageStock))
                .ForMember(model => model.BackorderMode, options => options.MapFrom(x => BackorderMode.NoBackorders));

            CreateMap<ZimzoneServiceModel, ZimzoneServiceEntity>()
                .ForMember(model => model.Id, options => options.MapFrom(x => x.Id))
                .ForMember(model => model.ServiceProductId, options => options.MapFrom(x => x.ServicePaymentProductId))
                .ForMember(model => model.ServiceProductName, options => options.MapFrom(x => x.ServiceProductName))
                .ForMember(model => model.PaymentProductName, options => options.MapFrom(x => x.ServicePaymentProductName))
                .ForMember(model => model.ServicePaymentProductId, options => options.MapFrom(x => x.ServicePaymentProductId))
                .ForMember(model => model.IsActive, options => options.MapFrom(x => x.IsActive));

            CreateMap<CustomOrderStatus, CustomOrderStatusModel>();
            CreateMap<CustomOrderStatusModel, CustomOrderStatus>().ForMember(model => model.Id, options => options.Ignore());

            CreateMap<OrderWithCustomStatus, CustomStatusListWithOrderModel>();
            CreateMap<CustomStatusListWithOrderModel, OrderWithCustomStatus>().ForMember(model => model.Id, options => options.Ignore());
        }
    }
}

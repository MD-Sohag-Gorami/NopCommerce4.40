using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.ErpSync;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public class OverridenAdminProductModelFactory : ProductModelFactory
    {
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly IErpPictureService _erpPictureService;
        private readonly ErpSyncSettings _erpSyncSettings;

        public OverridenAdminProductModelFactory(CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IAddressService addressService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IManufacturerService manufacturerService,
            IMeasureService measureService,
            IOrderService orderService,
            IPictureService pictureService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            ISettingModelFactory settingModelFactory,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            IErpPictureService erpPictureService,
            ErpSyncSettings erpSyncSettings) : base(catalogSettings,
                currencySettings,
                aclSupportedModelFactory,
                addressService,
                baseAdminModelFactory,
                categoryService,
                currencyService,
                customerService,
                dateTimeHelper,
                discountService,
                discountSupportedModelFactory,
                localizationService,
                localizedModelFactory,
                manufacturerService,
                measureService,
                orderService,
                pictureService,
                productAttributeFormatter,
                productAttributeParser,
                productAttributeService,
                productService,
                productTagService,
                productTemplateService,
                settingModelFactory,
                shipmentService,
                shippingService,
                shoppingCartService,
                specificationAttributeService,
                storeMappingSupportedModelFactory,
                storeService,
                urlRecordService,
                workContext,
                measureSettings,
                taxSettings,
                vendorSettings)
        {
            _categoryService = categoryService;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _erpPictureService = erpPictureService;
            _erpSyncSettings = erpSyncSettings;
        }
        public override async Task<ProductListModel> PrepareProductListModelAsync(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter comments
            var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;
            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                warehouseId: searchModel.SearchWarehouseId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: overridePublished);

            //prepare list model
            var model = await new ProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    //fill in model values from the entity
                    var productModel = product.ToModel<ProductModel>();

                    //little performance optimization: ensure that "FullDescription" is not returned
                    productModel.FullDescription = string.Empty;

                    //fill in additional values (not existing in the entity)
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();

                    //checking if product have any image from erp
                    var erpPictures = await _erpPictureService.GetErpPicturesByNopProductIdAsync(product.Id);
                    if (erpPictures != null && erpPictures.Count > 0)
                    {
                        productModel.PictureThumbnailUrl = ErpSyncHelper.PrepareImageUrlWithSize(erpPictures.First().ImageLink, 75, _erpSyncSettings);
                    }
                    else
                    {
                        (productModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                    }
                    productModel.ProductTypeName = await _localizationService.GetLocalizedEnumAsync(product.ProductType);
                    if (product.ProductType == ProductType.SimpleProduct && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productModel.StockQuantityStr = (await _productService.GetTotalStockQuantityAsync(product)).ToString();

                    return productModel;
                });
            });

            return model;
        }
    }
}

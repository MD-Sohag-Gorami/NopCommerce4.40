using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.ErpSync;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Product;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public class ZimzoneProductModelFactory : IZimzoneProductModelFactory
    {
        private readonly IWorkContext _workContext;
        private readonly ICategoryService _categoryService;
        private readonly IZimzoneProductService _zimzoneProductService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly IErpPictureService _erpPictureService;
        private readonly ErpSyncSettings _erpSyncSettings;
        private readonly ILocalizationService _localizationService;
        private readonly VendorSettings _vendorSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly CatalogSettings _catalogSettings;
        private readonly IProductService _productService;

        public ZimzoneProductModelFactory(IWorkContext workContext,
                                          ICategoryService categoryService,
                                          IZimzoneProductService zimzoneProductService,
                                          IUrlRecordService urlRecordService,
                                          IPictureService pictureService,
                                          IErpPictureService erpPictureService,
                                          ErpSyncSettings erpSyncSettings,
                                          ILocalizationService localizationService,
                                          VendorSettings vendorSettings,
                                          IBaseAdminModelFactory baseAdminModelFactory,
                                          CatalogSettings catalogSettings,
                                          IProductService productService)
        {
            _workContext = workContext;
            _categoryService = categoryService;
            _zimzoneProductService = zimzoneProductService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _erpPictureService = erpPictureService;
            _erpSyncSettings = erpSyncSettings;
            _localizationService = localizationService;
            _vendorSettings = vendorSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _catalogSettings = catalogSettings;
            _productService = productService;
        }

        public virtual async Task<ZimzoneProductSearchModel> PrepareProductSearchModelAsync(ZimzoneProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //a vendor should have access only to his products
            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
            searchModel.AllowVendorsToImportProducts = _vendorSettings.AllowVendorsToImportProducts;

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare available product types
            await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

            //prepare available warehouses
            await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.All")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.PublishedOnly")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly")
            });

            // add stock options
            searchModel.AvailableStockOptions.Add(new SelectListItem()
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.Common.All")
            });
            searchModel.AvailableStockOptions.Add(new SelectListItem()
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.StockOptions.InStock")
            });
            searchModel.AvailableStockOptions.Add(new SelectListItem()
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.StockOptions.OutOfStock")
            });

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<ProductListModel> PrepareProductListModelAsync(ZimzoneProductSearchModel searchModel)
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
            var products = await _zimzoneProductService.SearchProductsAsync(showHidden: true,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                warehouseId: searchModel.SearchWarehouseId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: overridePublished,
                stockOptionId: searchModel.SearchStockOptionId);

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Misc.ErpSync.Areas.Admin.Models;
using Nop.Plugin.Misc.ErpSync.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Erp;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Erp;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.Erp;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Seo;
using Nop.Services.Tasks;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Misc.ErpSync.Services
{
    public class ZimZoneErpSyncService : IZimZoneErpSyncService
    {
        private readonly ILogger _logger;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IErpPictureService _erpPictureService;
        private readonly ICategoryService _categoryService;
        private readonly IErpCategoryService _erpCategoryService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ErpSyncSettings _erpSyncSettings;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IErpManufacturerService _erpManufacturerService;
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;
        private readonly GiftVoucherSettings _giftVoucherSettings;

        public ZimZoneErpSyncService(ILogger logger,
            IProductService productService,
            IUrlRecordService urlRecordService,
            IErpPictureService erpPictureService,
            ICategoryService categoryService,
            IErpCategoryService erpCategoryService,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager staticCacheManager,
            ErpSyncSettings erpSyncSettings,
            IScheduleTaskService scheduleTaskService,
            IManufacturerService manufacturerService,
            IErpManufacturerService erpManufacturerService,
            IZimzoneServiceEntityService zimzoneServiceEntityService,
            GiftVoucherSettings giftVoucherSettings)
        {
            _logger = logger;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _erpPictureService = erpPictureService;
            _categoryService = categoryService;
            _erpCategoryService = erpCategoryService;
            _specificationAttributeService = specificationAttributeService;
            _staticCacheManager = staticCacheManager;
            _erpSyncSettings = erpSyncSettings;
            _scheduleTaskService = scheduleTaskService;
            _manufacturerService = manufacturerService;
            _erpManufacturerService = erpManufacturerService;
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
            _giftVoucherSettings = giftVoucherSettings;
        }

        public async Task<bool> SyncCategoriesAsync()
        {
            var url = _erpSyncSettings.CategorySyncUrl;// "https://zimazon.lemonkode.com:3001/categories";
            var request = WebRequest.Create(url);
            try
            {
                var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                var erpCategoryModels = JsonConvert.DeserializeObject<IList<ErpCategoryModel>>(response);
                await UpdateCategoriesAsync(erpCategoryModels);
                return true;
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "error while getting categories from erp", e.Message);
                return false;
            }
        }
        public async Task<bool> SyncManufacturersAsync()
        {
            var url = _erpSyncSettings.ManufacturerSyncUrl;// "https://zimazon.lemonkode.com:3001/brands/get-brands";
            var request = WebRequest.Create(url);
            try
            {
                var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                var erpManufacturerModels = JsonConvert.DeserializeObject<IList<ErpManufacturerModel>>(response);
                await UpdateManufacturersAsync(erpManufacturerModels);
                return true;
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "error while getting manufacturers from erp", e.Message);
                return false;
            }
        }
        public async Task<bool> SyncProductsAsync(string sku = "", DateTime? lastUpdatedTime = null)
        {
            string date;
            if (lastUpdatedTime.HasValue)
            {
                var updateDate = lastUpdatedTime.Value;
                updateDate = _erpSyncSettings.BufferTime > 0 ? updateDate.AddMinutes((_erpSyncSettings.BufferTime) * -1) : updateDate;
                date = $"{updateDate:yyyy-MM-dd HH:mm}";
            }
            else
            {
                date = $"2022-01-01 00:00";
            }

            var url = _erpSyncSettings.ProductSyncUrl; //"https://zimzone.lemonkode.com:3001/products/get-updated-products";
            url += $"?date={date}";
            await _logger.InformationAsync($"date:  {date}");
            if (!string.IsNullOrEmpty(sku))
            {
                url += $"&sku={sku}";
            }
            var request = WebRequest.Create(url);
            request.Timeout = (_erpSyncSettings.RequestTimeOutInSeconds.HasValue && _erpSyncSettings.RequestTimeOutInSeconds.Value > 120) ? _erpSyncSettings.RequestTimeOutInSeconds.Value * 1000 : 120 * 1000; // converted to milliseconds
            try
            {
                var webResponse = (HttpWebResponse)(await request.GetResponseAsync());
               
                using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                var erpProductModels = JsonConvert.DeserializeObject<IList<ErpProductModel>>(response);
                await _logger.InsertLogAsync(LogLevel.Information, $"{erpProductModels.Count} product is updating");
                await UpdateProductsAsync(erpProductModels);
                return true;
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, "error while getting product from erp", e.Message);
                return false;
            }
        }



        private async Task UpdateProductsAsync(IList<ErpProductModel> erpProductModels)
        {
            var allSpecificationAttribute = (await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync()).ToDictionary(x => x.Name);
            var existingManufacturers = (await _manufacturerService.GetAllManufacturersAsync(showHidden: true)).ToList();
            foreach (var erpProductModel in erpProductModels)
            {
                if (string.IsNullOrEmpty(erpProductModel?.ProductName))
                {
                    await _logger.InsertLogAsync(LogLevel.Error, $"ERROR - ERP sync (Product name is Empty). Product SKU: {erpProductModel.Sku}");
                    continue;
                }
                try
                {
                    var nopProduct = await _productService.GetProductBySkuAsync(erpProductModel.Sku);
                    //if nop product already exist
                    if (nopProduct != null)
                    {
                        if (erpProductModel.Published.Value && !nopProduct.Published && !nopProduct.MarkAsNewEndDateTimeUtc.HasValue)
                        {
                            nopProduct.MarkAsNew = true;
                            nopProduct.MarkAsNewStartDateTimeUtc = DateTime.UtcNow;
                            nopProduct.MarkAsNewEndDateTimeUtc = nopProduct.MarkAsNewStartDateTimeUtc.Value.AddDays(14);
                        }
                        //ERP product to nop Product
                        nopProduct = erpProductModel.ToEntity(nopProduct);
                        //Update product
                        await _productService.UpdateProductAsync(nopProduct);

                    }
                    else //nop product doesn't exist create a new product
                    {
                        nopProduct = erpProductModel.ToEntity<Product>();
                        nopProduct.VisibleIndividually = true;
                        if (nopProduct.Published)
                        {
                            nopProduct.MarkAsNew = true;
                            nopProduct.MarkAsNewStartDateTimeUtc = DateTime.UtcNow;
                            nopProduct.MarkAsNewEndDateTimeUtc = nopProduct.MarkAsNewStartDateTimeUtc.Value.AddDays(14);
                        }
                        await _productService.InsertProductAsync(nopProduct);

                        //Create se name for newly created product
                        var seName = await _urlRecordService.ValidateSeNameAsync(nopProduct, string.Empty, nopProduct.Name, true);
                        await _urlRecordService.SaveSlugAsync(nopProduct, seName, 0);

                    }
                    await UpadatePicturesAsync(erpProductModel, nopProduct);
                    await UpdateProductCategoryAsync(erpProductModel, nopProduct);
                    await UpdateSpecificationAttributeAsync(erpProductModel, nopProduct, allSpecificationAttribute);

                    //updating product manufacturer

                    if (!string.IsNullOrEmpty(erpProductModel.ProductBrand) && !string.IsNullOrWhiteSpace(erpProductModel.ProductBrand) && !string.IsNullOrEmpty(erpProductModel.BrandId) && !string.IsNullOrWhiteSpace(erpProductModel.BrandId))
                    {
                        await UpdateProductManufacturerAsync(erpProductModel.BrandId, erpProductModel.ProductBrand.Trim(), nopProduct);
                    }
                    else
                    {
                        await UpdateProductManufacturerAsync(string.Empty, string.Empty, nopProduct);
                    }
                }
                catch (Exception e)
                {
                    await _logger.InsertLogAsync(LogLevel.Error, $"ERROR - ERP sync products. Product SKU: {erpProductModel.Sku}", e.Message);
                }
            }

            var key = NopCatalogDefaults.ProductSpecificationAttributeByProductCacheKey;
            await _staticCacheManager.RemoveAsync(key);
        }

        private async Task UpdateProductCategoryAsync(ErpProductModel erpProductModel, Product nopProduct)
        {
            try
            {
                var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(nopProduct.Id);
                var existingNopCategoryIds = productCategories.Select(x => x.CategoryId).ToList();
                var existingErpCategoryIds = await _erpCategoryService.GetErpCategoryIdsByNopCategoryIdsAsync(existingNopCategoryIds);
                var currentErpCategoryIds = erpProductModel.Categories;

                var addList = currentErpCategoryIds.Except(existingErpCategoryIds).ToList();
                var removeList = existingErpCategoryIds.Except(currentErpCategoryIds).ToList();
                var nopCategoryIdAddList = await _erpCategoryService.GetNopCategoryIdsByErpCategoryIdsAsync(addList);
                var nopCategoryIdRemoveList = await _erpCategoryService.GetNopCategoryIdsByErpCategoryIdsAsync(removeList);
                foreach (var categoryId in nopCategoryIdAddList)
                {
                    await _categoryService.InsertProductCategoryAsync(new ProductCategory
                    {
                        CategoryId = categoryId,
                        ProductId = nopProduct.Id
                    });
                }
                var toRemoveProductCategories = productCategories.Where(x => nopCategoryIdRemoveList.Contains(x.CategoryId));
                foreach (var productCategory in toRemoveProductCategories)
                {
                    await _categoryService.DeleteProductCategoryAsync(productCategory);
                }
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"ERROR - ERP sync categories. Product SKU: {erpProductModel.Sku}", e.Message);

            }

        }
        private async Task UpdateSpecificationAttributeAsync(ErpProductModel erpProductModel, Product nopProduct, Dictionary<string, SpecificationAttribute> allSpecificationAttribute)
        {
            try
            {
                var currenctSpecifications = erpProductModel.Specifications;
                if (currenctSpecifications != null && currenctSpecifications.Count > 0)
                {
                    var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync();
                    var currentOptions = currenctSpecifications.Select(x => x.Value).ToList();
                    foreach (var erpSpecification in currenctSpecifications)
                    {
                        if (string.IsNullOrEmpty(erpSpecification?.Name) || string.IsNullOrEmpty(erpSpecification?.Value))
                        {
                            continue;
                        }
                        await AddAttributesOptionsAsync(erpSpecification, productSpecificationAttributes, allSpecificationAttribute, currentOptions, nopProduct);
                    }
                }
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"ERROR - ERP sync specifications. Product SKU: {erpProductModel.Sku}", e.Message);
            }

            //TODO
            await Task.CompletedTask;
        }
        protected async Task AddAttributesOptionsAsync(Specification erpSpecification, IList<ProductSpecificationAttribute> productSpecificationAttributes,
           Dictionary<string, SpecificationAttribute> allSpecificationAttribute, IList<string> currentOptions, Product product)
        {
            try
            {
                var attribute = allSpecificationAttribute.ContainsKey(erpSpecification.Name) ? allSpecificationAttribute[erpSpecification.Name] : null;

                if (attribute == null)
                {
                    //specification attribute doesn't exist have to create one with supplier name
                    attribute = new SpecificationAttribute
                    {
                        Name = erpSpecification.Name,
                        DisplayOrder = 1,
                    };
                    //insert specificationAttribute
                    await _specificationAttributeService.InsertSpecificationAttributeAsync(attribute);
                    //add to all specification attribute list so that we don't create it again.
                    allSpecificationAttribute.Add(attribute.Name, attribute);
                }


                ////attribute option
                var specificationAttributeOptions = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(attribute.Id);
                var specificationAttributeOption = specificationAttributeOptions.Where(x => x.Name.Equals(erpSpecification.Value)).FirstOrDefault();
                if (specificationAttributeOption == null)
                {
                    //option doesn't exist
                    //Create a SpecificationAttributeOption
                    specificationAttributeOption = new SpecificationAttributeOption
                    {
                        SpecificationAttributeId = attribute.Id,
                        Name = erpSpecification.Value,
                        ColorSquaresRgb = null
                    };
                    //insert option
                    await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(specificationAttributeOption);
                    specificationAttributeOptions.Add(specificationAttributeOption);
                }
                var productSpecificationAttribute = productSpecificationAttributes.Where(x => x.SpecificationAttributeOptionId == specificationAttributeOption.Id && x.ProductId == product.Id).FirstOrDefault();

                if (productSpecificationAttribute == null)
                {
                    //specification attribute add model(to map with product) if not exist
                    var addSpecificationAttributeModel = new AddSpecificationAttributeModel
                    {
                        AttributeId = attribute.Id,
                        AttributeTypeId = (int)SpecificationAttributeType.Option,
                        ProductId = product.Id,
                        AllowFiltering = true,
                        ValueRaw = null,
                        SpecificationAttributeOptionId = specificationAttributeOption.Id
                    };
                    var psa = addSpecificationAttributeModel.ToEntity<ProductSpecificationAttribute>();
                    await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psa);
                    productSpecificationAttributes.Add(psa);
                }

                //others options that doesnt exist in current option list
                var othersOptionsIds = specificationAttributeOptions.Where(x => !currentOptions.Contains(x.Name)).Select(x => x.Id);
                foreach (var optionId in othersOptionsIds)
                {
                    //option mapping that is currently not available with this product.
                    var toRemovePsa = productSpecificationAttributes.Where(x => x.SpecificationAttributeOptionId == optionId && x.ProductId == product.Id).FirstOrDefault();
                    if (toRemovePsa != null)
                    {
                        //delete non existing mappings
                        await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(toRemovePsa);
                        productSpecificationAttributes.Remove(toRemovePsa);
                        var currentPSACount = productSpecificationAttributes.Where(x => x.SpecificationAttributeOptionId == optionId && x.ProductId == product.Id).Count();
                        if (currentPSACount == 0)
                        {
                            var toRemoveOption = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(optionId);
                            await _specificationAttributeService.DeleteSpecificationAttributeOptionAsync(toRemoveOption);
                            specificationAttributeOptions.Remove(toRemoveOption);
                        }
                        var optionsCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.SpecificationAttributeOptionsCacheKey, attribute.Id);
                        await _staticCacheManager.RemoveAsync(optionsCacheKey);
                    }

                }

            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"ERROR - ERP sync. Product SKU: {product?.Sku}", e.Message);
            }
        }
        private async Task UpadatePicturesAsync(ErpProductModel erpProductModel, Product nopProduct)
        {
            try
            {
                var existingPictures = await _erpPictureService.GetErpPicturesBySkuAsync(erpProductModel.Sku);
                var existingPicturePaths = existingPictures.Select(x => x.ImageLink);
                var oldPicturePaths = new List<string>();
                var newPicturePaths = new List<string>();
                foreach (var imagePath in erpProductModel.ImagePaths)
                {
                    newPicturePaths.Add(imagePath);
                }
                foreach (var picturePath in existingPicturePaths)
                {
                    oldPicturePaths.Add(picturePath);
                }
                var addList = newPicturePaths.Except(oldPicturePaths);
                var removePaths = oldPicturePaths.Except(newPicturePaths);
                var removeList = existingPictures.Where(x => removePaths.Contains(x.ImageLink));
                foreach (var item in removeList)
                {
                    await _erpPictureService.DeleteErpProductPictureAsync(item);
                }

                foreach (var item in addList)
                {
                    try
                    {
                        await _erpPictureService.InsertErpProductPictureAsync(new Domains.ErpProductPicture
                        {
                            ImageLink = item,
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow,
                            Sku = erpProductModel.Sku,
                            NopProductId = nopProduct.Id
                        });
                    }
                    catch (Exception e)
                    {
                        await _logger.ErrorAsync(e.ToString());
                    }
                }
                var updateableErpPictures = existingPictures.Where(x => x.NopProductId != nopProduct.Id);
                foreach (var updateableErpPicture in updateableErpPictures)
                {
                    updateableErpPicture.NopProductId = nopProduct.Id;
                    await _erpPictureService.UpdateErpProductPictureAsync(updateableErpPicture);
                }
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"ERROR - ERP sync pictures. Product SKU: {erpProductModel.Sku}", e.Message);
            }

        }

        private async Task UpdateCategoriesAsync(IList<ErpCategoryModel> erpCategoryModels)
        {
            var existingErpCategories = await _erpCategoryService.GetAllErpCategoriesAsync();
            var existingNopCategories = await _categoryService.GetAllCategoriesAsync();

            foreach (var erpCategoryModel in erpCategoryModels)
            {
                try
                {
                    var erpCategory = existingErpCategories.Where(x => x.ErpId == erpCategoryModel.Id).FirstOrDefault();
                    if (erpCategory != null)
                    {
                        erpCategory.Name = erpCategoryModel.Name;
                        erpCategory.ParentId = erpCategoryModel.ParentId;
                        erpCategory.RootCategory = erpCategoryModel.RootCategory;
                        erpCategory.V = erpCategoryModel.V;
                        erpCategory.LowestLevelCategory = erpCategoryModel.LowestLevelCategory;

                        var nopCategory = await _categoryService.GetCategoryByIdAsync(erpCategory.NopCategoryId);


                        if (nopCategory == null || (nopCategory != null && nopCategory.Deleted))
                        {
                            if (!erpCategoryModel.Name.Equals(ErpSyncDefaults.DefaultRootCategoryName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                nopCategory = await CreateNopCategoryAsync(nopCategory, erpCategoryModel, existingNopCategories);
                                erpCategory.NopCategoryId = nopCategory.Id;
                            }
                        }
                        if (nopCategory != null && nopCategory.Name != erpCategory.Name)
                        {
                            nopCategory.Name = erpCategory.Name;
                            await _categoryService.UpdateCategoryAsync(nopCategory);
                            var existingNopCategory = existingNopCategories.Where(x => x.Id == erpCategory.NopCategoryId).FirstOrDefault();
                            existingNopCategory.Name = erpCategory.Name;
                        }

                        await _erpCategoryService.UpdateErpCategoryAsync(erpCategory);
                    }
                    else
                    {
                        var nopCategory = existingNopCategories.Where(x => x.Name.Equals(erpCategoryModel.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (nopCategory == null)
                        {
                            nopCategory = await CreateNopCategoryAsync(nopCategory, erpCategoryModel, existingNopCategories);
                        }

                        erpCategory = new ErpCategory
                        {
                            ErpId = erpCategoryModel.Id,
                            LowestLevelCategory = erpCategoryModel.LowestLevelCategory,
                            Name = erpCategoryModel.Name,
                            ParentId = erpCategoryModel.ParentId,
                            RootCategory = erpCategoryModel.RootCategory,
                            V = erpCategoryModel.V,
                            NopCategoryId = nopCategory.Id
                        };
                        await _erpCategoryService.InsertErpCategoryAsync(erpCategory);
                        existingErpCategories.Add(erpCategory);
                    }
                }
                catch (Exception e)
                {
                    await _logger.InsertLogAsync(LogLevel.Error, $"ERROR - ERP sync categories. Product SKU: {erpCategoryModel?.Name}", e.Message);
                }
            }
            existingErpCategories = await _erpCategoryService.GetAllErpCategoriesAsync();
            foreach (var erpCategory in existingErpCategories)
            {
                var nopCategory = await _categoryService.GetCategoryByIdAsync(erpCategory.NopCategoryId);
                if (nopCategory != null && !string.IsNullOrEmpty(erpCategory.ParentId))
                {
                    var parentCategory = existingErpCategories.Where(x => x.ErpId == erpCategory.ParentId).FirstOrDefault();
                    var nopParentCategoryId = parentCategory?.NopCategoryId;
                    if (parentCategory?.Name != ErpSyncDefaults.DefaultRootCategoryName && nopParentCategoryId.HasValue)
                    {
                        nopCategory.ParentCategoryId = nopParentCategoryId.Value;
                    }
                    else if (parentCategory?.Name == ErpSyncDefaults.DefaultRootCategoryName && nopParentCategoryId.HasValue) // parent category- ECOMMERCE
                    {
                        nopCategory.ParentCategoryId = 0;
                    }

                    await _categoryService.UpdateCategoryAsync(nopCategory);
                }

            }

            //remove non existing erp categories

            var existingCategoriesErpId = existingErpCategories.Select(x => x.ErpId);
            var newCategoriesErpIds = erpCategoryModels.Select(x => x.Id);

            var removeIds = existingCategoriesErpId.Except(newCategoriesErpIds).ToList();
            await _erpCategoryService.DeleteErpCategoriesAsync(removeIds);
        }

        public async Task UpdateManufacturersAsync(IList<ErpManufacturerModel> erpManufacturerModels)
        {
            var existingErpManufacturers = await _erpManufacturerService.GetAllErpManufacturersAsync();
            var existingNopManufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true);

            foreach (var erpManufacturerModel in erpManufacturerModels)
            {
                try
                {
                    var erpManufacturer = existingErpManufacturers.Where(x => x.ErpManufacturerId == erpManufacturerModel.Id).FirstOrDefault();
                    if (erpManufacturer != null)
                    {
                        erpManufacturer.ImageLink = erpManufacturerModel.ImagePath;
                        erpManufacturer.ModifiedOn = erpManufacturerModel.UpdatedAt;
                        erpManufacturer.BrandName = erpManufacturerModel.BrandName;
                        erpManufacturer.Active = erpManufacturerModel.Active;

                        var nopManufacturer = await _manufacturerService.GetManufacturerByIdAsync(erpManufacturer.NopManufacturerId);


                        if (nopManufacturer == null || (nopManufacturer != null && nopManufacturer.Deleted))
                        {
                            nopManufacturer = await CreateNopManufacturerAsync(nopManufacturer, erpManufacturerModel, existingNopManufacturers);
                            erpManufacturer.NopManufacturerId = nopManufacturer.Id;
                        }
                        if (nopManufacturer != null)
                        {
                            nopManufacturer.Name = erpManufacturer.BrandName;
                            nopManufacturer.Published = erpManufacturer.Active;
                            await _manufacturerService.UpdateManufacturerAsync(nopManufacturer);

                            var existingNopManufacturer = existingNopManufacturers.Where(x => x.Id == erpManufacturer.NopManufacturerId).FirstOrDefault();
                            existingNopManufacturer.Name = erpManufacturer.BrandName;
                            existingNopManufacturer.Published = erpManufacturer.Active;
                        }

                        await _erpManufacturerService.UpdateErpManufacturerAsync(erpManufacturer);
                    }
                    else
                    {
                        var nopManufacturer = existingNopManufacturers.Where(x => x.Name.Equals(erpManufacturerModel.BrandName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                        if (nopManufacturer == null)
                        {
                            nopManufacturer = await CreateNopManufacturerAsync(nopManufacturer, erpManufacturerModel, existingNopManufacturers);
                        }

                        erpManufacturer = new ErpManufacturer
                        {
                            ErpManufacturerId = erpManufacturerModel.Id,
                            BrandName = erpManufacturerModel.BrandName,
                            Active = erpManufacturerModel.Active,
                            NopManufacturerId = nopManufacturer.Id,
                            ImageLink = erpManufacturerModel.ImagePath,
                            CreatedOn = erpManufacturerModel.UpdatedAt
                        };
                        await _erpManufacturerService.InsertErpManufacturerAsync(erpManufacturer);
                        existingErpManufacturers.Add(erpManufacturer);
                    }
                }
                catch (Exception e)
                {

                }
            }
            var existingManufacturersErpId = existingErpManufacturers.Select(x => x.ErpManufacturerId);
            var newManufacturersErpIds = erpManufacturerModels.Select(x => x.Id);

            var removeIds = existingManufacturersErpId.Except(newManufacturersErpIds).ToList();
            await _erpManufacturerService.DeleteErpManufacturersAsync(removeIds);
        }

        private async Task<Category> CreateNopCategoryAsync(Category nopCategory, ErpCategoryModel erpCategoryModel, IList<Category> existingNopCategories)
        {
            nopCategory = new Category
            {
                Name = erpCategoryModel.Name,
                Published = true,
                PageSize = 15,
                ParentCategoryId = 0,
                CreatedOnUtc = DateTime.UtcNow,
                PageSizeOptions = "15,9,30,50,100",
                UpdatedOnUtc = DateTime.UtcNow,
                AllowCustomersToSelectPageSize = true,
                IncludeInTopMenu = true
            };
            await _categoryService.InsertCategoryAsync(nopCategory);
            existingNopCategories.Add(nopCategory);

            //Create se name for newly created category
            var seName = await _urlRecordService.ValidateSeNameAsync(nopCategory, string.Empty, nopCategory.Name, true);
            await _urlRecordService.SaveSlugAsync(nopCategory, seName, 0);

            return nopCategory;
        }

        private async Task<Manufacturer> CreateNopManufacturerAsync(Manufacturer nopManufacturer, ErpManufacturerModel erpManufacturerModel, IList<Manufacturer> existingNopManufacturers)
        {
            nopManufacturer = new Manufacturer
            {
                Name = erpManufacturerModel.BrandName,
                Published = erpManufacturerModel.Active,
                PageSize = 15,
                CreatedOnUtc = DateTime.UtcNow,
                PageSizeOptions = "15,9,30,50,100",
                UpdatedOnUtc = DateTime.UtcNow,
                AllowCustomersToSelectPageSize = true,
            };
            await _manufacturerService.InsertManufacturerAsync(nopManufacturer);
            existingNopManufacturers.Add(nopManufacturer);

            //Create se name for newly created category
            var seName = await _urlRecordService.ValidateSeNameAsync(nopManufacturer, string.Empty, nopManufacturer.Name, true);
            await _urlRecordService.SaveSlugAsync(nopManufacturer, seName, 0);

            return nopManufacturer;
        }


        protected async Task UpdateProductManufacturerAsync(string brandId, string manufacturerName, Product product)
        {
            // if the erp product contains no brandId then the existing product should have no manufacturer
            if (string.IsNullOrEmpty(manufacturerName) || string.IsNullOrEmpty(brandId))
            {
                var pms = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);
                foreach (var pm in pms)
                {
                    await _manufacturerService.DeleteProductManufacturerAsync(pm);
                }
                return;
            }

            var nopManufacturer = await _erpManufacturerService.GetNopManufacturerByErpManufacturerIdAsync(brandId);

            if (nopManufacturer != null)
            {
                var productManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);
                var productManufacturer = productManufacturers.Where(x => x.ManufacturerId == nopManufacturer.Id).FirstOrDefault();
                if (productManufacturer == null)
                {
                    productManufacturer = new ProductManufacturer
                    {
                        ProductId = product.Id,
                        ManufacturerId = nopManufacturer.Id,
                    };
                    await _manufacturerService.InsertProductManufacturerAsync(productManufacturer);
                    productManufacturers.Insert(0, productManufacturer);
                }
                if (productManufacturers.Count > 1)
                {
                    for (var i = 1; i < productManufacturers.Count; i++)
                    {
                        await _manufacturerService.DeleteProductManufacturerAsync(productManufacturers[i]);
                    }
                }
            }
        }
        public async Task SyncAsync(DateTime? lastUpdatedTime = null)
        {
            await _logger.InsertLogAsync(LogLevel.Information, "Erp sync started");

            _ = await SyncCategoriesAsync();

            _ = await SyncManufacturersAsync();

            _ = await SyncProductsAsync(lastUpdatedTime: lastUpdatedTime);

            await _logger.InsertLogAsync(LogLevel.Information, "Erp sync successfull");
        }

        public async Task<int> SyncErpStockAsync(string sku)
        {
            var url = _erpSyncSettings.StockSyncUrl;// "https://zimazon.lemonkode.com:3001/categories";
            url += sku;
            var request = WebRequest.Create(url);
            try
            {
                var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                var erpStockModel = JsonConvert.DeserializeObject<ErpStockModel>(response);
                if (erpStockModel == null)
                {
                    return 0;
                }
                return await UpdateStockAsync(erpStockModel);
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "error while getting stock from erp", e.Message);
            }
            return 0;
        }

        public async Task SyncErpStocksAsync(IList<string> skus)
        {
            var url = _erpSyncSettings.StockSyncUrl;// "https://zimazon.lemonkode.com:3001/categories";
            var request = WebRequest.Create(url);
            request.Method = "POST";
            request.Headers.Add("Content-Type", "application/json");
            var json = JsonConvert.SerializeObject(new ErpProductsStockRequestBody
            {
                Skus = skus
            });
            request.ContentLength = json.Length;
            using (var webStream = request.GetRequestStream())
            {
                using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                requestWriter.Write(json);
            }
            try
            {
                var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                var erpStockModels = JsonConvert.DeserializeObject<IList<ErpStockModel>>(response);
                if (erpStockModels != null)
                {
                    foreach (var erpStockModel in erpStockModels)
                    {
                        await UpdateStockAsync(erpStockModel);
                    }
                }
            }
            catch (Exception e)
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "error while getting stocks from erp", e.Message);
            }

        }
        protected async Task<int> UpdateStockAsync(ErpStockModel erpStock)
        {
            var product = await _productService.GetProductBySkuAsync(erpStock.Sku);
            if (product != null && erpStock.StockOnHand.HasValue)
            {
                product.StockQuantity = erpStock.StockOnHand.Value;
                await _productService.UpdateProductAsync(product);
                return product.StockQuantity;
            }
            return product.StockQuantity;
        }

        public async Task<Product> SyncErpStockAsync(Product product)
        {
            var paymentProducts = await _zimzoneServiceEntityService.GetAllPaymentProductIdAsync();
            if (product.VendorId > 0 || paymentProducts.Contains(product.Id) || product.Sku == _giftVoucherSettings.ZimazonGiftProductSku || product.Sku == _giftVoucherSettings.ElectrosalesGiftProductSku)
            {
                return await Task.FromResult(product);
            }
            var currentStock = await SyncErpStockAsync(product.Sku);
            if (currentStock == 0)
                return await Task.FromResult(product);
            product.StockQuantity = currentStock;
            return await Task.FromResult(product);
        }
    }
}

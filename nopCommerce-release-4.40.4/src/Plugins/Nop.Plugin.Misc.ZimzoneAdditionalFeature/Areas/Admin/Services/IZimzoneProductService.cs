﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services
{
    public interface IZimzoneProductService
    {
        Task<IPagedList<Product>> SearchProductsAsync(int pageIndex = 0, int pageSize = int.MaxValue, IList<int> categoryIds = null, IList<int> manufacturerIds = null, int storeId = 0, int vendorId = 0, int warehouseId = 0, ProductType? productType = null, bool visibleIndividuallyOnly = false, bool excludeFeaturedProducts = false, decimal? priceMin = null, decimal? priceMax = null, int productTagId = 0, string keywords = null, bool searchDescriptions = false, bool searchManufacturerPartNumber = true, bool searchSku = true, bool searchProductTags = false, int languageId = 0, IList<SpecificationAttributeOption> filteredSpecOptions = null, ProductSortingEnum orderBy = ProductSortingEnum.Position, bool showHidden = false, bool? overridePublished = null, int stockOptionId = 0);
    }
}
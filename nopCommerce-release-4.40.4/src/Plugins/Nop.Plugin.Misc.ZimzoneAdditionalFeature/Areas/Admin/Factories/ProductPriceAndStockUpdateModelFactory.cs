using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.PriceUpdate;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.PriceUpdate;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public class ProductPriceAndStockUpdateModelFactory : IProductPriceAndStockUpdateModelFactory
    {
        private readonly IRepository<ProductUpdateExcelImportLog> _priceUpdateLogRepository;

        public ProductPriceAndStockUpdateModelFactory(IRepository<ProductUpdateExcelImportLog> priceUpdateLogRepository)
        {
            _priceUpdateLogRepository = priceUpdateLogRepository;
        }
        public async Task<PriceUpdateErrorListModel> PrepareProductUpdateErrorListModelAsync(PriceUpdateErrorSearchModel searchModel)
        {
            var query = _priceUpdateLogRepository.Table;
            query = query.Where(x => x.VendorId == searchModel.VendorId);
            var productPriceAndStockUpdateErrorList = await query.ToPagedListAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            var model = await new PriceUpdateErrorListModel().PrepareToGridAsync(searchModel, productPriceAndStockUpdateErrorList, () =>
            {
                return productPriceAndStockUpdateErrorList.SelectAwait(async x =>
                {
                    var modelItem = new PriceUpdateErrorModel
                    {
                        Id = x.Id,
                        Price = x.Price,
                        Stock = x.Stock,
                        Sku = x.Sku,
                        VendorId = x.VendorId,
                        ErrorMessage = x.ErrorMessage
                    };
                    return await Task.FromResult(modelItem);
                });
            });

            return model;
        }
    }
}

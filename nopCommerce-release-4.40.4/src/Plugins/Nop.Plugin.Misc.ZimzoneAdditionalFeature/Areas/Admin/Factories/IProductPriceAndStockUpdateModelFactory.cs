using System.Threading.Tasks;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.PriceUpdate;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public interface IProductPriceAndStockUpdateModelFactory
    {
        Task<PriceUpdateErrorListModel> PrepareProductUpdateErrorListModelAsync(PriceUpdateErrorSearchModel searchModel);
    }
}
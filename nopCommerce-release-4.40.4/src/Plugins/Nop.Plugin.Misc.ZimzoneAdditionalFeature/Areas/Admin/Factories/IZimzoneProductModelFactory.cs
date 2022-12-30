using System.Threading.Tasks;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Product;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public interface IZimzoneProductModelFactory
    {
        Task<ProductListModel> PrepareProductListModelAsync(ZimzoneProductSearchModel searchModel);
        Task<ZimzoneProductSearchModel> PrepareProductSearchModelAsync(ZimzoneProductSearchModel searchModel);
    }
}
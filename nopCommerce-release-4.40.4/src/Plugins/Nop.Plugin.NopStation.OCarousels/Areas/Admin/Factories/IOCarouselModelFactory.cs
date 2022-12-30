using System.Threading.Tasks;
using Nop.Plugin.NopStation.OCarousels.Areas.Admin.Models;
using Nop.Plugin.NopStation.OCarousels.Domains;

namespace Nop.Plugin.NopStation.OCarousels.Areas.Admin.Factories
{
    public interface IOCarouselModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();

        Task<OCarouselSearchModel> PrepareOCarouselSearchModelAsync(OCarouselSearchModel searchModel);

        Task<OCarouselListModel> PrepareOCarouselListModelAsync(OCarouselSearchModel searchModel);

        Task<OCarouselModel> PrepareOCarouselModelAsync(OCarouselModel model, OCarousel carousel, 
            bool excludeProperties = false);

        Task<OCarouselItemListModel> PrepareOCarouselItemListModelAsync(OCarouselItemSearchModel searchModel, OCarousel carousel);

        Task<AddProductToCarouselSearchModel> PrepareAddProductToOCarouselSearchModelAsync(AddProductToCarouselSearchModel searchModel);

        Task<AddProductToCarouselListModel> PrepareAddProductToOCarouselListModelAsync(AddProductToCarouselSearchModel searchModel);
    }
}
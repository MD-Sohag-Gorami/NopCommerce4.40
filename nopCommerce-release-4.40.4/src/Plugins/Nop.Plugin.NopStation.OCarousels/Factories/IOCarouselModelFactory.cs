using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.NopStation.OCarousels.Domains;
using Nop.Plugin.NopStation.OCarousels.Models;

namespace Nop.Plugin.NopStation.OCarousels.Factories
{
    public partial interface IOCarouselModelFactory
    {
        Task<OCarouselListModel> PrepareCarouselListModelAsync(IList<OCarousel> carousels);

        Task<OCarouselModel> PrepareCarouselModelAsync(OCarousel carousel);
    }
}

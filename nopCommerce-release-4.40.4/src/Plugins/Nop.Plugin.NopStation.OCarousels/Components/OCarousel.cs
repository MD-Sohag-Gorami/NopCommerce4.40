using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.NopStation.Core.Components;
using Nop.Plugin.NopStation.OCarousels.Factories;
using Nop.Plugin.NopStation.OCarousels.Helpers;
using Nop.Plugin.NopStation.OCarousels.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.NopStation.OCarousels.Components
{
    public class OCarouselViewComponent : NopStationViewComponent
    {
        private readonly IStoreContext _storeContex;
        private readonly IOCarouselService _carouselService;
        private readonly IOCarouselModelFactory _carouselModelFactory;
        private readonly OCarouselSettings _carouselSettings;

        public OCarouselViewComponent(IStoreContext storeContext,
            IOCarouselModelFactory carouselModelFactory,
            IOCarouselService carouselService,
            OCarouselSettings carouselSettings)
        {
            _storeContex = storeContext;
            _carouselModelFactory = carouselModelFactory;
            _carouselService = carouselService;
            _carouselSettings = carouselSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_carouselSettings.EnableOCarousel)
                return Content("");

            if (!OCarouselHelper.TryGetWidgetZoneId(widgetZone, out int widgetZoneId))
                return Content("");

            var carousels = (await _carouselService.GetAllCarouselsAsync(new List<int> { widgetZoneId }, storeId: (await _storeContex.GetCurrentStoreAsync()).Id, active: true)).ToList();
            if (!carousels.Any())
                return Content("");

            var model = await _carouselModelFactory.PrepareCarouselListModelAsync(carousels);

            return View(model);
        }
    }
}

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models;
using Nop.Services.Directory;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Components
{
    public class ChangeCurrencyUsdButtonViewComponent : NopViewComponent
    {
        private readonly ICurrencyService _currencyService;

        public ChangeCurrencyUsdButtonViewComponent(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = (ChangeCurrencyUsdMessageModel)additionalData;
            var usdCurrency = _currencyService.GetAllCurrenciesAsync().Result.Where(x => x.CurrencyCode == (GiftVoucherDefaults.USD_CURRENCY_CODE)).FirstOrDefault();
            if (usdCurrency == null)
            {
                return Content(string.Empty);
            }
            model.DestinationCurrencyId = usdCurrency.Id;
            return View(model);
        }
    }
}

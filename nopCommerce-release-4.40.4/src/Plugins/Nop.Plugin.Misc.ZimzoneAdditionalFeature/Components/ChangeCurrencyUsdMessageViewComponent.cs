using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Components
{
    public class ChangeCurrencyUsdMessageViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var productId = (int)additionalData;
            var model = new ChangeCurrencyUsdMessageModel
            {
                ProductId = productId
            };
            return View(model);
        }
    }
}

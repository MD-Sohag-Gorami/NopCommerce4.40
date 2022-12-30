using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Components
{
    public class GiftCardCustomerNavigationViewComponent : NopViewComponent
    {
        public GiftCardCustomerNavigationViewComponent()
        {

        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View();
        }
    }
}

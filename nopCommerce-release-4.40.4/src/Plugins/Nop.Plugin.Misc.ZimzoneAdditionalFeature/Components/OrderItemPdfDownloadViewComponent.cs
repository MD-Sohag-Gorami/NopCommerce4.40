using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Components
{
    public class OrderItemPdfDownloadViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var orderId = 0;
            if (additionalData != null)
            {
                orderId = additionalData is OrderModel x ? x.Id : 0;
            }
            return View(orderId);
        }
    }
}

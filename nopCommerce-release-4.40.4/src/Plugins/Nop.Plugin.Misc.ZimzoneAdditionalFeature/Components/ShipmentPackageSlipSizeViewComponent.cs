using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Components
{
    public class ShipmentPackageSlipSizeViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var shipmentId = 0;
            if (additionalData != null)
            {
                shipmentId = additionalData is ShipmentModel x ? x.Id : 0;
            }
            return View(shipmentId);
        }
    }
}

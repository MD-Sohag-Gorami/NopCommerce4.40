using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Components
{
    public class CustomOrderStatusViewComponent : NopViewComponent
    {
        private readonly ICustomOrderStatusModelFactory _customOrderStatusModelFactory;

        public CustomOrderStatusViewComponent(ICustomOrderStatusModelFactory customOrderStatusModelFactory)
        {
            _customOrderStatusModelFactory = customOrderStatusModelFactory;
        }
        public async Task<IViewComponentResult> InvokeAsync(string widgetzone, object additionalData)
        {
            var model = new CustomStatusListWithOrderModel();
            model.IsMarkedToSendEmail = false;
            if (additionalData != null)
            {
                var order = additionalData is OrderModel x ? x : null;
                if (order != null)
                {
                    model.ParentOrderStatusId = order.OrderStatusId;
                    model.OrderId = order.Id;

                    model = await _customOrderStatusModelFactory.PrepareCustomOrderStatusListAsync(model);
                }
            }
            return View(model);
        }
    }
}

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Filters
{
    public class ZimzoneActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
                return;

            var descriptor = ((ControllerBase)context.Controller).ControllerContext?.ActionDescriptor;
            if (descriptor.ControllerName.Equals("GiftCard", StringComparison.InvariantCultureIgnoreCase) &&
                descriptor.ActionName.Equals("Edit", StringComparison.InvariantCultureIgnoreCase) &&
                descriptor.RouteValues["area"].Equals("Admin", StringComparison.InvariantCultureIgnoreCase))
            {

                if (context.HttpContext.Request.RouteValues.ContainsKey("id") &&
                    int.TryParse(context.HttpContext.Request.RouteValues["id"].ToString(), out var giftCardId) &&
                    giftCardId != 0)
                {
                    var giftCardService = EngineContext.Current.Resolve<IGiftCardService>();
                    var giftVoucherSettings = EngineContext.Current.Resolve<GiftVoucherSettings>();
                    var productService = EngineContext.Current.Resolve<IProductService>();
                    var orderService = EngineContext.Current.Resolve<IOrderService>();

                    var giftCard = giftCardService.GetGiftCardByIdAsync(giftCardId).Result;
                    var orderItem = orderService.GetOrderItemByIdAsync(giftCard?.PurchasedWithOrderItemId ?? 0).Result;
                    var product = productService.GetProductByIdAsync(orderItem?.ProductId ?? 0).Result;
                    if (product != null && product.Sku == giftVoucherSettings.ElectrosalesGiftProductSku)
                    {
                        context.Result = new RedirectToActionResult("Edit", "ElectrosalesVoucher", new { id = giftCardId });
                    }
                }
            }

            else if (descriptor.ControllerName.Equals("Order", StringComparison.InvariantCultureIgnoreCase) &&
                descriptor.ActionName.Equals("List", StringComparison.InvariantCultureIgnoreCase) &&
                descriptor.RouteValues["area"].Equals("Admin", StringComparison.InvariantCultureIgnoreCase))
            {
                context.Result = new RedirectToActionResult("OrderWithCustomStatusList", "CustomOrderStatus", null);
            }

            else if (descriptor.ControllerName.Equals("Product", StringComparison.InvariantCultureIgnoreCase) &&
                descriptor.ActionName.Equals("List", StringComparison.InvariantCultureIgnoreCase) &&
                descriptor.RouteValues["area"].Equals("Admin", StringComparison.InvariantCultureIgnoreCase))
            {
                context.Result = new RedirectToActionResult("List", "ZimzoneProductSearch", null);
            }
        }
    }

}

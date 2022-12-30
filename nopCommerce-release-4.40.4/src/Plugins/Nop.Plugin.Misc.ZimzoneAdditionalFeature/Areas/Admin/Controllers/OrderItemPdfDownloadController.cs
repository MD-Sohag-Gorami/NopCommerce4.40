using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class OrderItemPdfDownloadController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IOrderService _orderService;
        private readonly IOrderItemPdfDownloadService _orderItemPdfDownloadService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly IShipmentService _shipmentService;

        public OrderItemPdfDownloadController(IPermissionService permissionService,
                                              IOrderService orderService,
                                              IOrderItemPdfDownloadService orderItemPdfDownloadService,
                                              IWorkContext workContext,
                                              OrderSettings orderSettings,
                                              IShipmentService shipmentService)
        {
            _permissionService = permissionService;
            _orderService = orderService;
            _orderItemPdfDownloadService = orderItemPdfDownloadService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _shipmentService = shipmentService;
        }
        public async Task<IActionResult> OrderItemPdf(int orderId)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return RedirectToAction("List", "Order");
            }

            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _orderItemPdfDownloadService.OrderItemsToPdfAsync(stream, order, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, $"order-info_{orderId}.pdf");
        }


        public async Task<IActionResult> ShipmentItemPdf(int shipmentId)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            if (shipment == null)
            {
                RedirectToAction("List", "Shipment");
            }

            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _orderItemPdfDownloadService.ShipmentItemsToPdfAsync(stream, shipment, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, $"order-info_{shipment.OrderId}_{shipment.Id}.pdf");
        }

    }
}

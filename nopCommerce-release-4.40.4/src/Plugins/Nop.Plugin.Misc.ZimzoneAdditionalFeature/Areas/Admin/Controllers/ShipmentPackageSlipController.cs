using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Services.Common;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class ShipmentPackageSlipController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IShipmentService _shipmentService;
        private readonly IWorkContext _workContext;
        private readonly IPdfService _pdfService;
        private readonly OrderSettings _orderSettings;
        private readonly IOrderService _orderService;
        private readonly IShipmentPackageSlipService _shipmentPackageSlipService;

        public ShipmentPackageSlipController(IPermissionService permissionService,
            IShipmentService shipmentService,
            IWorkContext workContext,
            IPdfService pdfService,
            OrderSettings orderSettings,
            IOrderService orderService,
            IShipmentPackageSlipService shipmentPackageSlipService)
        {
            _permissionService = permissionService;
            _shipmentService = shipmentService;
            _workContext = workContext;
            _pdfService = pdfService;
            _orderSettings = orderSettings;
            _orderService = orderService;
            _shipmentPackageSlipService = shipmentPackageSlipService;
        }

        private async Task<bool> HasAccessToOrderAsync(int orderId)
        {
            if (orderId == 0)
                return false;

            if (await _workContext.GetCurrentVendorAsync() == null)
                //not a vendor; has access
                return true;

            var vendorId = (await _workContext.GetCurrentVendorAsync()).Id;
            var hasVendorProducts = (await _orderService.GetOrderItemsAsync(orderId, vendorId: vendorId)).Any();

            return hasVendorProducts;
        }

        private async ValueTask<bool> HasAccessToShipmentAsync(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            if (await _workContext.GetCurrentVendorAsync() == null)
                //not a vendor; has access
                return true;

            return await HasAccessToOrderAsync(shipment.OrderId);
        }

        public async Task<IActionResult> PdfPackagingSlip(int shipmentId, string pageSize)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            var shipments = new List<Shipment>
            {
                shipment
            };

            var a5Portrait = ShipmentPackageSlipDefaults.A5Portrait;
            var a5Landscape = ShipmentPackageSlipDefaults.A5Landscape;
            var a6Portrait = ShipmentPackageSlipDefaults.A6Portrait;
            var a6Landscape = ShipmentPackageSlipDefaults.A6Landscape;

            var pdfPage = a5Portrait;
            var fontSize = 22;
            if (pageSize == "A5Landscape")
            {
                pdfPage = a5Landscape;
            }
            else if (pageSize == "A6")
            {
                pdfPage = a6Portrait;
                fontSize = 16;
            }
            else if (pageSize == "A6Landscape")
            {
                pdfPage = a6Landscape;
                fontSize = 16;
            }

            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _shipmentPackageSlipService.PrintPackagingSlipsToPdfAsync(stream, shipments, pdfPage, fontSize, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, $"packagingslip_{shipment.Id}.pdf");
        }
    }
}

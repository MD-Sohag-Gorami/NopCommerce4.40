using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class OverridenOrderController : OrderController
    {
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPdfService _pdfService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly IShipmentService _shipmentService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly IEventPublisher _eventPublisher;

        public OverridenOrderController(IAddressAttributeParser addressAttributeParser, 
                                        IAddressService addressService,
                                        ICustomerActivityService customerActivityService, 
                                        ICustomerService customerService, 
                                        IDateTimeHelper dateTimeHelper,
                                        IDownloadService downloadService, 
                                        IEncryptionService encryptionService, 
                                        IExportManager exportManager,
                                        IGiftCardService giftCardService, 
                                        ILocalizationService localizationService, 
                                        INotificationService notificationService,
                                        IOrderModelFactory orderModelFactory, 
                                        IOrderProcessingService orderProcessingService, 
                                        IOrderService orderService,
                                        IPaymentService paymentService, 
                                        IPdfService pdfService, 
                                        IPermissionService permissionService,
                                        IPriceCalculationService priceCalculationService, 
                                        IProductAttributeFormatter productAttributeFormatter,
                                        IProductAttributeParser productAttributeParser, 
                                        IProductAttributeService productAttributeService,
                                        IProductService productService, 
                                        IShipmentService shipmentService, 
                                        IShippingService shippingService,
                                        IShoppingCartService shoppingCartService, 
                                        IWorkContext workContext, 
                                        IWorkflowMessageService workflowMessageService,
                                        OrderSettings orderSettings,
                                        IEventPublisher eventPublisher) : base(addressAttributeParser, 
                                            addressService, 
                                            customerActivityService, 
                                            customerService,
                                            dateTimeHelper, 
                                            downloadService, 
                                            encryptionService, 
                                            exportManager, 
                                            giftCardService, 
                                            localizationService,
                                            notificationService, 
                                            orderModelFactory, 
                                            orderProcessingService, 
                                            orderService,
                                            paymentService, 
                                            pdfService, 
                                            permissionService, 
                                            priceCalculationService, 
                                            productAttributeFormatter,
                                            productAttributeParser, 
                                            productAttributeService, 
                                            productService, 
                                            shipmentService, 
                                            shippingService,
                                            shoppingCartService, 
                                            workContext, 
                                            workflowMessageService, 
                                            orderSettings)
        {
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _orderModelFactory = orderModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _pdfService = pdfService;
            _permissionService = permissionService;
            _productService = productService;
            _shipmentService = shipmentService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _eventPublisher = eventPublisher;
        }

        #region Shipments

        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> ShipmentList()
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //prepare model
            var model = await _orderModelFactory.PrepareShipmentSearchModelAsync(new ShipmentSearchModel());

            return View(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> ShipmentListSelect(ShipmentSearchModel searchModel)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _orderModelFactory.PrepareShipmentListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> ShipmentsByOrder(OrderShipmentSearchModel searchModel)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return await AccessDeniedDataTablesJson();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(searchModel.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToOrderAsync(order))
                return Content(string.Empty);

            //prepare model
            var model = await _orderModelFactory.PrepareOrderShipmentListModelAsync(searchModel, order);

            return Json(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> ShipmentsItemsByShipmentId(ShipmentItemSearchModel searchModel)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return await AccessDeniedDataTablesJson();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(searchModel.ShipmentId)
                ?? throw new ArgumentException("No shipment found with the specified id");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return Content(string.Empty);

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId)
                ?? throw new ArgumentException("No order found with the specified id");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToOrderAsync(order))
                return Content(string.Empty);

            //prepare model
            searchModel.SetGridPageSize();
            var model = await _orderModelFactory.PrepareShipmentItemListModelAsync(searchModel, shipment);

            return Json(model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> AddShipment(int id)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToOrderAsync(order))
                return RedirectToAction("List");

            //prepare model
            var model = await _orderModelFactory.PrepareShipmentModelAsync(new ShipmentModel(), null, order);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> AddShipment(ShipmentModel model, IFormCollection form, bool continueEditing)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(model.OrderId);
            if (order == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToOrderAsync(order))
                return RedirectToAction("List");

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isShipEnabled: true);
            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
            {
                orderItems = await orderItems.WhereAwait(HasAccessToProductAsync).ToListAsync();
            }

            var shipment = new Shipment
            {
                OrderId = order.Id,
                TrackingNumber = model.TrackingNumber,
                TotalWeight = null,
                AdminComment = model.AdminComment,
                CreatedOnUtc = DateTime.UtcNow
            };

            var shipmentItems = new List<ShipmentItem>();

            decimal? totalWeight = null;

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                //ensure that this product can be shipped (have at least one item to ship)
                var maxQtyToAdd = await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);
                if (maxQtyToAdd <= 0)
                    continue;

                var qtyToAdd = 0; //parse quantity
                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"qtyToAdd{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                var warehouseId = 0;
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.UseMultipleWarehouses)
                {
                    //multiple warehouses supported
                    //warehouse is chosen by a store owner
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"warehouse_{orderItem.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(form[formKey], out warehouseId);
                            break;
                        }
                }
                else
                {
                    //multiple warehouses are not supported
                    warehouseId = product.WarehouseId;
                }

                //validate quantity
                if (qtyToAdd <= 0)
                    continue;
                if (qtyToAdd > maxQtyToAdd)
                    qtyToAdd = maxQtyToAdd;

                //ok. we have at least one item. let's create a shipment (if it does not exist)

                var orderItemTotalWeight = orderItem.ItemWeight * qtyToAdd;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }

                //create a shipment item
                shipmentItems.Add(new ShipmentItem
                {
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd,
                    WarehouseId = warehouseId
                });
            }

            //if we have at least one item in the shipment, then save it
            if (shipmentItems.Any())
            {
                shipment.TotalWeight = totalWeight;
                await _shipmentService.InsertShipmentAsync(shipment);

                foreach (var shipmentItem in shipmentItems)
                {
                    shipmentItem.ShipmentId = shipment.Id;
                    await _shipmentService.InsertShipmentItemAsync(shipmentItem);
                }

                // published a event for shipment created admin email notification
                await _eventPublisher.PublishAsync(new ShipmentUpdatedEvent(shipment));

                //add a note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "A shipment has been added",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                if (model.CanShip)
                    await _orderProcessingService.ShipAsync(shipment, true);

                if (model.CanShip && model.CanDeliver)
                    await _orderProcessingService.DeliverAsync(shipment, true);

                await LogEditOrderAsync(order.Id);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.Added"));
                return continueEditing
                        ? RedirectToAction("ShipmentDetails", new { id = shipment.Id })
                        : RedirectToAction("Edit", new { id = model.OrderId });
            }

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.NoProductsSelected"));

            return RedirectToAction("AddShipment", model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> ShipmentDetails(int id)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            //prepare model
            var model = await _orderModelFactory.PrepareShipmentModelAsync(null, shipment, null);

            return View(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> DeleteShipment(int id)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            foreach (var shipmentItem in await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id))
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(shipmentItem.OrderItemId);
                if (orderItem == null)
                    continue;

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                await _productService.ReverseBookedInventoryAsync(product, shipmentItem,
                    string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.DeleteShipment"), shipment.OrderId));
            }

            var orderId = shipment.OrderId;
            await _shipmentService.DeleteShipmentAsync(shipment);

            var order = await _orderService.GetOrderByIdAsync(orderId);
            //add a note
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = "A shipment has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            await LogEditOrderAsync(order.Id);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.Deleted"));
            return RedirectToAction("Edit", new { id = orderId });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("settrackingnumber")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> SetTrackingNumber(ShipmentModel model)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            shipment.TrackingNumber = model.TrackingNumber;
            await _shipmentService.UpdateShipmentAsync(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setadmincomment")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> SetShipmentAdminComment(ShipmentModel model)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            shipment.AdminComment = model.AdminComment;
            await _shipmentService.UpdateShipmentAsync(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasshipped")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> SetAsShipped(int id)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            try
            {
                await _orderProcessingService.ShipAsync(shipment, true);
                await LogEditOrderAsync(shipment.OrderId);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("saveshippeddate")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> EditShippedDate(ShipmentModel model)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            try
            {
                if (!model.ShippedDateUtc.HasValue)
                {
                    throw new Exception("Enter shipped date");
                }

                shipment.ShippedDateUtc = model.ShippedDateUtc;
                await _shipmentService.UpdateShipmentAsync(shipment);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasdelivered")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> SetAsDelivered(int id)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            try
            {
                await _orderProcessingService.DeliverAsync(shipment, true);
                await LogEditOrderAsync(shipment.OrderId);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("savedeliverydate")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> EditDeliveryDate(ShipmentModel model)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            //try to get a shipment with the specified id
            var shipment = await _shipmentService.GetShipmentByIdAsync(model.Id);
            if (shipment == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToShipmentAsync(shipment))
                return RedirectToAction("List");

            try
            {
                if (!model.DeliveryDateUtc.HasValue)
                {
                    throw new Exception("Enter delivery date");
                }

                shipment.DeliveryDateUtc = model.DeliveryDateUtc;
                await _shipmentService.UpdateShipmentAsync(shipment);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> PdfPackagingSlip(int shipmentId)
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

            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _pdfService.PrintPackagingSlipsToPdfAsync(stream, shipments, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id);
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf, $"packagingslip_{shipment.Id}.pdf");
        }

        [HttpPost, ActionName("ShipmentList")]
        [FormValueRequired("exportpackagingslips-all")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> PdfPackagingSlipAll(ShipmentSearchModel model)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            var startDateValue = model.StartDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            var endDateValue = model.EndDate == null ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

            //a vendor should have access only to his products
            var vendorId = 0;
            if (await _workContext.GetCurrentVendorAsync() != null)
                vendorId = (await _workContext.GetCurrentVendorAsync()).Id;

            //load shipments
            var shipments = await _shipmentService.GetAllShipmentsAsync(vendorId: vendorId,
                warehouseId: model.WarehouseId,
                shippingCountryId: model.CountryId,
                shippingStateId: model.StateProvinceId,
                shippingCounty: model.County,
                shippingCity: model.City,
                trackingNumber: model.TrackingNumber,
                loadNotShipped: model.LoadNotShipped,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue);

            //ensure that we at least one shipment selected
            if (!shipments.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.Shipments.NoShipmentsSelected"));
                return RedirectToAction("ShipmentList");
            }

            try
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await _pdfService.PrintPackagingSlipsToPdfAsync(stream, shipments, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, "packagingslips.pdf");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ShipmentList");
            }
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> PdfPackagingSlipSelected(string selectedIds)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                shipments.AddRange(await _shipmentService.GetShipmentsByIdsAsync(ids));
            }
            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
            {
                shipments = await shipments.WhereAwait(HasAccessToShipmentAsync).ToListAsync();
            }

            try
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await _pdfService.PrintPackagingSlipsToPdfAsync(stream, shipments, _orderSettings.GeneratePdfInvoiceInCustomerLanguage ? 0 : (await _workContext.GetWorkingLanguageAsync()).Id);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, "packagingslips.pdf");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("ShipmentList");
            }
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> SetAsShippedSelected(ICollection<int> selectedIds)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(await _shipmentService.GetShipmentsByIdsAsync(selectedIds.ToArray()));
            }
            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
            {
                shipments = await shipments.WhereAwait(HasAccessToShipmentAsync).ToListAsync();
            }

            foreach (var shipment in shipments)
            {
                try
                {
                    await _orderProcessingService.ShipAsync(shipment, true);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task<IActionResult> SetAsDeliveredSelected(ICollection<int> selectedIds)
        {
            if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders) || await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage)))
                return AccessDeniedView();

            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                shipments.AddRange(await _shipmentService.GetShipmentsByIdsAsync(selectedIds.ToArray()));
            }
            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
            {
                shipments = await shipments.WhereAwait(HasAccessToShipmentAsync).ToListAsync();
            }

            foreach (var shipment in shipments)
            {
                try
                {
                    await _orderProcessingService.DeliverAsync(shipment, true);
                }
                catch
                {
                    //ignore any exception
                }
            }

            return Json(new { Result = true });
        }

        #endregion

    }
}

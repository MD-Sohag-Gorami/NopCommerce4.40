using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Events;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class ServiceRequestController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IServiceRequestModelFactory _serviceRequestModelFactory;
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IZimzoneServiceEntityService _zimzoneService;
        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ServiceAttributeSettings _serviceAttributeSettings;
        private readonly IDownloadService _downloadService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IOrderService _orderService;

        public ServiceRequestController(IPermissionService permissionService,
            IServiceRequestModelFactory serviceRequestModelFactory,
            IServiceRequestService serviceRequestService,
            IProductAttributeParser productAttributeParser,
            IZimzoneServiceEntityService zimzoneService,
            IProductService productService,
            IProductAttributeService productAttributeService,
            ServiceAttributeSettings serviceAttributeSettings,
            IDownloadService downloadService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IOrderService orderService)
        {
            _permissionService = permissionService;
            _serviceRequestModelFactory = serviceRequestModelFactory;
            _serviceRequestService = serviceRequestService;
            _productAttributeParser = productAttributeParser;
            _zimzoneService = zimzoneService;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _serviceAttributeSettings = serviceAttributeSettings;
            _downloadService = downloadService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _orderService = orderService;
        }
        public async Task<IActionResult> RequestList()
        {
            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneServiceRequestPanel))
            {
                return AccessDeniedView();
            }

            //prepare model
            var model = await _serviceRequestModelFactory.PrepareRequestSearchModelAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Requests(ServiceRequestSearchModel searchModel)
        {

            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneServiceRequestPanel))
            {
                return AccessDeniedView();
            }

            if (searchModel == null)
            {
                return RedirectToAction("ServiceList");
            }
            //prepare model
            var model = await _serviceRequestModelFactory.PrepareRequestListModelAsync(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> EditRequest(int id)
        {

            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneServiceRequestPanel))
            {
                return AccessDeniedView();
            }

            var request = await _serviceRequestService.GetRequestByIdAsync(id);

            if (request == null)
            {
                return RedirectToAction("RequestList");
            }
            var service = await _zimzoneService.GetById(request.ZimZoneServiceId);
            var (name, email, _, description, downloadId) = await _serviceRequestService.ParseServiceRequestAttributeAsync(request);
            var orderId = request.PaidByOrderItemId > 0 ? (await _orderService.GetOrderByOrderItemAsync(request.PaidByOrderItemId))?.Id ?? 0 : 0;
            var model = new ServiceRequestModel
            {
                Id = request.Id,
                ServiceName = service?.ServiceProductName ?? string.Empty,
                CustomerName = name,
                CustomerEmail = email,
                CustomerAddress = request.CustomerAddress,
                Description = description,
                DownloadGuid = downloadId,
                AgreedAmount = request.AgreedAmount,
                OrderId = orderId,
                IsAgreed = request.IsAgreed,
                QuoteDownloadId = request.QuoteDownloadId,
                CreatedOn = request.CreatedOn,
                CustomName = request.CustomName ?? string.Empty

            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRequest(ServiceRequestModel model)
        {

            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneServiceRequestPanel))
            {
                return AccessDeniedView();
            }

            if (ModelState.IsValid)
            {
                var request = await _serviceRequestService.GetRequestByIdAsync(model.Id);

                //(_, _,_, string description, _) = await _serviceRequestService.ParseServiceRequestAttributeAsync(request);
                request.Description = model.Description;
                request.CustomerAddress = model.CustomerAddress;
                request.QuoteDownloadId = model.QuoteDownloadId;
                request.CustomName = model.CustomName;
                if (request != null && model.AgreedAmount > 0M && request.AgreedAmount != model.AgreedAmount)
                {
                    request.AgreedAmount = model.AgreedAmount;
                    request.IsAgreed = true;
                    await _serviceRequestService.UpdateRequestAsync(request);
                    var customer = await _customerService.GetCustomerByEmailAsync(request.CustomerEmail);
                    if (customer == null)
                    {
                        customer = await _customerService.GetCustomerByIdAsync(request.CustomerId);
                    }
                    var addToCart = false;
                    if (customer != null)
                    {
                        addToCart = await _serviceRequestService.AddToCartAsync(request, customer);
                    }
                    if (addToCart)
                    {
                        await _eventPublisher.PublishAsync(new ServiceRequestAcceptedEvent(request));
                    }
                }
                await _serviceRequestService.UpdateRequestAsync(request);
                return RedirectToAction("RequestList");
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {

            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneServiceRequestPanel))
            {
                return AccessDeniedView();
            }

            var request = await _serviceRequestService.GetRequestByIdAsync(id);

            if (request != null)
            {
                await _serviceRequestService.DeleteRequestAsync(request);
            }
            return RedirectToAction("RequestList");
        }
        public async Task<IActionResult> GetFile(Guid downloadGuid)
        {

            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneServiceRequestPanel))
            {
                return AccessDeniedView();
            }

            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);

            if (download != null)
            {
                return File(download.DownloadBinary, download.ContentType, download.Filename + download.Extension);
            }
            return Content(string.Empty);
        }
    }
}

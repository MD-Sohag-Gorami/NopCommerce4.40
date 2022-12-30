using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Controllers
{
    public class ServiceRequestController : BasePluginController
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly ILogger _logger;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderService _orderService;
        private readonly CurrencySettings _currencySetting;
        private readonly IDownloadService _downloadService;
        private readonly IServiceRequestModelFactory _serviceRequestModelFactory;

        public ServiceRequestController(IServiceRequestService serviceRequestService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IZimzoneServiceEntityService zimzoneServiceEntityService,
            ILocalizationService localizationService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            ILogger logger,
            IShoppingCartService shoppingCartService,
            IOrderService orderService,
            CurrencySettings currencySetting,
            IDownloadService downloadService,
            IServiceRequestModelFactory serviceRequestModelFactory)
        {
            _serviceRequestService = serviceRequestService;
            _workContext = workContext;
            _storeContext = storeContext;
            _currencyService = currencyService;
            _customerService = customerService;
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
            _localizationService = localizationService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _logger = logger;
            _shoppingCartService = shoppingCartService;
            _orderService = orderService;
            _currencySetting = currencySetting;
            _downloadService = downloadService;
            _serviceRequestModelFactory = serviceRequestModelFactory;
        }

        public async Task<IActionResult> ServiceRequestList()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            if (await _customerService.IsRegisteredAsync(customer))
            {
                var requests = new List<ServiceRequestModel>();
                requests = await _serviceRequestModelFactory.PrepareServiceRequestModelListAsync(requests, customer, currentCurrency);
                return View(requests);
            }
            return RedirectToRoute("Login");
        }
        public async Task<IActionResult> ServiceRequestPayment(int requestId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var request = (await _serviceRequestService.GetAllRequestsAsync(customer)).Where(x => x.Id == requestId).FirstOrDefault();
            if (request == null)
            {
                return RedirectToRoute("Homepage");
            }
            var addedToCart = await _serviceRequestService.AddToCartAsync(request, customer);
            if (addedToCart)
            {
                return RedirectToRoute("ShoppingCart");
            }
            return Content(string.Empty);
        }
        public async Task<IActionResult> ServiceRequestDetails(int requestId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var request = (await _serviceRequestService.GetAllRequestsAsync(customer)).Where(x => x.Id == requestId).FirstOrDefault();
            if (request == null)
            {
                return RedirectToRoute("Homepage");
            }

            var model = new ServiceRequestModel();

            model = await _serviceRequestModelFactory.PrepareServiceRequestModelAsync(model, request);

            return View(model);
        }


        public async Task<IActionResult> GetFile(int quoteDownloadId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
            {
                return RedirectToRoute("Login", new { returnUrl = Url.RouteUrl("Customer.ServiceRequestQuoteFile", new { quoteFileId = quoteDownloadId }) });
            }
            var request = (await _serviceRequestService.GetAllRequestsAsync(customer)).Where(x => x.QuoteDownloadId == quoteDownloadId).FirstOrDefault();
            if (request == null)
            {
                return NotFound();
            }
            var download = await _downloadService.GetDownloadByIdAsync(quoteDownloadId);

            if (download == null)
            {
                return NotFound();
            }
            return File(download.DownloadBinary, download.ContentType, download.Filename + download.Extension);
        }

        public async Task<IActionResult> Cart()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
            {
                return RedirectToRoute("Login", new { returnUrl = Url.RouteUrl("ShoppingCart") });
            }
            return RedirectToRoute("ShoppingCart");
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public class ServiceRequestModelFactory : IServiceRequestModelFactory
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IZimzoneServiceEntityService _zimzoneService;
        private readonly ILocalizationService _localizationService;
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly CurrencySettings _currencySettings;

        public ServiceRequestModelFactory(IServiceRequestService serviceRequestService,
            IZimzoneServiceEntityService zimzoneService,
            ILocalizationService localizationService,
            IZimzoneServiceEntityService zimzoneServiceEntityService,
            IOrderService orderService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IWorkContext workContext,
            CurrencySettings currencySettings)
        {
            _serviceRequestService = serviceRequestService;
            _zimzoneService = zimzoneService;
            _localizationService = localizationService;
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
            _orderService = orderService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _workContext = workContext;
            _currencySettings = currencySettings;
        }
        public async Task<ServiceRequestListModel> PrepareRequestListModelAsync(ServiceRequestSearchModel searchModel)
        {
            var requests = await _serviceRequestService.GetAllRequestsAsync(searchModel);
            var requestList = new PagedList<ZimzoneServiceRequestEntity>(requests, 0, searchModel.PageSize);
            var model = await new ServiceRequestListModel().PrepareToGridAsync(searchModel, requestList, () =>
            {
                return requestList.SelectAwait(async m =>
                {
                    var service = await _zimzoneService.GetById(m.ZimZoneServiceId);
                    var request = new ServiceRequestModel
                    {
                        Id = m.Id,
                        CustomerEmail = m.CustomerEmail,
                        ServiceName = service?.ServiceProductName ?? string.Empty,
                        Status = m.IsAgreed ? m.PaidByOrderItemId > 0 ? _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Paid").Result : _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Accepted").Result : _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Pending").Result,
                        IsAgreed = m.IsAgreed,
                        AgreedAmount = m.AgreedAmount,
                        CreatedOn = m.CreatedOn
                    };

                    if (!string.IsNullOrEmpty(m.CustomName))
                        request.ServiceName += $"({m.CustomName})";

                    return await Task.FromResult(request);
                });
            });

            return model;
        }

        public async Task<ServiceRequestSearchModel> PrepareRequestSearchModelAsync()
        {
            return await Task.FromResult(new ServiceRequestSearchModel
            {
                AvailablePageSizes = "10,20,50,100",
                AvailableServices = await PrepareAvailableServicesAsync(),
                AvailableStatus = await PrepareAvailableStatusAsync()
            });
        }

        public async Task<ServiceRequestModel> PrepareServiceRequestModelAsync(ServiceRequestModel model, ZimzoneServiceRequestEntity request)
        {
            var (name, email, address, description, downloadId) = await _serviceRequestService.ParseServiceRequestAttributeAsync(request);
            var service = await _zimzoneServiceEntityService.GetById(request.ZimZoneServiceId);
            var orderId = request.PaidByOrderItemId > 0 ? (await _orderService.GetOrderByOrderItemAsync(request.PaidByOrderItemId))?.Id ?? 0 : 0;

            model.Id = request.Id;
            model.ServiceName = service?.ServiceProductName ?? string.Empty;
            model.CustomerName = name;
            model.CustomerEmail = email;
            model.CustomerAddress = address;
            model.Description = description;
            model.PaidByOrderItemId = request.PaidByOrderItemId;
            model.DownloadGuid = downloadId;
            model.Status = request.IsAgreed ? request.PaidByOrderItemId > 0 ? _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Paid").Result : _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Accepted").Result : _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Pending").Result;
            model.AgreedAmount = request.AgreedAmount;
            model.AgreedAmountInCustomerCurrency = await GetAmountInCustomerCurrencyStringAsync(request.AgreedAmount);
            model.OrderId = orderId;
            model.IsAgreed = request.IsAgreed;

            if (!string.IsNullOrEmpty(request.CustomName))
                model.ServiceName = request.CustomName;

            model.QuoteDownloadId = request.QuoteDownloadId;

            return model;
        }

        public async Task<List<ServiceRequestModel>> PrepareServiceRequestModelListAsync(List<ServiceRequestModel> requests, Customer customer, Currency currentCurrency)
        {
            requests = (await _serviceRequestService.GetAllRequestsAsync(customer)).Select(x =>
            {
                var service = _zimzoneServiceEntityService.GetById(x.ZimZoneServiceId).Result;
                var agreedAmountInCustomerCurrency = _currencyService.ConvertCurrency(x.AgreedAmount, currentCurrency.Rate);
                var agreedAmount = _priceFormatter.FormatPriceAsync(agreedAmountInCustomerCurrency, true, currentCurrency.CurrencyCode, false, (_workContext.GetWorkingLanguageAsync().Result).Id).Result;

                var model = new ServiceRequestModel
                {
                    Id = x.Id,
                    CreatedOn = x.CreatedOn,
                    AgreedAmount = x.AgreedAmount,
                    PaidByOrderItemId = x.PaidByOrderItemId,
                    CustomerEmail = x.CustomerEmail,
                    ServiceName = service?.ServiceProductName ?? string.Empty,
                    IsAgreed = x.IsAgreed,
                    Status = x.IsAgreed ? x.PaidByOrderItemId > 0 ? _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Paid").Result : _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Accepted").Result : _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Pending").Result,
                    AgreedAmountInCustomerCurrency = agreedAmount
                };
                if (!string.IsNullOrEmpty(x.CustomName))
                    model.ServiceName = x.CustomName;

                return model;
            }).ToList();

            return requests;
        }

        protected virtual async Task<string> GetAmountInCustomerCurrencyStringAsync(decimal amount)
        {
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            if (currentCurrency.Id == _currencySettings.PrimaryStoreCurrencyId)
            {
                return await _priceFormatter.FormatPriceAsync(amount, true, currentCurrency.CurrencyCode, false, (await _workContext.GetWorkingLanguageAsync()).Id);

            }
            var amountInCustomerCurrency = _currencyService.ConvertCurrency(amount, currentCurrency.Rate);
            return await _priceFormatter.FormatPriceAsync(amountInCustomerCurrency, true, currentCurrency.CurrencyCode, false, (await _workContext.GetWorkingLanguageAsync()).Id);
        }

        async Task<IList<SelectListItem>> PrepareAvailableServicesAsync()
        {
            var availablesServices = (await _zimzoneService.GetAllZimzoneServiceAsync()).Select(x =>
            {
                var model = new SelectListItem
                {
                    Text = x.ServiceProductName,
                    Value = x.Id.ToString()
                };
                return model;
            }).ToList();
            availablesServices.Insert(0, new SelectListItem { Text = "None", Value = "0" });
            return availablesServices;
        }
        async Task<IList<SelectListItem>> PrepareAvailableStatusAsync()
        {
            var availablesStatus = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text=await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Pending"),
                    Value="1"
                },
                new SelectListItem
                {
                    Text=await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Accepted"),
                    Value="2"
                }
            };
            availablesStatus.Insert(0, new SelectListItem { Text = "None", Value = "0" });
            return availablesStatus;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public class CustomOrderStatusModelFactory : ICustomOrderStatusModelFactory
    {
        private readonly ICustomOrderStatusService _customOrderStatusService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IStoreService _storeService;
        private readonly ILocalizationService _localizationService;
        private readonly AddressSettings _addressSettings;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly ICountryService _countryService;
        private readonly CatalogSettings _catalogSettings;

        public CustomOrderStatusModelFactory(ICustomOrderStatusService customOrderStatusService,
                                             IWorkContext workContext,
                                             IDateTimeHelper dateTimeHelper,
                                             IProductService productService,
                                             IOrderService orderService,
                                             IAddressService addressService,
                                             IPriceFormatter priceFormatter,
                                             ICurrencyService currencyService,
                                             IStoreService storeService,
                                             ILocalizationService localizationService,
                                             AddressSettings addressSettings,
                                             IBaseAdminModelFactory baseAdminModelFactory,
                                             IPaymentPluginManager paymentPluginManager,
                                             ICountryService countryService,
                                             CatalogSettings catalogSettings)
        {
            _customOrderStatusService = customOrderStatusService;
            _workContext = workContext;
            _dateTimeHelper = dateTimeHelper;
            _productService = productService;
            _orderService = orderService;
            _addressService = addressService;
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
            _storeService = storeService;
            _localizationService = localizationService;
            _addressSettings = addressSettings;
            _baseAdminModelFactory = baseAdminModelFactory;
            _paymentPluginManager = paymentPluginManager;
            _countryService = countryService;
            _catalogSettings = catalogSettings;
        }

        private List<SelectListItem> PrepareOrderStatusList()
        {
            var orderStatus = new List<OrderStatus>()
            {
                OrderStatus.Pending,
                OrderStatus.Processing,
                OrderStatus.Complete,
                OrderStatus.Cancelled,
            };

            var orderStatusList = orderStatus.Select(x =>
            {
                var status = new SelectListItem()
                {
                    Text = x.ToString(),
                    Value = ((int)x).ToString()
                };
                return status;
            }).ToList();

            return orderStatusList;
        }

        public CustomOrderStatusSearchModel PrepareCustomOrderStatusSearchModel(CustomOrderStatusSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            var orderStatusList = PrepareOrderStatusList();
            //add default value
            orderStatusList.Insert(0, new SelectListItem()
            {
                Text = "All",
                Value = ""
            });

            searchModel.OrderStatusList = orderStatusList;

            return searchModel;
        }

        public async Task<CustomOrderStatusListModel> PrepareCustomOrderStatusListModelAsync(CustomOrderStatusSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            var domain = await _customOrderStatusService.GetAllPagedCustomOrderStatusAsync(customOrderStatusName: searchModel.CustomOrderStatusName, parentOrderStatusId: searchModel.ParentOrderStatusId);

            var model = await new CustomOrderStatusListModel().PrepareToGridAsync(searchModel, domain, () =>
            {
                return domain.SelectAwait(async os =>
                {
                    //fill in model values from the entity
                    var customOrderStatusModel = os.ToModel<CustomOrderStatusModel>();
                    //retrieve order status name with index - enum
                    customOrderStatusModel.ParentOrderStatus = ((OrderStatus)customOrderStatusModel.ParentOrderStatusId).ToString();
                    return customOrderStatusModel;
                });
            });

            return model;
        }

        public CustomOrderStatusModel PrepareCustomOrderStatusModel(CustomOrderStatusModel model)
        {
            var orderStatusList = PrepareOrderStatusList();
            //add default value
            orderStatusList.Insert(0, new SelectListItem()
            {
                Text = "Select One",
                Value = ""
            });
            model.OrderStatusList = orderStatusList;

            return model;
        }

        public async Task<CustomStatusListWithOrderModel> PrepareCustomOrderStatusListAsync(CustomStatusListWithOrderModel model)
        {
            var orderWithCustomStatus = await _customOrderStatusService.GetOrderWithCustomStatusAsync(orderId: model.OrderId);
            if (orderWithCustomStatus != null)
            {
                model.CustomOrderStatusId = orderWithCustomStatus.CustomOrderStatusId;
                model.ParentOrderStatusId = orderWithCustomStatus.ParentOrderStatusId;
                model.Id = orderWithCustomStatus.Id;
            }

            var customOrderStatusList = await _customOrderStatusService.GetCustomOrderStatusByParentOrderStatusIdAsync(model.ParentOrderStatusId);
            var customOrderStatusSelectList = customOrderStatusList.Select(x =>
            {
                var status = new SelectListItem()
                {
                    Text = x.CustomOrderStatusName,
                    Value = x.Id.ToString()
                };
                return status;
            }).ToList();

            customOrderStatusSelectList.Insert(0, new SelectListItem()
            {
                Text = "Select One",
                Value = "0"
            });

            model.CustomOrderStatus = customOrderStatusSelectList;

            return model;
        }

        public async Task PrepareCustomOrderStatusAsync(IList<SelectListItem> items)
        {
            var customOrderStatus = await _customOrderStatusService.GetAllCustomOrderStatusAsync();
            var selectListItems = customOrderStatus.Select(x =>
            {
                var status = new SelectListItem()
                {
                    Text = x.CustomOrderStatusName,
                    Value = x.Id.ToString()
                };
                return status;
            }).ToList();

            selectListItems.Insert(0, new SelectListItem()
            {
                Text = "All",
                Value = "0"
            });

            foreach (var item in selectListItems)
            {
                items.Add(item);
            }
        }

        public async Task<OrderWithCustomStatusSearchModel> PrepareOrderWithCustomStatusSearchModelAsync(OrderWithCustomStatusSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
            searchModel.BillingPhoneEnabled = _addressSettings.PhoneEnabled;

            //prepare available order, payment and shipping statuses
            await _baseAdminModelFactory.PrepareOrderStatusesAsync(searchModel.AvailableOrderStatuses);
            if (searchModel.AvailableOrderStatuses.Any())
            {
                if (searchModel.OrderStatusIds?.Any() ?? false)
                {
                    var ids = searchModel.OrderStatusIds.Select(id => id.ToString());
                    searchModel.AvailableOrderStatuses.Where(statusItem => ids.Contains(statusItem.Value)).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    searchModel.AvailableOrderStatuses.FirstOrDefault().Selected = true;
            }

            await PrepareCustomOrderStatusAsync(searchModel.CustomOrderStatusList);

            if (searchModel.CustomOrderStatusList.Any())
            {
                if (searchModel.CustomOrderStatusIds?.Any() ?? false)
                {
                    var ids = searchModel.CustomOrderStatusIds.Select(id => id.ToString());
                    searchModel.CustomOrderStatusList.Where(statusItem => ids.Contains(statusItem.Value)).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    searchModel.CustomOrderStatusList.FirstOrDefault().Selected = true;
            }

            await _baseAdminModelFactory.PreparePaymentStatusesAsync(searchModel.AvailablePaymentStatuses);
            if (searchModel.AvailablePaymentStatuses.Any())
            {
                if (searchModel.PaymentStatusIds?.Any() ?? false)
                {
                    var ids = searchModel.PaymentStatusIds.Select(id => id.ToString());
                    searchModel.AvailablePaymentStatuses.Where(statusItem => ids.Contains(statusItem.Value)).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    searchModel.AvailablePaymentStatuses.FirstOrDefault().Selected = true;
            }

            await _baseAdminModelFactory.PrepareShippingStatusesAsync(searchModel.AvailableShippingStatuses);
            if (searchModel.AvailableShippingStatuses.Any())
            {
                if (searchModel.ShippingStatusIds?.Any() ?? false)
                {
                    var ids = searchModel.ShippingStatusIds.Select(id => id.ToString());
                    searchModel.AvailableShippingStatuses.Where(statusItem => ids.Contains(statusItem.Value)).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    searchModel.AvailableShippingStatuses.FirstOrDefault().Selected = true;
            }

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare available warehouses
            await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

            //prepare available payment methods
            searchModel.AvailablePaymentMethods = (await _paymentPluginManager.LoadAllPluginsAsync()).Select(method =>
                new SelectListItem { Text = method.PluginDescriptor.FriendlyName, Value = method.PluginDescriptor.SystemName }).ToList();
            searchModel.AvailablePaymentMethods.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = string.Empty });

            //prepare available billing countries
            searchModel.AvailableCountries = (await _countryService.GetAllCountriesForBillingAsync(showHidden: true))
                .Select(country => new SelectListItem { Text = country.Name, Value = country.Id.ToString() }).ToList();
            searchModel.AvailableCountries.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });

            //prepare grid
            searchModel.SetGridPageSize();

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            return searchModel;
        }

        public async Task<OrderWithCustomStatusListModel> PrepareOrderWithCustomStatusListModelAsync(OrderWithCustomStatusSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var orderStatusIds = (searchModel.OrderStatusIds?.Contains(0) ?? true) ? null : searchModel.OrderStatusIds.ToList();
            var customOrderStatusIds = (searchModel.CustomOrderStatusIds?.Contains(0) ?? true) ? null : searchModel.CustomOrderStatusIds.ToList();
            var paymentStatusIds = (searchModel.PaymentStatusIds?.Contains(0) ?? true) ? null : searchModel.PaymentStatusIds.ToList();
            var shippingStatusIds = (searchModel.ShippingStatusIds?.Contains(0) ?? true) ? null : searchModel.ShippingStatusIds.ToList();
            if (await _workContext.GetCurrentVendorAsync() != null)
                searchModel.VendorId = (await _workContext.GetCurrentVendorAsync()).Id;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.StartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.EndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId);
            var filterByProductId = product != null && (await _workContext.GetCurrentVendorAsync() == null || product.VendorId == (await _workContext.GetCurrentVendorAsync()).Id)
                ? searchModel.ProductId : 0;

            var orderWithCustomStatus = await _customOrderStatusService.SearchOrderWithCustomStatusAsync(storeId: searchModel.StoreId,
                vendorId: searchModel.VendorId,
                productId: filterByProductId,
                warehouseId: searchModel.WarehouseId,
                paymentMethodSystemName: searchModel.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                cosIds: customOrderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingPhone: searchModel.BillingPhone,
                billingEmail: searchModel.BillingEmail,
                billingLastName: searchModel.BillingLastName,
                billingCountryId: searchModel.BillingCountryId,
                orderNotes: searchModel.OrderNotes,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);


            //prepare list model
            var model = await new OrderWithCustomStatusListModel().PrepareToGridAsync(searchModel, orderWithCustomStatus, () =>
            {
                //fill in model values from the entity
                return orderWithCustomStatus.SelectAwait(async order =>
                {
                    var billingAddress = await _addressService.GetAddressByIdAsync(order.Order.BillingAddressId);
                    var orderTotalText = "{0}/{1}";
                    var orderTotalInPrimaryCurrency = await _priceFormatter.FormatPriceAsync(order.Order.OrderTotal, true, false);
                    var orderTotalInOrderCurrency = await GetOrderTotalInOrderCurrencyString(order.Order, true, order.Order.CustomerCurrencyCode);
                    //fill in model values from the entity
                    var orderModel = new OrderWithCustomStatusModel
                    {
                        Id = order.Order.Id,
                        OrderStatusId = order.Order.OrderStatusId,
                        CustomOrderStatusId = order.CustomOrderStatusId,
                        CustomOrderStatusName = order.CustomOrderStatusName ?? "N/A",
                        PaymentStatusId = order.Order.PaymentStatusId,
                        ShippingStatusId = order.Order.ShippingStatusId,
                        CustomerEmail = billingAddress.Email,
                        CustomerFullName = $"{billingAddress.FirstName} {billingAddress.LastName}",
                        CustomerId = order.Order.CustomerId,
                        CustomOrderNumber = order.Order.CustomOrderNumber
                    };

                    //convert dates to the user time
                    orderModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.Order.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    orderModel.StoreName = (await _storeService.GetStoreByIdAsync(order.Order.StoreId))?.Name ?? "Deleted";
                    orderModel.OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.Order.OrderStatus);
                    orderModel.PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.Order.PaymentStatus);
                    orderModel.ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.Order.ShippingStatus);
                    orderModel.OrderTotal = string.Format(orderTotalText, orderTotalInPrimaryCurrency, orderTotalInOrderCurrency);
                    return orderModel;
                });
            });

            return model;
        }

        protected virtual async Task<string> GetOrderTotalInOrderCurrencyString(Order order,
            bool showCurrency, string currencyCode)
        {
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var orderTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, (await _workContext.GetWorkingLanguageAsync()).Id);
            return orderTotal;
        }
    }
}

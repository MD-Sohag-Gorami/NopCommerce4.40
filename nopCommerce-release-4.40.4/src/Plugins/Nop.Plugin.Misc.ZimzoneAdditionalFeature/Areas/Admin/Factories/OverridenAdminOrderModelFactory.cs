using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public class OverridenAdminOrderModelFactory : OrderModelFactory
    {
        private readonly CurrencySettings _currencySettings;
        private readonly IAddressService _addressService;
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountService _discountService;
        private readonly IDownloadService _downloadService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderReportService _orderReportService;
        private readonly IProductService _productService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;


        public OverridenAdminOrderModelFactory(AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IAffiliateService affiliateService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IDownloadService downloadService,
            IEncryptionService encryptionService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            IOrderProcessingService orderProcessingService,
            IOrderReportService orderReportService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IStateProvinceService stateProvinceService,
            IStoreService storeService,
            ITaxService taxService,
            IUrlHelperFactory urlHelperFactory,
            IVendorService vendorService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            OrderSettings orderSettings,
            ShippingSettings shippingSettings,
            IUrlRecordService urlRecordService,
            TaxSettings taxSettings,
            IZimzoneServiceEntityService zimzoneServiceEntityService,
            IServiceRequestService serviceRequestService) : base(addressSettings, catalogSettings,
                currencySettings, actionContextAccessor, addressModelFactory,
                addressService, affiliateService, baseAdminModelFactory, countryService,
                currencyService, customerService, dateTimeHelper, discountService,
                downloadService, encryptionService, giftCardService, localizationService,
                measureService, orderProcessingService, orderReportService, orderService,
                paymentPluginManager, paymentService, pictureService, priceCalculationService,
                priceFormatter, productAttributeService, productService, returnRequestService,
                rewardPointService, shipmentService, shippingService, stateProvinceService,
                storeService, taxService, urlHelperFactory, vendorService, workContext,
                measureSettings, orderSettings, shippingSettings, urlRecordService, taxSettings)
        {
            _currencySettings = currencySettings;
            _addressService = addressService;
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _discountService = discountService;
            _downloadService = downloadService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _orderReportService = orderReportService;
            _productService = productService;
            _rewardPointService = rewardPointService;
            _storeService = storeService;
            _vendorService = vendorService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _taxSettings = taxSettings;
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
            _serviceRequestService = serviceRequestService;
            _orderService = orderService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
        }
        public override async Task<OrderListModel> PrepareOrderListModelAsync(OrderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter orders
            var orderStatusIds = (searchModel.OrderStatusIds?.Contains(0) ?? true) ? null : searchModel.OrderStatusIds.ToList();
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

            //get orders
            var orders = await _orderService.SearchOrdersAsync(storeId: searchModel.StoreId,
                vendorId: searchModel.VendorId,
                productId: filterByProductId,
                warehouseId: searchModel.WarehouseId,
                paymentMethodSystemName: searchModel.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingPhone: searchModel.BillingPhone,
                billingEmail: searchModel.BillingEmail,
                billingLastName: searchModel.BillingLastName,
                billingCountryId: searchModel.BillingCountryId,
                orderNotes: searchModel.OrderNotes,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new OrderListModel().PrepareToGridAsync(searchModel, orders, () =>
            {
                //fill in model values from the entity
                return orders.SelectAwait(async order =>
                {
                    var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                    var orderTotalText = "{0}/{1}";
                    var orderTotalInPrimaryCurrency = await _priceFormatter.FormatPriceAsync(order.OrderTotal, true, false);
                    var orderTotalInOrderCurrency = await GetOrderTotalInOrderCurrencyString(order, true, order.CustomerCurrencyCode);
                    //fill in model values from the entity
                    var orderModel = new OrderModel
                    {
                        Id = order.Id,
                        OrderStatusId = order.OrderStatusId,
                        PaymentStatusId = order.PaymentStatusId,
                        ShippingStatusId = order.ShippingStatusId,
                        CustomerEmail = billingAddress.Email,
                        CustomerFullName = $"{billingAddress.FirstName} {billingAddress.LastName}",
                        CustomerId = order.CustomerId,
                        CustomOrderNumber = order.CustomOrderNumber
                    };

                    //convert dates to the user time
                    orderModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    orderModel.StoreName = (await _storeService.GetStoreByIdAsync(order.StoreId))?.Name ?? "Deleted";
                    orderModel.OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus);
                    orderModel.PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
                    orderModel.ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus);
                    orderModel.OrderTotal = string.Format(orderTotalText, orderTotalInPrimaryCurrency, orderTotalInOrderCurrency);
                    return orderModel;
                });
            });

            return model;
        }

        protected override async Task PrepareOrderModelTotalsAsync(OrderModel model, Order order)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            //subtotal
            model.OrderSubtotalInclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubtotalInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true);
            model.OrderSubtotalExclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubtotalExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false);
            model.OrderSubtotalInclTaxValue = order.OrderSubtotalInclTax;
            model.OrderSubtotalExclTaxValue = order.OrderSubtotalExclTax;

            //discount (applied to order subtotal)
            var orderSubtotalDiscountInclTaxStr = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubTotalDiscountInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true);
            var orderSubtotalDiscountExclTaxStr = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubTotalDiscountExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false);
            if (order.OrderSubTotalDiscountInclTax > decimal.Zero)
                model.OrderSubTotalDiscountInclTax = orderSubtotalDiscountInclTaxStr;
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                model.OrderSubTotalDiscountExclTax = orderSubtotalDiscountExclTaxStr;
            model.OrderSubTotalDiscountInclTaxValue = order.OrderSubTotalDiscountInclTax;
            model.OrderSubTotalDiscountExclTaxValue = order.OrderSubTotalDiscountExclTax;

            //shipping
            model.OrderShippingInclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderShippingInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true,
                _taxSettings.ShippingIsTaxable && _taxSettings.DisplayTaxSuffix);
            model.OrderShippingExclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderShippingExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false,
                _taxSettings.ShippingIsTaxable && _taxSettings.DisplayTaxSuffix);
            model.OrderShippingInclTaxValue = order.OrderShippingInclTax;
            model.OrderShippingExclTaxValue = order.OrderShippingExclTax;

            //payment method additional fee
            if (order.PaymentMethodAdditionalFeeInclTax > decimal.Zero)
            {
                model.PaymentMethodAdditionalFeeInclTax = await _priceFormatter
                    .FormatOrderPriceAsync(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true,
                    _taxSettings.PaymentMethodAdditionalFeeIsTaxable && _taxSettings.DisplayTaxSuffix);
                model.PaymentMethodAdditionalFeeExclTax = await _priceFormatter
                    .FormatOrderPriceAsync(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false,
                    _taxSettings.PaymentMethodAdditionalFeeIsTaxable && _taxSettings.DisplayTaxSuffix);
            }

            model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
            model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;

            //tax
            model.Tax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
            var taxRates = _orderService.ParseTaxRates(order, order.TaxRates);
            var displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
            var displayTax = !displayTaxRates;
            foreach (var tr in taxRates)
            {
                model.TaxRates.Add(new OrderModel.TaxRate
                {
                    Rate = _priceFormatter.FormatTaxRate(tr.Key),
                    Value = await _priceFormatter
                        .FormatOrderPriceAsync(tr.Value, order.CurrencyRate, order.CustomerCurrencyCode,
                        _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false)
                });
            }

            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.TaxValue = order.OrderTax;
            model.TaxRatesValue = order.TaxRates;

            //discount
            if (order.OrderDiscount > 0)
            {
                model.OrderTotalDiscount = await _priceFormatter
                    .FormatOrderPriceAsync(-order.OrderDiscount, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
            }
            model.OrderTotalDiscountValue = order.OrderDiscount;

            //gift cards
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                model.GiftCards.Add(new OrderModel.GiftCard
                {
                    CouponCode = (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId)).GiftCardCouponCode,
                    Amount = await _priceFormatter.FormatPriceAsync(-gcuh.UsedValue, true, false)
                });
            }

            //reward points
            if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                model.RedeemedRewardPoints = -redeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount =
                    await _priceFormatter.FormatPriceAsync(-redeemedRewardPointsEntry.UsedAmount, true, false);
            }

            //total
            model.OrderTotal = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderTotal, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
            model.OrderTotalValue = order.OrderTotal;

            //refunded amount
            if (order.RefundedAmount > decimal.Zero)
                model.RefundedAmount = await _priceFormatter.FormatPriceAsync(order.RefundedAmount, true, false);

            //used discounts
            var duh = await _discountService.GetAllDiscountUsageHistoryAsync(orderId: order.Id);
            foreach (var d in duh)
            {
                var discount = await _discountService.GetDiscountByIdAsync(d.DiscountId);

                model.UsedDiscounts.Add(new OrderModel.UsedDiscountModel
                {
                    DiscountId = d.DiscountId,
                    DiscountName = discount.Name
                });
            }

            //profit (hide for vendors)
            if (await _workContext.GetCurrentVendorAsync() != null)
                return;

            var profit = await _orderReportService.ProfitReportAsync(orderId: order.Id);
            model.Profit = await _priceFormatter
                .FormatOrderPriceAsync(profit, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
        }

        protected override async Task PrepareOrderItemModelsAsync(IList<OrderItemModel> models, Order order)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

            //get order items
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: (await _workContext.GetCurrentVendorAsync())?.Id ?? 0);

            var servicePaymentProductIds = await _zimzoneServiceEntityService.GetAllPaymentProductIdAsync();

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                //fill in model values from the entity
                var orderItemModel = new OrderItemModel
                {
                    Id = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    ProductName = product.Name,
                    Quantity = orderItem.Quantity,
                    IsDownload = product.IsDownload,
                    DownloadCount = orderItem.DownloadCount,
                    DownloadActivationType = product.DownloadActivationType,
                    IsDownloadActivated = orderItem.IsDownloadActivated,
                    UnitPriceInclTaxValue = orderItem.UnitPriceInclTax,
                    UnitPriceExclTaxValue = orderItem.UnitPriceExclTax,
                    DiscountInclTaxValue = orderItem.DiscountAmountInclTax,
                    DiscountExclTaxValue = orderItem.DiscountAmountExclTax,
                    SubTotalInclTaxValue = orderItem.PriceInclTax,
                    SubTotalExclTaxValue = orderItem.PriceExclTax,
                    AttributeInfo = orderItem.AttributeDescription
                };

                // concatenate service product custom name with name
                if (servicePaymentProductIds.Contains(orderItem.ProductId))
                {
                    if (int.TryParse(orderItem.AttributesXml, out var requestId))
                    {
                        var request = await _serviceRequestService.GetRequestByIdAsync(requestId);
                        if (request != null && !string.IsNullOrEmpty(request.CustomName))
                        {
                            orderItemModel.ProductName += $"({request.CustomName})";
                        }
                    }
                }


                //fill in additional values (not existing in the entity)
                orderItemModel.Sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                orderItemModel.VendorName = (await _vendorService.GetVendorByIdAsync(product.VendorId))?.Name;

                //picture
                var orderItemPicture = await _pictureService.GetProductPictureAsync(product, orderItem.AttributesXml);
                (orderItemModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(orderItemPicture, 75);

                //license file
                if (orderItem.LicenseDownloadId.HasValue)
                {
                    orderItemModel.LicenseDownloadGuid = (await _downloadService
                        .GetDownloadByIdAsync(orderItem.LicenseDownloadId.Value))?.DownloadGuid ?? Guid.Empty;
                }

                var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

                //unit price
                orderItemModel.UnitPriceInclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.UnitPriceInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true, true);
                orderItemModel.UnitPriceExclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.UnitPriceExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false, true);

                //discounts
                orderItemModel.DiscountInclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.DiscountAmountInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true, true);
                orderItemModel.DiscountExclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.DiscountAmountExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false, true);

                //subtotal
                orderItemModel.SubTotalInclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.PriceInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true, true);
                orderItemModel.SubTotalExclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.PriceExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false, true);

                //recurring info
                if (product.IsRecurring)
                {
                    orderItemModel.RecurringInfo = string.Format(await _localizationService.GetResourceAsync("Admin.Orders.Products.RecurringPeriod"),
                        product.RecurringCycleLength, await _localizationService.GetLocalizedEnumAsync(product.RecurringCyclePeriod));
                }

                //rental info
                if (product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value) : string.Empty;
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value) : string.Empty;
                    orderItemModel.RentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                }

                //prepare return request models
                await PrepareReturnRequestBriefModelsAsync(orderItemModel.ReturnRequests, orderItem);

                //gift card identifiers
                orderItemModel.PurchasedGiftCardIds = (await _giftCardService
                    .GetGiftCardsByPurchasedWithOrderItemIdAsync(orderItem.Id)).Select(card => card.Id).ToList();

                models.Add(orderItemModel);
            }
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

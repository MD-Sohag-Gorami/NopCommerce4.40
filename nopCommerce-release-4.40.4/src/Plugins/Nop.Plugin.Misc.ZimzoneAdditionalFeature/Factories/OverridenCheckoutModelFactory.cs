using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Factories
{
    public class OverridenCheckoutModelFactory : CheckoutModelFactory
    {
        private readonly CommonSettings _commonSettings;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly IZimZoneErpSyncService _zimZoneErpSyncService;
        private readonly IProductService _productService;

        public OverridenCheckoutModelFactory(AddressSettings addressSettings,
            CommonSettings commonSettings,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPickupPluginManager pickupPluginManager,
            IPriceFormatter priceFormatter,
            IRewardPointService rewardPointService,
            IShippingPluginManager shippingPluginManager,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            ITaxService taxService,
            IWorkContext workContext,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            IZimZoneErpSyncService zimZoneErpSyncService,
            IProductService productService) : base(addressSettings,
                commonSettings,
                addressModelFactory,
                addressService,
                countryService,
                currencyService,
                customerService,
                genericAttributeService,
                localizationService,
                orderProcessingService,
                orderTotalCalculationService,
                paymentPluginManager,
                paymentService,
                pickupPluginManager,
                priceFormatter,
                rewardPointService,
                shippingPluginManager,
                shippingService,
                shoppingCartService,
                stateProvinceService,
                storeContext,
                storeMappingService,
                taxService,
                workContext,
                orderSettings,
                paymentSettings,
                rewardPointsSettings,
                shippingSettings)
        {
            _commonSettings = commonSettings;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _priceFormatter = priceFormatter;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _zimZoneErpSyncService = zimZoneErpSyncService;
            _productService = productService;
        }
        //overriding the method to update current stock from erp
        public override async Task<CheckoutConfirmModel> PrepareConfirmOrderModelAsync(IList<ShoppingCartItem> cart)
        {
            //updating stock from erp
            var skus = (await _productService.GetProductsByIdsAsync(cart.Select(x => x.ProductId).ToArray())).Select(x => x.Sku);
            await _zimZoneErpSyncService.SyncErpStocksAsync(skus.ToList());

            var model = new CheckoutConfirmModel
            {
                //terms of service
                TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage,
                TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks
            };
            //min order amount validation
            var minOrderTotalAmountOk = await _orderProcessingService.ValidateMinOrderTotalAmountAsync(cart);
            if (!minOrderTotalAmountOk)
            {
                var minOrderTotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderTotalAmount, await _workContext.GetWorkingCurrencyAsync());
                model.MinOrderTotalWarning = string.Format(await _localizationService.GetResourceAsync("Checkout.MinOrderTotalAmount"), await _priceFormatter.FormatPriceAsync(minOrderTotalAmount, true, false));
            }
            return model;
        }
    }
}

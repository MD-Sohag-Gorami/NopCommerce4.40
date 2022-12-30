using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.ErpSync;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service;
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
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Factories
{
    public class OverridenShoppingCartModelFactory : ShoppingCartModelFactory
    {
        private readonly CustomerSettings _customerSettings;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly GiftVoucherSettings _giftVoucherSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IErpPictureService _erpPictureService;
        private readonly ErpSyncSettings _erpSyncSettings;
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;
        private readonly IServiceRequestService _serviceRequestService;

        public OverridenShoppingCartModelFactory(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            CustomerSettings customerSettings,
            IAddressModelFactory addressModelFactory,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IProductService productService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            GiftVoucherSettings giftVoucherSettings,
            CurrencySettings currencySettings,
            IErpPictureService erpPictureService,
            ErpSyncSettings erpSyncSettings,
            IZimzoneServiceEntityService zimzoneServiceEntityService,
            IServiceRequestService serviceRequestService) : base(addressSettings,
                captchaSettings,
                catalogSettings,
                commonSettings,
                customerSettings,
                addressModelFactory,
                checkoutAttributeFormatter,
                checkoutAttributeParser,
                checkoutAttributeService,
                countryService,
                currencyService,
                customerService,
                dateTimeHelper,
                discountService,
                downloadService,
                genericAttributeService,
                giftCardService,
                httpContextAccessor,
                localizationService,
                orderProcessingService,
                orderTotalCalculationService,
                paymentPluginManager,
                paymentService,
                permissionService,
                pictureService,
                priceFormatter,
                productAttributeFormatter,
                productService,
                shippingService,
                shoppingCartService,
                stateProvinceService,
                staticCacheManager,
                storeContext,
                storeMappingService,
                taxService,
                urlRecordService,
                vendorService,
                webHelper,
                workContext,
                mediaSettings,
                orderSettings,
                rewardPointsSettings,
                shippingSettings,
                shoppingCartSettings,
                taxSettings,
                vendorSettings)
        {
            _customerSettings = customerSettings;
            _checkoutAttributeService = checkoutAttributeService;
            _currencyService = currencyService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentService = paymentService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatter = productAttributeFormatter;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _giftVoucherSettings = giftVoucherSettings;
            _currencySettings = currencySettings;
            _erpPictureService = erpPictureService;
            _erpSyncSettings = erpSyncSettings;
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
            _serviceRequestService = serviceRequestService;
        }
        protected override async Task<ShoppingCartModel.ShoppingCartItemModel> PrepareShoppingCartItemModelAsync(IList<ShoppingCartItem> cart, ShoppingCartItem sci)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (sci == null)
                throw new ArgumentNullException(nameof(sci));

            var paymentProductIds = await _zimzoneServiceEntityService.GetAllPaymentProductIdAsync();

            var product = await _productService.GetProductByIdAsync(sci.ProductId);

            var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
            {
                Id = sci.Id,
                Sku = await _productService.FormatSkuAsync(product, sci.AttributesXml),
                VendorName = _vendorSettings.ShowVendorOnOrderDetailsPage ? (await _vendorService.GetVendorByProductIdAsync(product.Id))?.Name : string.Empty,
                ProductId = sci.ProductId,
                ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                Quantity = sci.Quantity,
                AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, sci.AttributesXml),
            };

            //added custom name for service product
            if (paymentProductIds.Contains(product.Id))
            {
                if (int.TryParse(sci.AttributesXml, out var requestId))
                {
                    var request = await _serviceRequestService.GetRequestByIdAsync(requestId);
                    if (request != null)
                    {
                        if (!string.IsNullOrEmpty(request.CustomName))
                            cartItemModel.ProductName = request.CustomName;
                        if (request.QuoteDownloadId > 0)
                        {
                            cartItemModel.CustomProperties.Add("QuoteDownloadId", request.QuoteDownloadId);
                            // quote file download link will be added with cartitem
                        }
                    }
                }
            }

            //allow editing?
            //1. setting enabled?
            //2. simple product?
            //3. has attribute or gift card?
            //4. visible individually?
            cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                                             product.ProductType == ProductType.SimpleProduct &&
                                             (!string.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                              product.IsGiftCard) &&
                                             product.VisibleIndividually;

            //disable removal?
            //1. do other items require this one?
            cartItemModel.DisableRemoval = (await _shoppingCartService.GetProductsRequiringProductAsync(cart, product)).Any();

            //allowed quantities
            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            foreach (var qty in allowedQuantities)
            {
                cartItemModel.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = sci.Quantity == qty
                });
            }

            //recurring info
            if (product.IsRecurring)
                cartItemModel.RecurringInfo = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.RecurringPeriod"),
                        product.RecurringCycleLength, await _localizationService.GetLocalizedEnumAsync(product.RecurringCyclePeriod));

            //rental info
            if (product.IsRental)
            {
                var rentalStartDate = sci.RentalStartDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, sci.RentalStartDateUtc.Value)
                    : string.Empty;
                var rentalEndDate = sci.RentalEndDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, sci.RentalEndDateUtc.Value)
                    : string.Empty;
                cartItemModel.RentalInfo =
                    string.Format(await _localizationService.GetResourceAsync("ShoppingCart.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
            }

            //unit prices
            if (product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
            }
            else
            {
                var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
            }
            //subtotal, discount
            if (product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.SubTotal = await _localizationService.GetResourceAsync("Products.CallForPrice");
            }
            else
            {
                //sub total
                var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(sci, true);
                var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
                var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                cartItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);
                cartItemModel.MaximumDiscountedQty = maximumDiscountQty;
                if (product.Sku == _giftVoucherSettings.ElectrosalesGiftProductSku)
                {
                    var primaryCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
                    var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                    var itemPriceInPrimaryCurrency = string.Empty;
                    itemPriceInPrimaryCurrency = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscountBase, true, primaryCurrency.CurrencyCode, languageId, true);
                    cartItemModel.ProductName = cartItemModel.ProductName + $"({itemPriceInPrimaryCurrency})";
                }
                //display an applied discount amount
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    (shoppingCartItemDiscountBase, _) = await _taxService.GetProductPriceAsync(product, shoppingCartItemDiscountBase);
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        var shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                        cartItemModel.Discount = await _priceFormatter.FormatPriceAsync(shoppingCartItemDiscount);
                    }
                }
            }

            //picture
            if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
            {
                cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                    _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
            }

            //item warnings
            var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(
                await _workContext.GetCurrentCustomerAsync(),
                sci.ShoppingCartType,
                product,
                sci.StoreId,
                sci.AttributesXml,
                sci.CustomerEnteredPrice,
                sci.RentalStartDateUtc,
                sci.RentalEndDateUtc,
                sci.Quantity,
                false,
                sci.Id);
            foreach (var warning in itemWarnings)
                cartItemModel.Warnings.Add(warning);

            return cartItemModel;
        }

        public override async Task<MiniShoppingCartModel> PrepareMiniShoppingCartModelAsync()
        {
            var model = new MiniShoppingCartModel
            {
                ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
                //let's always display it
                DisplayShoppingCartButton = true,
                CurrentCustomerIsGuest = await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()),
                AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
            };

            //performance optimization (use "HasShoppingCartItems" property)
            if ((await _workContext.GetCurrentCustomerAsync()).HasShoppingCartItems)
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (cart.Any())
                {
                    model.TotalProducts = cart.Sum(item => item.Quantity);

                    //subtotal
                    var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                    var (_, _, _, subTotalWithoutDiscountBase, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
                    var subtotalBase = subTotalWithoutDiscountBase;
                    var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, await _workContext.GetWorkingCurrencyAsync());
                    model.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal, false, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);

                    var requiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
                    //a customer should visit the shopping cart page (hide checkout button) before going to checkout if:
                    //1. "terms of service" are enabled
                    //2. min order sub-total is OK
                    //3. we have at least one checkout attribute
                    var checkoutAttributesExist = (await _checkoutAttributeService
                        .GetAllCheckoutAttributesAsync((await _storeContext.GetCurrentStoreAsync()).Id, !requiresShipping))
                        .Any();

                    var minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(cart);

                    var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();

                    var downloadableProductsRequireRegistration =
                        _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

                    model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                        minOrderSubtotalAmountOk &&
                        !checkoutAttributesExist &&
                        !(downloadableProductsRequireRegistration
                            && await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()));

                    var paymentProductIds = await _zimzoneServiceEntityService.GetAllPaymentProductIdAsync();

                    //products. sort descending (recently added products)
                    foreach (var sci in cart
                        .OrderByDescending(x => x.Id)
                        .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                        .ToList())
                    {
                        var product = await _productService.GetProductByIdAsync(sci.ProductId);

                        var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel
                        {
                            Id = sci.Id,
                            ProductId = sci.ProductId,
                            ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                            ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                            Quantity = sci.Quantity,
                            AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, sci.AttributesXml)
                        };

                        // concatenate custom name with product name
                        if (paymentProductIds.Contains(product.Id))
                        {
                            if (int.TryParse(sci.AttributesXml, out var requestId))
                            {
                                var request = await _serviceRequestService.GetRequestByIdAsync(requestId);
                                if (request != null && !string.IsNullOrEmpty(request.CustomName))
                                {
                                    cartItemModel.ProductName = request.CustomName;
                                }
                            }
                        }

                        //unit prices
                        if (product.CallForPrice &&
                            //also check whether the current user is impersonated
                            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                        {
                            cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                        }
                        else
                        {
                            var primaryCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
                            var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                            var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
                            cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
                            if (product.Sku == _giftVoucherSettings.ElectrosalesGiftProductSku)
                            {
                                cartItemModel.ProductName += $"({await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscountBase, true, primaryCurrency.CurrencyCode, (await _workContext.GetWorkingLanguageAsync()).Id, true)})";
                            }
                        }

                        //picture
                        if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                        {
                            cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                                _mediaSettings.MiniCartThumbPictureSize, true, cartItemModel.ProductName);
                        }

                        model.Items.Add(cartItemModel);
                    }
                }
            }

            return model;
        }

        public override async Task<PictureModel> PrepareCartItemPictureModelAsync(ShoppingCartItem sci, int pictureSize, bool showDefaultPicture, string productName)
        {
            var pictureCacheKey = _staticCacheManager.PrepareKeyForShortTermCache(NopModelCacheDefaults.CartPictureModelKey
                , sci, pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

            var model = await _staticCacheManager.GetAsync(pictureCacheKey, async () =>
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                //shopping cart item picture
                var sciPicture = await _pictureService.GetProductPictureAsync(product, sci.AttributesXml);

                var erpSciPicture = (await _erpPictureService.GetErpPicturesByNopProductIdAsync(product.Id)).FirstOrDefault();
                if (sciPicture == null && erpSciPicture != null)
                {
                    return new PictureModel
                    {
                        ImageUrl = ErpSyncHelper.PrepareImageUrlWithSize(erpSciPicture.ImageLink, pictureSize, _erpSyncSettings),
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), productName),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), productName),
                    };
                }
                return new PictureModel
                {
                    ImageUrl = (await _pictureService.GetPictureUrlAsync(sciPicture, pictureSize, showDefaultPicture)).Url,
                    Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), productName),
                    AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), productName),
                };
            });

            return model;
        }

        public override async Task<OrderTotalsModel> PrepareOrderTotalsModelAsync(IList<ShoppingCartItem> cart, bool isEditable)
        {
            var model = new OrderTotalsModel
            {
                IsEditable = isEditable
            };

            if (cart.Any())
            {
                //subtotal
                var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                var (orderSubTotalDiscountAmountBase, _, subTotalWithoutDiscountBase, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
                var subtotalBase = subTotalWithoutDiscountBase;
                var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, await _workContext.GetWorkingCurrencyAsync());
                model.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal, true, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);

                if (orderSubTotalDiscountAmountBase > decimal.Zero)
                {
                    var orderSubTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderSubTotalDiscountAmountBase, await _workContext.GetWorkingCurrencyAsync());
                    model.SubTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountAmount, true, await _workContext.GetWorkingCurrencyAsync(), (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);
                }

                //shipping info
                model.RequiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
                if (model.RequiresShipping)
                {
                    var shoppingCartShippingBase = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        var shoppingCartShipping = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartShippingBase.Value, await _workContext.GetWorkingCurrencyAsync());
                        model.Shipping = await _priceFormatter.FormatShippingPriceAsync(shoppingCartShipping, true);

                        //selected shipping method
                        var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(await _workContext.GetCurrentCustomerAsync(),
                            NopCustomerDefaults.SelectedShippingOptionAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                        if (shippingOption != null)
                            model.SelectedShippingMethod = shippingOption.Name;
                    }
                }
                else
                {
                    model.HideShippingTotal = _shippingSettings.HideShippingTotal;
                }

                //payment method fee
                var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart, paymentMethodSystemName);
                var (paymentMethodAdditionalFeeWithTaxBase, _) = await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, await _workContext.GetCurrentCustomerAsync());
                if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
                {
                    var paymentMethodAdditionalFeeWithTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(paymentMethodAdditionalFeeWithTaxBase, await _workContext.GetWorkingCurrencyAsync());
                    model.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeWithTax, true);
                }

                //tax
                bool displayTax;
                bool displayTaxRates;
                if (_taxSettings.HideTaxInOrderSummary && await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    var (shoppingCartTaxBase, taxRates) = await _orderTotalCalculationService.GetTaxTotalAsync(cart);
                    var shoppingCartTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTaxBase, await _workContext.GetWorkingCurrencyAsync());

                    if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                        displayTax = !displayTaxRates;

                        model.Tax = await _priceFormatter.FormatPriceAsync(shoppingCartTax, true, false);
                        foreach (var tr in taxRates)
                        {
                            model.TaxRates.Add(new OrderTotalsModel.TaxRate
                            {
                                Rate = _priceFormatter.FormatTaxRate(tr.Key),
                                Value = await _priceFormatter.FormatPriceAsync(await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(tr.Value, await _workContext.GetWorkingCurrencyAsync()), true, false),
                            });
                        }
                    }
                }

                model.DisplayTaxRates = displayTaxRates;
                model.DisplayTax = displayTax;

                //total
                var (shoppingCartTotalBase, orderTotalDiscountAmountBase, _, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
                if (shoppingCartTotalBase.HasValue)
                {
                    var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, await _workContext.GetWorkingCurrencyAsync());
                    model.OrderTotal = await _priceFormatter.FormatPriceAsync(shoppingCartTotal, true, false);
                }

                //discount
                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    var orderTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderTotalDiscountAmountBase, await _workContext.GetWorkingCurrencyAsync());
                    model.OrderTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderTotalDiscountAmount, true, false);
                }

                //gift cards
                if (appliedGiftCards != null && appliedGiftCards.Any())
                {
                    foreach (var appliedGiftCard in appliedGiftCards)
                    {
                        var gcModel = new OrderTotalsModel.GiftCard
                        {
                            Id = appliedGiftCard.GiftCard.Id,
                            CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
                        };
                        var amountCanBeUsed = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(appliedGiftCard.AmountCanBeUsed, await _workContext.GetWorkingCurrencyAsync());
                        gcModel.Amount = await _priceFormatter.FormatPriceAsync(-amountCanBeUsed, true, false);

                        var remainingAmountBase = await _giftCardService.GetGiftCardRemainingAmountAsync(appliedGiftCard.GiftCard) - appliedGiftCard.AmountCanBeUsed;
                        var remainingAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(remainingAmountBase, await _workContext.GetWorkingCurrencyAsync());
                        gcModel.Remaining = await _priceFormatter.FormatPriceAsync(remainingAmount, true, false);

                        model.GiftCards.Add(gcModel);
                    }
                }

                //reward points to be spent (redeemed)
                if (redeemedRewardPointsAmount > decimal.Zero)
                {
                    var redeemedRewardPointsAmountInCustomerCurrency = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(redeemedRewardPointsAmount, await _workContext.GetWorkingCurrencyAsync());
                    model.RedeemedRewardPoints = redeemedRewardPoints;
                    model.RedeemedRewardPointsAmount = await _priceFormatter.FormatPriceAsync(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
                }

                //reward points to be earned
                if (_rewardPointsSettings.Enabled && _rewardPointsSettings.DisplayHowMuchWillBeEarned && shoppingCartTotalBase.HasValue)
                {
                    //get shipping total
                    var shippingBaseInclTax = !model.RequiresShipping ? 0 : (await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, true)).shippingTotal ?? 0;

                    //get total for reward points
                    var totalForRewardPoints = _orderTotalCalculationService
                        .CalculateApplicableOrderTotalForRewardPoints(shippingBaseInclTax, shoppingCartTotalBase.Value);
                    if (totalForRewardPoints > decimal.Zero)
                        model.WillEarnRewardPoints = await _orderTotalCalculationService.CalculateRewardPointsAsync(await _workContext.GetCurrentCustomerAsync(), totalForRewardPoints);
                }
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            var sci = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart);
            var itemsProductIds = sci.Select(x => x.ProductId).ToArray();
            var productsSkus = (await _productService.GetProductsByIdsAsync(itemsProductIds)).Select(x => x.Sku).ToList();

            var hasElectrosalesVoucher = productsSkus.Contains(_giftVoucherSettings.ElectrosalesGiftProductSku);
            var hasOnlyElectrosalesVoucher = hasElectrosalesVoucher && productsSkus.Count == 1;
            var customData = new
            {
                TaxFieldKey = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Vat"),
                TaxFieldValues = new List<string>()
            };
            if (!hasOnlyElectrosalesVoucher)
            {
                customData.TaxFieldValues.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Tax.TaxMessage"));
            }
            if (hasElectrosalesVoucher)
            {
                customData.TaxFieldValues.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Tax.NoVatOnElectrosales"));
            }
            model.CustomProperties.Add("OrderTotalCustomData", customData);

            return model;
        }

    }
}

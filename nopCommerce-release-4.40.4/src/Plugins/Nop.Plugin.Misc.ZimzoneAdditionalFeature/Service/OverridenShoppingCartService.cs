using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public class OverridenShoppingCartService : ShoppingCartService
    {
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly IWorkContext _workContext;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IGiftCardAttributeParser _giftCardAttributeParser;
        private readonly GiftVoucherSettings _giftVoucherSettings;
        private readonly IZimzoneServiceEntityService _zimzoneServices;
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IZimZoneErpSyncService _zimZoneErpSyncService;

        public OverridenShoppingCartService(CatalogSettings catalogSettings,
            IAclService aclService,
            IActionContextAccessor actionContextAccessor,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            IDateTimeHelper dateTimeHelper,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IRepository<ShoppingCartItem> sciRepository,
            IShippingService shippingService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            OrderSettings orderSettings,
            ShoppingCartSettings shoppingCartSettings,
            IGiftCardAttributeParser giftCardAttributeParser,
            GiftVoucherSettings giftVoucherSettings,
            IZimzoneServiceEntityService zimzoneServices,
            IServiceRequestService serviceRequestService,
            IZimZoneErpSyncService zimZoneErpSyncService) : base(catalogSettings,
                aclService,
                actionContextAccessor,
                checkoutAttributeParser,
                checkoutAttributeService,
                currencyService,
                customerService,
                dateRangeService,
                dateTimeHelper,
                genericAttributeService,
                localizationService,
                permissionService,
                priceCalculationService,
                priceFormatter,
                productAttributeParser,
                productAttributeService,
                productService,
                sciRepository,
                shippingService,
                staticCacheManager,
                storeContext,
                storeMappingService,
                urlHelperFactory,
                urlRecordService,
                workContext,
                orderSettings,
                shoppingCartSettings)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _productAttributeParser = productAttributeParser;
            _sciRepository = sciRepository;
            _workContext = workContext;
            _shoppingCartSettings = shoppingCartSettings;
            _giftCardAttributeParser = giftCardAttributeParser;
            _giftVoucherSettings = giftVoucherSettings;
            _zimzoneServices = zimzoneServices;
            _serviceRequestService = serviceRequestService;
            _zimZoneErpSyncService = zimZoneErpSyncService;
        }

        protected override async Task<bool> ShoppingCartItemIsEqualAsync(ShoppingCartItem shoppingCartItem,
            Product product,
            string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate,
            DateTime? rentalEndDate)
        {
            if (shoppingCartItem.ProductId != product.Id)
                return false;

            //attributes
            var isPaymentProduct = (await _zimzoneServices.GetAllPaymentProductIdAsync()).Contains(product.Id);
            if(isPaymentProduct)
            {
                if(shoppingCartItem.AttributesXml != attributesXml)
                {
                    return false;
                }
            }
            var attributesEqual = await _productAttributeParser.AreProductAttributesEqualAsync(shoppingCartItem.AttributesXml, attributesXml, false, false);
            if (!attributesEqual)
                return false;

            //gift cards
            if (product.IsGiftCard)
            {
                _productAttributeParser.GetGiftCardAttribute(attributesXml, out var giftCardRecipientName1, out var _, out var giftCardSenderName1, out var _, out var _);

                _productAttributeParser.GetGiftCardAttribute(shoppingCartItem.AttributesXml, out var giftCardRecipientName2, out var _, out var giftCardSenderName2, out var _, out var _);

                var giftCardsAreEqual = giftCardRecipientName1.Equals(giftCardRecipientName2, StringComparison.InvariantCultureIgnoreCase)
                    && giftCardSenderName1.Equals(giftCardSenderName2, StringComparison.InvariantCultureIgnoreCase);
                if (!giftCardsAreEqual)
                    return false;
            }

            //price is the same (for products which require customers to enter a price)
            if (product.CustomerEntersPrice)
            {
                //we use rounding to eliminate errors associated with storing real numbers in memory when comparing
                var customerEnteredPricesEqual = Math.Round(shoppingCartItem.CustomerEnteredPrice, 2) == Math.Round(customerEnteredPrice, 2);
                if (!customerEnteredPricesEqual)
                    return false;
            }

            if (!product.IsRental)
                return true;

            //rental products
            var rentalInfoEqual = shoppingCartItem.RentalStartDateUtc == rentalStartDate && shoppingCartItem.RentalEndDateUtc == rentalEndDate;

            return rentalInfoEqual;
        }

        public override async Task<IList<string>> AddToCartAsync(Customer customer, Product product,
            ShoppingCartType shoppingCartType, int storeId, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));


            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }

            if (shoppingCartType == ShoppingCartType.Wishlist && !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }

            if (customer.IsSearchEngineAccount())
            {
                warnings.Add("Search engine can't add to cart");
                return warnings;
            }

            if (quantity <= 0)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.QuantityShouldPositive"));
                return warnings;
            }
            //updating stock
            product = await _zimZoneErpSyncService.SyncErpStockAsync(product);
            //reset checkout info
            await _customerService.ResetCheckoutDataAsync(customer, storeId);

            var cart = await GetShoppingCartAsync(customer, shoppingCartType, storeId);

            var shoppingCartItem = await FindShoppingCartItemInTheCartAsync(cart,
                shoppingCartType, product, attributesXml, customerEnteredPrice,
                rentalStartDate, rentalEndDate);

            if (shoppingCartItem != null)
            {
                //update existing shopping cart item
                var newQuantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(await GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product,
                    storeId, attributesXml,
                    customerEnteredPrice, rentalStartDate, rentalEndDate,
                    newQuantity, addRequiredProducts, shoppingCartItem.Id));

                if (warnings.Any())
                    return warnings;

                shoppingCartItem.AttributesXml = attributesXml;
                shoppingCartItem.Quantity = newQuantity;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                await _sciRepository.UpdateAsync(shoppingCartItem);
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(await GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product,
                    storeId, attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate,
                    quantity, addRequiredProducts));

                if (warnings.Any())
                    return warnings;

                //maximum items validation
                switch (shoppingCartType)
                {
                    case ShoppingCartType.ShoppingCart:
                        if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                        {
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));
                            return warnings;
                        }

                        break;
                    case ShoppingCartType.Wishlist:
                        if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                        {
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                            return warnings;
                        }

                        break;
                    default:
                        break;
                }

                var now = DateTime.UtcNow;
                var services = (await _zimzoneServices.GetAllZimzoneServiceAsync()).Where(x => x.IsActive && x.ServiceProductId > 0);
                var serviceProductIds = services.Select(x => x.ServiceProductId).ToArray();
                if (serviceProductIds.Contains(product.Id))
                {
                    if (await _customerService.IsGuestAsync(customer))
                    {
                        warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.ServiceRequest.LoginMessage"));
                        return warnings;
                    }
                    var service = services.Where(x => x.ServiceProductId == product.Id).FirstOrDefault();
                    var (name, email, address, description, downloadId) = await _serviceRequestService.ParseServiceRequestAttributeAsync(attributesXml, product);
                    var request = new ZimzoneServiceRequestEntity()
                    {
                        CustomerId = customer?.Id ?? 0,
                        CustomerEmail = email,
                        CustomerName = name,
                        CustomerAddress = address,
                        CreatedOn = DateTime.UtcNow,
                        ZimZoneServiceId = service?.Id ?? 0,
                        ServiceDetailsAttr = attributesXml,
                        Description = description
                    };
                    await _serviceRequestService.InsertRequestAsync(request);
                    return warnings;
                }
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartType = shoppingCartType,
                    StoreId = storeId,
                    ProductId = product.Id,
                    AttributesXml = attributesXml,
                    CustomerEnteredPrice = customerEnteredPrice,
                    Quantity = quantity,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate,
                    CreatedOnUtc = now,
                    UpdatedOnUtc = now,
                    CustomerId = customer.Id
                };

                await _sciRepository.InsertAsync(shoppingCartItem);

                //updated "HasShoppingCartItems" property used for performance optimization
                customer.HasShoppingCartItems = !IsCustomerShoppingCartEmpty(customer);

                await _customerService.UpdateCustomerAsync(customer);
            }

            return warnings;
        }
        public override async Task<IList<string>> GetShoppingCartItemGiftCardWarningsAsync(ShoppingCartType shoppingCartType,
           Product product, string attributesXml)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();

            //gift cards
            if (!product.IsGiftCard)
                return warnings;

            _giftCardAttributeParser.GetGiftCardAttribute(attributesXml, out var giftCardFirstName,
                out var giftCardLastName, out var giftCardRecipientEmail, out var giftCardSenderName,
                out var giftCardSenderEmail, out var giftCardMessage, out var giftCardPhysicalAddress,
                out var giftCardCellPhoneNumber, out var giftCardIdOrPassportNumber, out var giftCardDeliveryDate);

            var isZimazonProduct = product.Sku.Equals(_giftVoucherSettings.ZimazonGiftProductSku);
            var isElectrosalesProduct = product.Sku.Equals(_giftVoucherSettings.ElectrosalesGiftProductSku);
            //if(isElectrosalesProduct && _workContext.GetWorkingCurrencyAsync().Result.CurrencyCode!= GiftVoucherDefaults.USD_CURRENCY_CODE)
            //{
            //    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.CurrencyError"));
            //}
            if (product.GiftCardType == GiftCardType.Virtual)
            {
                //validate for virtual gift cards only


                if (string.IsNullOrEmpty(giftCardFirstName))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.FirstNameError"));

                if (string.IsNullOrEmpty(giftCardLastName))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.LastNameError"));

                if (string.IsNullOrEmpty(giftCardSenderName))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.SenderNameError"));

                if (string.IsNullOrEmpty(giftCardSenderEmail) || !CommonHelper.IsValidEmail(giftCardSenderEmail))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.SenderEmailError"));

            }
            if (isZimazonProduct)
            {
                if (_giftVoucherSettings.RequireRecipientEmailZimazon && (string.IsNullOrEmpty(giftCardRecipientEmail) || !CommonHelper.IsValidEmail(giftCardRecipientEmail)))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.RecipientEmailError"));

                if (_giftVoucherSettings.EnablePhysicalAddressZimazon && _giftVoucherSettings.RequirePhysicalAddressZimazon && string.IsNullOrEmpty(giftCardPhysicalAddress))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.PhysicalAddressError"));

                if (_giftVoucherSettings.EnableCellPhoneNumberZimazon && _giftVoucherSettings.RequireCellPhoneNumberZimazon && string.IsNullOrEmpty(giftCardCellPhoneNumber))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.CellPhoneNumberError"));

                if (_giftVoucherSettings.EnableIdOrPassportNumberZimazon && _giftVoucherSettings.RequireIdOrPassportNumberZimazon && string.IsNullOrEmpty(giftCardIdOrPassportNumber))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.IdOrPassportNumberError"));

            }
            if (isElectrosalesProduct)
            {
                if (_giftVoucherSettings.RequireRecipientEmailElectrosales && (string.IsNullOrEmpty(giftCardRecipientEmail) || !CommonHelper.IsValidEmail(giftCardRecipientEmail)))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.RecipientEmailError"));

                if (_giftVoucherSettings.EnablePhysicalAddressElectrosales && _giftVoucherSettings.RequirePhysicalAddressElectrosales && string.IsNullOrEmpty(giftCardPhysicalAddress))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.PhysicalAddressError"));

                if (_giftVoucherSettings.EnableCellPhoneNumberElectrosales && _giftVoucherSettings.RequireCellPhoneNumberElectrosales && string.IsNullOrEmpty(giftCardCellPhoneNumber))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.CellPhoneNumberError"));

                if (_giftVoucherSettings.EnableIdOrPassportNumberElectrosales && _giftVoucherSettings.RequireIdOrPassportNumberElectrosales && string.IsNullOrEmpty(giftCardIdOrPassportNumber))
                    warnings.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.IdOrPassportNumberError"));

            }

            return warnings;
        }
    }
}

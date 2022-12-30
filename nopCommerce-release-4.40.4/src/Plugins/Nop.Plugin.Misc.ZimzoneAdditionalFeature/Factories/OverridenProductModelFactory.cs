using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.ErpSync;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public class OverridenProductModelFactory : ProductModelFactory
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateRangeService _dateRangeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly SeoSettings _seoSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IGiftCardAttributeParser _giftCardAttributeParser;
        private readonly IErpPictureService _erpPictureService;
        private readonly ErpSyncSettings _erpSyncSettings;
        private readonly GiftVoucherSettings _giftVoucherSettings;
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;
        private readonly ServiceAttributeSettings _serviceAttributeSettings;

        public OverridenProductModelFactory(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CustomerSettings customerSettings,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            IReviewTypeService reviewTypeService,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IShoppingCartModelFactory shoppingCartModelFactory,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            SeoSettings seoSettings,
            ShippingSettings shippingSettings,
            VendorSettings vendorSettings,
            IGiftCardAttributeParser giftCardAttributeParser,
            IErpPictureService erpPictureService,
            ErpSyncSettings erpSyncSettings,
            GiftVoucherSettings giftVoucherSettings,
            IZimzoneServiceEntityService zimzoneServiceEntityService,
            ServiceAttributeSettings serviceAttributeSettings) : base(captchaSettings,
                catalogSettings,
                customerSettings,
                categoryService,
                currencyService,
                customerService,
                dateRangeService,
                dateTimeHelper,
                downloadService,
                genericAttributeService,
                localizationService,
                manufacturerService,
                permissionService,
                pictureService,
                priceCalculationService,
                priceFormatter,
                productAttributeParser,
                productAttributeService,
                productService,
                productTagService,
                productTemplateService,
                reviewTypeService,
                specificationAttributeService,
                staticCacheManager,
                storeContext,
                shoppingCartModelFactory,
                taxService,
                urlRecordService,
                vendorService,
                webHelper,
                workContext,
                mediaSettings,
                orderSettings,
                seoSettings,
                shippingSettings,
                vendorSettings)
        {
            _catalogSettings = catalogSettings;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateRangeService = dateRangeService;
            _dateTimeHelper = dateTimeHelper;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _seoSettings = seoSettings;
            _shippingSettings = shippingSettings;
            _vendorSettings = vendorSettings;
            _giftCardAttributeParser = giftCardAttributeParser;
            _erpPictureService = erpPictureService;
            _erpSyncSettings = erpSyncSettings;
            _giftVoucherSettings = giftVoucherSettings;
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
            _serviceAttributeSettings = serviceAttributeSettings;
        }

        protected override async Task<ProductDetailsModel.AddToCartModel> PrepareProductAddToCartModelAsync(Product product, ShoppingCartItem updatecartitem)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductDetailsModel.AddToCartModel
            {
                ProductId = product.Id
            };

            if (updatecartitem != null)
            {
                model.UpdatedShoppingCartItemId = updatecartitem.Id;
                model.UpdateShoppingCartItemType = updatecartitem.ShoppingCartType;
            }

            //quantity
            model.EnteredQuantity = updatecartitem != null ? updatecartitem.Quantity : product.OrderMinimumQuantity;
            //allowed quantities
            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            foreach (var qty in allowedQuantities)
            {
                model.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = updatecartitem != null && updatecartitem.Quantity == qty
                });
            }
            //minimum quantity notification
            if (product.OrderMinimumQuantity > 1)
            {
                model.MinimumQuantityNotification = string.Format(await _localizationService.GetResourceAsync("Products.MinimumQuantityNotification"), product.OrderMinimumQuantity);
            }

            //'add to cart', 'add to wishlist' buttons
            model.DisableBuyButton = product.DisableBuyButton || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart);
            model.DisableWishlistButton = product.DisableWishlistButton || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist);
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                model.DisableBuyButton = true;
                model.DisableWishlistButton = true;
            }
            //pre-order
            if (product.AvailableForPreOrder)
            {
                model.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                    product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                model.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;

                if (model.AvailableForPreOrder &&
                    model.PreOrderAvailabilityStartDateTimeUtc.HasValue &&
                    _catalogSettings.DisplayDatePreOrderAvailability)
                {
                    model.PreOrderAvailabilityStartDateTimeUserTime =
                        (await _dateTimeHelper.ConvertToUserTimeAsync(model.PreOrderAvailabilityStartDateTimeUtc.Value)).ToString("D");
                }
            }
            //rental
            model.IsRental = product.IsRental;

            //customer entered price
            model.CustomerEntersPrice = product.CustomerEntersPrice;
            if (!model.CustomerEntersPrice)
                return model;

            var minimumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MinimumCustomerEnteredPrice, await _workContext.GetWorkingCurrencyAsync());
            var maximumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MaximumCustomerEnteredPrice, await _workContext.GetWorkingCurrencyAsync());

            if (product.IsGiftCard)
            {
                if (minimumCustomerEnteredPrice < 10)
                {
                    minimumCustomerEnteredPrice = 10;
                }
            }

            model.CustomerEnteredPrice = updatecartitem != null ? updatecartitem.CustomerEnteredPrice : minimumCustomerEnteredPrice;
            model.CustomerEnteredPriceRange = string.Format(await _localizationService.GetResourceAsync("Products.EnterProductPrice.Range"),
                await _priceFormatter.FormatPriceAsync(minimumCustomerEnteredPrice, false, false),
                await _priceFormatter.FormatPriceAsync(maximumCustomerEnteredPrice, false, false));

            return model;
        }

        public override async Task<ProductDetailsModel> PrepareProductDetailsModelAsync(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //standard properties
            var model = new ProductDetailsModel
            {
                Id = product.Id,
                Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                MetaKeywords = await _localizationService.GetLocalizedAsync(product, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(product, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(product, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(product),
                ProductType = product.ProductType,
                ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage,
                Sku = product.Sku,
                ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                FreeShippingNotificationEnabled = _catalogSettings.ShowFreeShippingNotification,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                ShowGtin = _catalogSettings.ShowGtin,
                Gtin = product.Gtin,
                ManageInventoryMethod = product.ManageInventoryMethod,
                StockAvailability = await _productService.FormatStockMessageAsync(product, string.Empty),
                HasSampleDownload = product.IsDownload && product.HasSampleDownload,
                DisplayDiscontinuedMessage = !product.Published && _catalogSettings.DisplayDiscontinuedMessageForUnpublishedProducts,
                AvailableEndDate = product.AvailableEndDateTimeUtc,
                VisibleIndividually = product.VisibleIndividually,
                AllowAddingOnlyExistingAttributeCombinations = product.AllowAddingOnlyExistingAttributeCombinations
            };

            //automatically generate product description?
            if (_seoSettings.GenerateProductMetaDescription && string.IsNullOrEmpty(model.MetaDescription))
            {
                //based on short description
                model.MetaDescription = model.ShortDescription;
            }

            //shipping info
            model.IsShipEnabled = product.IsShipEnabled;
            model.IsShipEnabled = product.IsShipEnabled;
            if (product.IsShipEnabled)
            {
                model.IsFreeShipping = product.IsFreeShipping;
                //delivery date
                var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(product.DeliveryDateId);
                if (deliveryDate != null)
                {
                    model.DeliveryDate = await _localizationService.GetLocalizedAsync(deliveryDate, dd => dd.Name);
                }
            }

            //email a friend
            model.EmailAFriendEnabled = _catalogSettings.EmailAFriendEnabled;
            //compare products
            model.CompareProductsEnabled = _catalogSettings.CompareProductsEnabled;
            //store name
            model.CurrentStoreName = await _localizationService.GetLocalizedAsync(await _storeContext.GetCurrentStoreAsync(), x => x.Name);

            //vendor details
            if (_vendorSettings.ShowVendorOnProductDetailsPage)
            {
                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    model.ShowVendor = true;

                    model.VendorModel = new VendorBriefInfoModel
                    {
                        Id = vendor.Id,
                        Name = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(vendor),
                    };
                }
            }

            //page sharing
            if (_catalogSettings.ShowShareButton && !string.IsNullOrEmpty(_catalogSettings.PageShareCode))
            {
                var shareCode = _catalogSettings.PageShareCode;
                if (_webHelper.IsCurrentConnectionSecured())
                {
                    //need to change the add this link to be https linked when the page is, so that the page doesn't ask about mixed mode when viewed in https...
                    shareCode = shareCode.Replace("http://", "https://");
                }

                model.PageShareCode = shareCode;
            }

            switch (product.ManageInventoryMethod)
            {
                case ManageInventoryMethod.ManageStock:
                    model.InStock = product.BackorderMode != BackorderMode.NoBackorders
                        || await _productService.GetTotalStockQuantityAsync(product) > 0;
                    model.DisplayBackInStockSubscription = !model.InStock && product.AllowBackInStockSubscriptions;
                    break;

                case ManageInventoryMethod.ManageStockByAttributes:
                    model.InStock = (await _productAttributeService
                        .GetAllProductAttributeCombinationsAsync(product.Id))
                        ?.Any(c => c.StockQuantity > 0 || c.AllowOutOfStockOrders)
                        ?? false;
                    break;
            }

            //breadcrumb
            //do not prepare this model for the associated products. anyway it's not used
            if (_catalogSettings.CategoryBreadcrumbEnabled && !isAssociatedProduct)
            {
                model.Breadcrumb = await PrepareProductBreadcrumbModelAsync(product);
            }

            //product tags
            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                model.ProductTags = await PrepareProductTagModelsAsync(product);
            }

            //pictures
            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            IList<PictureModel> allPictureModels;
            (model.DefaultPictureModel, allPictureModels) = await PrepareProductDetailsPictureModelAsync(product, isAssociatedProduct);
            model.PictureModels = allPictureModels;

            //price
            model.ProductPrice = await PrepareProductPriceModelAsync(product);

            //'Add to cart' model
            model.AddToCart = await PrepareProductAddToCartModelAsync(product, updatecartitem);

            //gift card
            if (product.IsGiftCard)
            {
                model.GiftCard.IsGiftCard = true;
                model.GiftCard.GiftCardType = product.GiftCardType;
                _giftCardAttributeParser.GetGiftCardAttribute(updatecartitem?.AttributesXml ?? string.Empty, out var giftCardFirstName,
            out var giftCardLastName, out var giftCardRecipientEmail, out var giftCardSenderName,
            out var giftCardSenderEmail, out var giftCardMessage,
            out var giftCardPhysicalAddress, out var giftCardCellPhoneNumber,
            out var giftCardIdOrPassportNumber, out var deliveryDate);

                if (updatecartitem == null)
                {
                    model.GiftCard.SenderName = await _customerService.GetCustomerFullNameAsync(await _workContext.GetCurrentCustomerAsync());
                    model.GiftCard.SenderEmail = (await _workContext.GetCurrentCustomerAsync()).Email;
                    var additionalGiftCardModel = new GiftCardAdditionalDataModel
                    {
                        GiftCardType = model.GiftCard.GiftCardType,
                        ProductId = model.Id,
                        ProductSku = model.Sku,
                        IsGiftCard = model.GiftCard.IsGiftCard,
                        FirstName = giftCardFirstName,
                        LastName = giftCardLastName,
                        RecipientEmail = giftCardRecipientEmail,
                        SenderName = model.GiftCard.SenderName,
                        SenderEmail = model.GiftCard.SenderEmail,
                        PhysicalAddress = giftCardPhysicalAddress,
                        CellPhoneNumber = giftCardCellPhoneNumber,
                        IdOrPassportNumber = giftCardIdOrPassportNumber,
                        GiftCardDeliveryDate = DateTime.UtcNow,
                        HasValidCurrency = true
                    };

                    if (product.Sku.Equals(_giftVoucherSettings.ZimazonGiftProductSku))
                    {
                        additionalGiftCardModel.Message = _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Zimazon.DefaultMessage").Result;
                    }
                    if (product.Sku.Equals(_giftVoucherSettings.ElectrosalesGiftProductSku))
                    {
                        additionalGiftCardModel.Message = _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Electrosales.DefaultMessage").Result;
                        var activeCurrency = await _workContext.GetWorkingCurrencyAsync();

                        if (activeCurrency.CurrencyCode != GiftVoucherDefaults.USD_CURRENCY_CODE)
                        {
                            additionalGiftCardModel.HasValidCurrency = false;
                            additionalGiftCardModel.ElectrosalesVoucherCurrencyErrorMessage = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Electrosales.ChangeCurrencyToUsdMessage");
                        }
                    }
                    await PrepareSettingsForVoucher(_giftVoucherSettings, product, additionalGiftCardModel);
                    model.CustomProperties.Add("GiftCardAdditionalDataModel", additionalGiftCardModel);
                }
                else
                {
                    model.GiftCard.RecipientName = giftCardFirstName + " " + giftCardLastName;
                    model.GiftCard.RecipientEmail = giftCardRecipientEmail;
                    model.GiftCard.SenderName = giftCardSenderName;
                    model.GiftCard.SenderEmail = giftCardSenderEmail;
                    model.GiftCard.Message = giftCardMessage;
                    var additionalGiftCardModel = new GiftCardAdditionalDataModel
                    {
                        GiftCardType = model.GiftCard.GiftCardType,
                        ProductId = model.Id,
                        ProductSku = model.Sku,
                        IsGiftCard = model.GiftCard.IsGiftCard,
                        FirstName = giftCardFirstName,
                        LastName = giftCardLastName,
                        RecipientEmail = giftCardRecipientEmail,
                        SenderName = giftCardSenderName,
                        SenderEmail = giftCardSenderEmail,
                        Message = giftCardMessage,
                        PhysicalAddress = giftCardPhysicalAddress,
                        CellPhoneNumber = giftCardCellPhoneNumber,
                        IdOrPassportNumber = giftCardIdOrPassportNumber,
                        GiftCardDeliveryDate = DateTime.UtcNow,
                        HasValidCurrency = true
                    };
                    await PrepareSettingsForVoucher(_giftVoucherSettings, product, additionalGiftCardModel);
                    model.CustomProperties.Add("GiftCardAdditionalDataModel", additionalGiftCardModel);
                }

            }

            #region checkServiceProduct

            if (IsServiceProduct(product.Id))
            {
                string error = null;
                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()))
                {
                    error = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.ServiceRequest.LoginMessage");
                }
                model.CustomProperties.Add("ServiceProductInfo",
                new AdditionalServiceProductInformationModel()
                {
                    IsServiceProduct = true,
                    SubmitButtonText = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Request.Sumbmit.Text"),
                    Errors = new List<string>()
                    {
                        error
                    }
                });
            }

            #endregion

            //product attributes
            model.ProductAttributes = await PrepareProductAttributeModelsAsync(product, updatecartitem);

            //product specifications
            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);
            }

            //product review overview
            model.ProductReviewOverview = await PrepareProductReviewOverviewModelAsync(product);

            //tier prices
            if (product.HasTierPrices && await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                model.TierPrices = await PrepareProductTierPriceModelsAsync(product);
            }

            //manufacturers
            model.ProductManufacturers = await PrepareProductManufacturerModelsAsync(product);

            //rental products
            if (product.IsRental)
            {
                model.IsRental = true;
                //set already entered dates attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    model.RentalStartDate = updatecartitem.RentalStartDateUtc;
                    model.RentalEndDate = updatecartitem.RentalEndDateUtc;
                }
            }

            //estimate shipping
            if (_shippingSettings.EstimateShippingProductPageEnabled && !model.IsFreeShipping)
            {
                var wrappedProduct = new ShoppingCartItem
                {
                    StoreId = (await _storeContext.GetCurrentStoreAsync()).Id,
                    ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                    ProductId = product.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };

                var estimateShippingModel = await _shoppingCartModelFactory.PrepareEstimateShippingModelAsync(new[] { wrappedProduct });

                model.ProductEstimateShipping.ProductId = product.Id;
                model.ProductEstimateShipping.RequestDelay = estimateShippingModel.RequestDelay;
                model.ProductEstimateShipping.Enabled = estimateShippingModel.Enabled;
                model.ProductEstimateShipping.CountryId = estimateShippingModel.CountryId;
                model.ProductEstimateShipping.StateProvinceId = estimateShippingModel.StateProvinceId;
                model.ProductEstimateShipping.ZipPostalCode = estimateShippingModel.ZipPostalCode;
                model.ProductEstimateShipping.UseCity = estimateShippingModel.UseCity;
                model.ProductEstimateShipping.City = estimateShippingModel.City;
                model.ProductEstimateShipping.AvailableCountries = estimateShippingModel.AvailableCountries;
                model.ProductEstimateShipping.AvailableStates = estimateShippingModel.AvailableStates;
            }

            //associated products
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //ensure no circular references
                if (!isAssociatedProduct)
                {
                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, (await _storeContext.GetCurrentStoreAsync()).Id);
                    foreach (var associatedProduct in associatedProducts)
                        model.AssociatedProducts.Add(await PrepareProductDetailsModelAsync(associatedProduct, null, true));
                }
            }

            return model;
        }

        protected async override Task<IList<ProductDetailsModel.ProductAttributeModel>> PrepareProductAttributeModelsAsync(Product product, ShoppingCartItem updatecartitem)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new List<ProductDetailsModel.ProductAttributeModel>();

            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                var attributeModel = new ProductDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Name),
                    Description = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Description),
                    TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = updatecartitem != null ? null : await _localizationService.GetLocalizedAsync(attribute, x => x.DefaultValue),
                    HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected,
                            CustomerEntersQty = attributeValue.CustomerEntersQty,
                            Quantity = attributeValue.Quantity
                        };
                        attributeModel.Values.Add(valueModel);

                        //display price if allowed
                        if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
                        {
                            var customer = updatecartitem?.CustomerId is null ? await _workContext.GetCurrentCustomerAsync() : await _customerService.GetCustomerByIdAsync(updatecartitem.CustomerId);

                            var attributeValuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer);
                            var (priceAdjustmentBase, _) = await _taxService.GetProductPriceAsync(product, attributeValuePriceAdjustment);
                            var priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase, await _workContext.GetWorkingCurrencyAsync());

                            if (attributeValue.PriceAdjustmentUsePercentage)
                            {
                                var priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
                                if (attributeValue.PriceAdjustment > decimal.Zero)
                                    valueModel.PriceAdjustment = "+";
                                valueModel.PriceAdjustment += priceAdjustmentStr + "%";
                            }
                            else
                            {
                                if (priceAdjustmentBase > decimal.Zero)
                                    valueModel.PriceAdjustment = "+" + await _priceFormatter.FormatPriceAsync(priceAdjustment, false, false);
                                else if (priceAdjustmentBase < decimal.Zero)
                                    valueModel.PriceAdjustment = "-" + await _priceFormatter.FormatPriceAsync(-priceAdjustment, false, false);
                            }

                            valueModel.PriceAdjustmentValue = priceAdjustment;
                        }

                        //"image square" picture (with with "image squares" attribute type only)
                        if (attributeValue.ImageSquaresPictureId > 0)
                        {
                            var productAttributeImageSquarePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributeImageSquarePictureModelKey
                                , attributeValue.ImageSquaresPictureId,
                                    _webHelper.IsCurrentConnectionSecured(),
                                    await _storeContext.GetCurrentStoreAsync());
                            valueModel.ImageSquaresPictureModel = await _staticCacheManager.GetAsync(productAttributeImageSquarePictureCacheKey, async () =>
                            {
                                var imageSquaresPicture = await _pictureService.GetPictureByIdAsync(attributeValue.ImageSquaresPictureId);
                                string fullSizeImageUrl, imageUrl;
                                (imageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture, _mediaSettings.ImageSquarePictureSize);
                                (fullSizeImageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture);

                                if (imageSquaresPicture != null)
                                {
                                    return new PictureModel
                                    {
                                        FullSizeImageUrl = fullSizeImageUrl,
                                        ImageUrl = imageUrl
                                    };
                                }

                                return new PictureModel();
                            });
                        }

                        //picture of a product attribute value
                        valueModel.PictureId = attributeValue.PictureId;
                    }
                }

                //set already selected attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    //clear default selection
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    //select new values
                                    var selectedValues = await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml);
                                    foreach (var attributeValue in selectedValues)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                            {
                                                item.IsPreSelected = true;

                                                //set customer entered quantity
                                                if (attributeValue.CustomerEntersQty)
                                                    item.Quantity = attributeValue.Quantity;
                                            }
                                }
                            }

                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                            {
                                //values are already pre-set

                                //set customer entered quantity
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    foreach (var attributeValue in (await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml))
                                        .Where(value => value.CustomerEntersQty))
                                    {
                                        var item = attributeModel.Values.FirstOrDefault(value => value.Id == attributeValue.Id);
                                        if (item != null)
                                            item.Quantity = attributeValue.Quantity;
                                    }
                                }
                            }

                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var enteredText = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                    if (enteredText.Any())
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }

                            break;
                        case AttributeControlType.Datepicker:
                            {
                                //keep in mind my that the code below works only in the current culture
                                var selectedDateStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                if (selectedDateStr.Any())
                                {
                                    if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture, DateTimeStyles.None, out var selectedDate))
                                    {
                                        //successfully parsed
                                        attributeModel.SelectedDay = selectedDate.Day;
                                        attributeModel.SelectedMonth = selectedDate.Month;
                                        attributeModel.SelectedYear = selectedDate.Year;
                                    }
                                }
                            }

                            break;
                        case AttributeControlType.FileUpload:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var downloadGuidStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id).FirstOrDefault();
                                    Guid.TryParse(downloadGuidStr, out var downloadGuid);
                                    var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                    if (download != null)
                                        attributeModel.DefaultValue = download.DownloadGuid.ToString();
                                }
                            }

                            break;
                        default:
                            break;
                    }
                }


                var currentCustomer = await _workContext.GetCurrentCustomerAsync();

                if (attributeModel.ProductAttributeId == _serviceAttributeSettings.NameAttributeId)
                {
                    attributeModel.CustomProperties.Add("IsEditable", false);

                    if (await _customerService.IsRegisteredAsync(currentCustomer) && string.IsNullOrEmpty(attributeModel.DefaultValue))
                    {
                        attributeModel.DefaultValue = await _customerService.GetCustomerFullNameAsync(currentCustomer);
                    }
                }

                if (attributeModel.ProductAttributeId == _serviceAttributeSettings.EmailAttributeId)
                {
                    attributeModel.CustomProperties.Add("IsEditable", false);

                    if (await _customerService.IsRegisteredAsync(currentCustomer) && string.IsNullOrEmpty(attributeModel.DefaultValue))
                    {
                        attributeModel.DefaultValue = currentCustomer.Email;
                    }
                }

                model.Add(attributeModel);
            }

            return model;
        }

        public async override Task<IEnumerable<ProductOverviewModel>> PrepareProductOverviewModelsAsync(IEnumerable<Product> products, bool preparePriceModel = true, bool preparePictureModel = true, int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false, bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            string error = null;
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()))
            {
                error = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.ServiceRequest.LoginMessage");
            }

            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                    ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                    FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                    SeName = await _urlRecordService.GetSeNameAsync(product),
                    Sku = product.Sku,
                    ProductType = product.ProductType,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)
                };

                //price
                if (preparePriceModel)
                {
                    model.ProductPrice = await PrepareProductOverviewPriceModelAsync(product, forceRedirectionAfterAddingToCart);
                }

                //picture
                if (preparePictureModel)
                {
                    model.DefaultPictureModel = await PrepareProductOverviewPictureModelAsync(product, productThumbPictureSize);
                }

                //specs
                if (prepareSpecificationAttributes)
                {
                    model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);
                }

                //reviews
                model.ReviewOverviewModel = await PrepareProductReviewOverviewModelAsync(product);

                #region checkServiceProduct

                if (IsServiceProduct(product.Id))
                {
                    model.CustomProperties.Add("ServiceProductInfo",
                        new AdditionalServiceProductInformationModel()
                        {
                            IsServiceProduct = true,
                            SubmitButtonText = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Request.Sumbmit.Text"),
                            Errors = new List<string>()
                            {
                                error
                            }
                        });
                }

                #endregion

                models.Add(model);
            }

            return models;
        }


        protected override async Task<PictureModel> PrepareProductOverviewPictureModelAsync(Product product, int? productThumbPictureSize = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //prepare picture model
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductDefaultPictureModelKey,
                product, pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
                await _storeContext.GetCurrentStoreAsync());

            var defaultPictureModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                string fullSizeImageUrl, imageUrl;
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                if (picture == null)
                {
                    var erpPicture = (await _erpPictureService.GetErpPicturesByNopProductIdAsync(product.Id)).FirstOrDefault();
                    if (erpPicture != null)
                    {
                        var erpImageUrl = ErpSyncHelper.PrepareImageUrlWithSize(erpPicture.ImageLink, pictureSize, _erpSyncSettings);
                        return new PictureModel
                        {
                            ImageUrl = erpImageUrl,
                            FullSizeImageUrl = erpImageUrl,
                            Title = product.Name,
                            AlternateText = product.Name
                        };
                    }
                }
                var pictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    //"title" attribute
                    Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                        ? picture.TitleAttribute
                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
                            productName),
                    //"alt" attribute
                    AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                        ? picture.AltAttribute
                        : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
                            productName)
                };

                return pictureModel;
            });

            return defaultPictureModel;
        }

        protected override async Task<(PictureModel pictureModel, IList<PictureModel> allPictureModels)> PrepareProductDetailsPictureModelAsync(Product product, bool isAssociatedProduct)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //default picture size
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;

            //prepare picture models
            var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductDetailsPicturesModelKey
                , product, defaultPictureSize, isAssociatedProduct,
                await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            var cachedPictures = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

                var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
                var defaultPicture = pictures.FirstOrDefault();

                string fullSizeImageUrl, imageUrl, thumbImageUrl;
                (imageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, defaultPictureSize, !isAssociatedProduct);
                (fullSizeImageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, 0, !isAssociatedProduct);

                var defaultPictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl
                };
                //"title" attribute
                defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                    defaultPicture.TitleAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                //"alt" attribute
                defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                    defaultPicture.AltAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                //all pictures
                var pictureModels = new List<PictureModel>();
                for (var i = 0; i < pictures.Count(); i++)
                {
                    var picture = pictures[i];

                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, !isAssociatedProduct);
                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    var pictureModel = new PictureModel
                    {
                        ImageUrl = imageUrl,
                        ThumbImageUrl = thumbImageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                        picture.AltAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                    pictureModels.Add(pictureModel);
                }
                var erpPictures = await _erpPictureService.GetErpPicturesByNopProductIdAsync(product.Id);
                if (defaultPicture == null && erpPictures.Count > 0)
                {
                    var erpDefaultPicture = erpPictures.First();
                    var erpThumbImageUrl = ErpSyncHelper.PrepareImageUrlWithSize(erpDefaultPicture.ImageLink, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage, _erpSyncSettings);
                    var erpFullSizeImageUrl = ErpSyncHelper.PrepareImageUrlWithSize(erpDefaultPicture.ImageLink, defaultPictureSize, _erpSyncSettings);
                    defaultPictureModel = new PictureModel
                    {
                        ImageUrl = erpDefaultPicture.ImageLink,
                        FullSizeImageUrl = erpFullSizeImageUrl,
                        ThumbImageUrl = erpThumbImageUrl,
                        Title = product.Name,
                        AlternateText = product.Name
                    };
                    foreach (var erpPicture in erpPictures)
                    {
                        erpThumbImageUrl = ErpSyncHelper.PrepareImageUrlWithSize(erpPicture.ImageLink, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage, _erpSyncSettings);
                        erpFullSizeImageUrl = ErpSyncHelper.PrepareImageUrlWithSize(erpPicture.ImageLink, defaultPictureSize, _erpSyncSettings);

                        pictureModels.Add(new PictureModel
                        {
                            ImageUrl = erpPicture.ImageLink,
                            FullSizeImageUrl = erpFullSizeImageUrl,
                            ThumbImageUrl = erpThumbImageUrl,
                            Title = product.Name,
                            AlternateText = product.Name
                        });
                    }
                }
                return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
            });

            var allPictureModels = cachedPictures.PictureModels;
            return (cachedPictures.DefaultPictureModel, allPictureModels);
        }

        async Task PrepareSettingsForVoucher(GiftVoucherSettings giftVoucherSettings, Product product, GiftCardAdditionalDataModel additionalDataModel)
        {
            additionalDataModel.IsEnabledFirstName = true;
            additionalDataModel.IsRequiredFirstName = true;
            additionalDataModel.IsEnabledLastName = true;
            additionalDataModel.IsRequiredLastName = true;
            additionalDataModel.IsEnabledRecipientEmail = true;
            additionalDataModel.IsEnabledGiftCardDeliveryDate = true;
            additionalDataModel.IsRequiredGiftCardDeliveryDate = true;

            additionalDataModel.IsEnabledSenderName = true;
            additionalDataModel.IsRequiredSenderName = true;

            additionalDataModel.IsEnabledSenderEmail = true;
            additionalDataModel.IsRequiredSenderEmail = true;

            additionalDataModel.IsEnabledMessage = true;
            additionalDataModel.IsRequiredMessage = true;


            if (product.Sku.Equals(_giftVoucherSettings.ZimazonGiftProductSku))
            {
                additionalDataModel.IsRequiredRecipientEmail = giftVoucherSettings.RequireRecipientEmailZimazon;
                additionalDataModel.IsEnabledCellPhoneNumber = giftVoucherSettings.EnableCellPhoneNumberZimazon;
                additionalDataModel.IsRequiredCellPhoneNumber = giftVoucherSettings.RequireCellPhoneNumberZimazon;
                additionalDataModel.IsEnabledIdOrPassportNumber = giftVoucherSettings.EnableIdOrPassportNumberZimazon;
                additionalDataModel.IsRequiredIdOrPassportNumber = giftVoucherSettings.RequireIdOrPassportNumberZimazon;
                additionalDataModel.IsEnabledPhysicalAddress = giftVoucherSettings.EnablePhysicalAddressZimazon;
                additionalDataModel.IsRequiredPhysicalAddress = giftVoucherSettings.RequirePhysicalAddressZimazon;
                additionalDataModel.ButtonsValue = await PrepareAvailableAmountsAsync(_giftVoucherSettings.ZimazonGiftCardAvaiableAmounts);
                additionalDataModel.DeliveryDateMessage = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Zimazon.DeliveryDateMessage");
                additionalDataModel.ValidUptoInDays = 365;
            }
            if (product.Sku.Equals(_giftVoucherSettings.ElectrosalesGiftProductSku))
            {
                additionalDataModel.IsRequiredRecipientEmail = giftVoucherSettings.RequireRecipientEmailElectrosales;
                additionalDataModel.IsEnabledCellPhoneNumber = giftVoucherSettings.EnableCellPhoneNumberElectrosales;
                additionalDataModel.IsRequiredCellPhoneNumber = giftVoucherSettings.RequireCellPhoneNumberElectrosales;
                additionalDataModel.IsEnabledIdOrPassportNumber = giftVoucherSettings.EnableIdOrPassportNumberElectrosales;
                additionalDataModel.IsRequiredIdOrPassportNumber = giftVoucherSettings.RequireIdOrPassportNumberElectrosales;
                additionalDataModel.IsEnabledPhysicalAddress = giftVoucherSettings.EnablePhysicalAddressElectrosales;
                additionalDataModel.IsRequiredPhysicalAddress = giftVoucherSettings.RequirePhysicalAddressElectrosales;
                additionalDataModel.ButtonsValue = await PrepareAvailableAmountsAsync(_giftVoucherSettings.ElectrosalesGiftVoucherAvaiableAmounts);
                additionalDataModel.DeliveryDateMessage = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Electrosales.DeliveryDateMessage");
                additionalDataModel.ValidUptoInDays = 90;
            }
        }

        protected async Task<Dictionary<string, int>> PrepareAvailableAmountsAsync(string amounts)
        {
            var avaiableAmounts = new List<int>();
            var availablePrices = new Dictionary<string, int>();
            if (!string.IsNullOrEmpty(amounts))
            {
                if (amounts.Contains(","))
                {
                    var ids = amounts.Split(",").Select(int.Parse).ToList();
                    avaiableAmounts.AddRange(ids);
                }
                else
                {
                    avaiableAmounts.Add(int.Parse(amounts));
                }
            }
            var currency = await _workContext.GetWorkingCurrencyAsync();
            avaiableAmounts.ForEach(x =>
            {
                var str = x.ToString("C", new CultureInfo(currency.DisplayLocale));
                availablePrices.Add(str, x);
            });
            return availablePrices;
        }

        protected bool IsServiceProduct(int productId)
        {
            return _zimzoneServiceEntityService.GetAllServiceProductIdAsync().Result.Contains(productId);
        }
    }
}

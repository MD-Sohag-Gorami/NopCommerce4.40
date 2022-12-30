using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Html;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Pickup.PickupInStore.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public class OverridenPdfService : PdfService
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShipmentService _shipmentService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly PdfSettings _pdfSettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly GiftVoucherSettings _giftVoucherSettings;

        private readonly AddressSettings _addressSettings;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStorePickupPointService _storePickupPointService;
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;
        private readonly IServiceRequestService _serviceRequestService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly IDateTimeHelper _dateTimeHelper;


        public OverridenPdfService(AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAddressService addressService,
            ICountryService countryService,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            IGiftCardService giftCardService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            INopFileProvider fileProvider,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IRewardPointService rewardPointService,
            ISettingService settingService,
            IShipmentService shipmentService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            IVendorService vendorService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            PdfSettings pdfSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            GiftVoucherSettings giftVoucherSettings,
            IStorePickupPointService storePickupPointService,
            IZimzoneServiceEntityService zimzoneServiceEntityService,
            IServiceRequestService serviceRequestService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService) : base(addressSettings,
                catalogSettings,
                currencySettings,
                addressAttributeFormatter,
                addressService,
                countryService,
                currencyService,
                dateTimeHelper,
                giftCardService,
                languageService,
                localizationService,
                measureService,
                fileProvider,
                orderService,
                paymentPluginManager,
                paymentService,
                pictureService,
                priceFormatter,
                productService,
                rewardPointService,
                settingService,
                shipmentService,
                stateProvinceService,
                storeContext,
                storeService,
                vendorService,
                workContext,
                measureSettings,
                pdfSettings,
                taxSettings,
                vendorSettings)
        {
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _currencyService = currencyService;
            _giftCardService = giftCardService;
            _languageService = languageService;
            _localizationService = localizationService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _rewardPointService = rewardPointService;
            _shipmentService = shipmentService;
            _vendorService = vendorService;
            _workContext = workContext;
            _pdfSettings = pdfSettings;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _giftVoucherSettings = giftVoucherSettings;
            _addressSettings = addressSettings;
            _addressAttributeFormatter = addressAttributeFormatter;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _storePickupPointService = storePickupPointService;
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
            _serviceRequestService = serviceRequestService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _storeService = storeService;
            _pictureService = pictureService;
            _dateTimeHelper = dateTimeHelper;

        }
        protected override async Task PrintShippingInfoAsync(Language lang, Order order, Font titleFont, Font font, PdfPTable addressTable)
        {
            var shippingAddressPdf = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang)
            };
            shippingAddressPdf.DefaultCell.Border = Rectangle.NO_BORDER;

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                //cell = new PdfPCell();
                //cell.Border = Rectangle.NO_BORDER;
                const string indent = "   ";

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddressId == null || await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is not Address shippingAddress)
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.ShippingInformation", lang, titleFont));
                    if (!string.IsNullOrEmpty(shippingAddress.Company))
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Company", indent, lang, font, shippingAddress.Company));
                    shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Name", indent, lang, font, shippingAddress.FirstName + " " + shippingAddress.LastName));
                    if (_addressSettings.PhoneEnabled)
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Phone", indent, lang, font, shippingAddress.PhoneNumber));
                    if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(shippingAddress.FaxNumber))
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Fax", indent, lang, font, shippingAddress.FaxNumber));
                    if (_addressSettings.StreetAddressEnabled)
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Address", indent, lang, font, shippingAddress.Address1));
                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Address2", indent, lang, font, shippingAddress.Address2));
                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{indent}{shippingAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(shippingAddress.County) ? $"{shippingAddress.County}, " : string.Empty)}" +
                            $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                            $"{shippingAddress.ZipPostalCode}";
                        shippingAddressPdf.AddCell(new Paragraph(addressLine, font));
                    }

                    if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(shippingAddress) is Country country)
                    {
                        shippingAddressPdf.AddCell(
                            new Paragraph(indent + await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id), font));
                    }
                    //custom attributes
                    var customShippingAddressAttributes = await _addressAttributeFormatter
                        .FormatAttributesAsync(shippingAddress.CustomAttributes, $"<br />{indent}");
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        var text = HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true);
                        shippingAddressPdf.AddCell(new Paragraph(indent + text, font));
                    }

                    shippingAddressPdf.AddCell(new Paragraph(" "));
                }
                else if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Pickup", lang, titleFont));
                    var openingHour = string.Empty;
                    var contactInfo = string.Empty;
                    /*var pickupPoint = await _storePickupPointService.GetStorePickupPointByAddressIdAsync(order.PickupAddressId.Value);

                    if (pickupPoint != null)
                    {
                        openingHour = pickupPoint.OpeningHours;
                        contactInfo = pickupPoint.ContactInfo;
                    }*/

                    if (!string.IsNullOrEmpty(pickupAddress.Address1))
                        shippingAddressPdf.AddCell(new Paragraph(
                            $"{indent}{string.Format(await _localizationService.GetResourceAsync("PDFInvoice.Address", lang.Id), pickupAddress.Address1)}",
                            font));

                    if (!string.IsNullOrEmpty(pickupAddress.City))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.City}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.County))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.County}", font));

                    if (await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country)
                        shippingAddressPdf.AddCell(
                            new Paragraph($"{indent}{await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id)}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.ZipPostalCode))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.ZipPostalCode}", font));
                    //override for custom pickup point details
                    if (!string.IsNullOrEmpty(openingHour))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{openingHour}", font));
                    if (!string.IsNullOrEmpty(contactInfo))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{contactInfo}", font));

                    shippingAddressPdf.AddCell(new Paragraph(" "));
                }

                shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.ShippingMethod", indent, lang, font, order.ShippingMethod));
                shippingAddressPdf.AddCell(new Paragraph());

                addressTable.AddCell(shippingAddressPdf);
            }
            else
            {
                shippingAddressPdf.AddCell(new Paragraph());
                addressTable.AddCell(shippingAddressPdf);
            }
        }
        protected override async Task PrintProductsAsync(int vendorId, Language lang, Font titleFont, Document doc, Order order, Font font, Font attributesFont)
        {
            var productsHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            var cellProducts = await GetPdfCellAsync("PDFInvoice.Product(s)", lang, titleFont);
            cellProducts.Border = Rectangle.NO_BORDER;
            productsHeader.AddCell(cellProducts);
            doc.Add(productsHeader);
            doc.Add(new Paragraph(" "));

            //a vendor should have access only to products
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendorId);

            var count = 4 + (_catalogSettings.ShowSkuOnProductDetailsPage ? 1 : 0)
                        + (_vendorSettings.ShowVendorOnOrderDetailsPage ? 1 : 0);

            var productsTable = new PdfPTable(count)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { 4, new[] { 50, 20, 10, 20 } },
                { 5, new[] { 45, 15, 15, 10, 15 } },
                { 6, new[] { 40, 13, 13, 12, 10, 12 } }
            };

            productsTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);

            //product name
            var cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductName", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //SKU
            if (_catalogSettings.ShowSkuOnProductDetailsPage)
            {
                cellProductItem = await GetPdfCellAsync("PDFInvoice.SKU", lang, font);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            //Vendor name
            if (_vendorSettings.ShowVendorOnOrderDetailsPage)
            {
                cellProductItem = await GetPdfCellAsync("PDFInvoice.VendorName", lang, font);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            //price
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductPrice", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //qty
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductQuantity", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //total
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductTotal", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? await _vendorService.GetVendorsByProductIdsAsync(orderItems.Select(item => item.ProductId).ToArray()) : new List<Vendor>();

            var servicePaymentProductIds = await _zimzoneServiceEntityService.GetAllPaymentProductIdAsync();

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                var pAttribTable = new PdfPTable(1) { RunDirection = GetDirection(lang) };
                pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;
                var name = await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id);

                var electrosalesProductPriceInPrimaryCurrency = string.Empty;
                var primaryCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
                if (/*primaryCurrency.CurrencyCode != order.CustomerCurrencyCode &&*/ product.Sku == _giftVoucherSettings.ElectrosalesGiftProductSku)
                {
                    electrosalesProductPriceInPrimaryCurrency = (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax) ? await _priceFormatter.FormatPriceAsync(orderItem.UnitPriceExclTax, true, primaryCurrency.CurrencyCode, lang.Id, true) : await _priceFormatter.FormatPriceAsync(orderItem.PriceExclTax, true, primaryCurrency.CurrencyCode, lang.Id, true);
                    name = name + $"({electrosalesProductPriceInPrimaryCurrency})";
                }
                //product name

                // replace service product custom name with name
                if (servicePaymentProductIds.Contains(orderItem.ProductId))
                {
                    if (int.TryParse(orderItem.AttributesXml, out var requestId))
                    {
                        var request = await _serviceRequestService.GetRequestByIdAsync(requestId);
                        if (request != null && !string.IsNullOrEmpty(request.CustomName))
                        {
                            name = request.CustomName;
                        }
                    }
                }

                pAttribTable.AddCell(new Paragraph(name, font));
                cellProductItem.AddElement(new Paragraph(name, font));
                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var attributesParagraph =
                        new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true),
                            attributesFont);
                    pAttribTable.AddCell(attributesParagraph);
                }

                //rental info
                if (product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value)
                        : string.Empty;
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value)
                        : string.Empty;
                    var rentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);

                    var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                    pAttribTable.AddCell(rentalInfoParagraph);
                }

                productsTable.AddCell(pAttribTable);

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                    cellProductItem = GetPdfCell(sku ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                //Vendor name
                if (_vendorSettings.ShowVendorOnOrderDetailsPage)
                {
                    var vendorName = vendors.FirstOrDefault(v => v.Id == product.VendorId)?.Name ?? string.Empty;
                    cellProductItem = GetPdfCell(vendorName, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }


                //price
                string unitPrice;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang.Id, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang.Id, false);
                }

                cellProductItem = GetPdfCell(unitPrice, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //qty
                cellProductItem = GetPdfCell(orderItem.Quantity, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //total
                string subTotal;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang.Id, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang.Id, false);
                }

                cellProductItem = GetPdfCell(subTotal, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);
            }

            doc.Add(productsTable);
        }

        protected override async Task PrintTotalsAsync(int vendorId, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //vendors cannot see totals
            if (vendorId != 0)
                return;

            //subtotal
            var totalsTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var languageId = lang.Id;

            //order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                var orderSubtotalInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                var orderSubtotalInclTaxStr = await _priceFormatter.FormatPriceAsync(orderSubtotalInclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, languageId, true);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Sub-Total", languageId)} {orderSubtotalInclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
            else
            {
                //excluding tax

                var orderSubtotalExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                var orderSubtotalExclTaxStr = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, languageId, false);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Sub-Total", languageId)} {orderSubtotalExclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //discount (applied to order subtotal)
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
            {
                //order subtotal
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                    !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                {
                    //including tax

                    var orderSubTotalDiscountInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(
                        -orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax

                    var orderSubTotalDiscountExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(
                        -orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //shipping
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var orderShippingInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                    var orderShippingInclTaxStr = await _priceFormatter.FormatShippingPriceAsync(
                        orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                    var p = GetPdfCell($"{ await _localizationService.GetResourceAsync("PDFInvoice.Shipping", languageId)} {orderShippingInclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var orderShippingExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                    var orderShippingExclTaxStr = await _priceFormatter.FormatShippingPriceAsync(
                        orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Shipping", languageId)} {orderShippingExclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //payment fee
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var paymentMethodAdditionalFeeInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeInclTaxStr = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                        paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.PaymentMethodAdditionalFee", languageId)} {paymentMethodAdditionalFeeInclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var paymentMethodAdditionalFeeExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeExclTaxStr = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                        paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.PaymentMethodAdditionalFee", languageId)} {paymentMethodAdditionalFeeExclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //tax
            var taxStr = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            bool displayTax;
            var displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = _orderService.ParseTaxRates(order, order.TaxRates);

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    taxStr = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        false, languageId);
                }
            }

            if (displayTax)
            {
                //override for showing VAT insteated of tax
                //var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Tax", languageId)} {taxStr}", font);

                var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
                var orderItemProductsSkus = (await _productService.GetProductsByIdsAsync(orderItems.Select(x => x.ProductId).ToArray()))
                                            .Select(x => x.Sku).ToList();
                var hasElectrosalesVoucher = orderItemProductsSkus.Contains(_giftVoucherSettings.ElectrosalesGiftProductSku);
                var hasOnlyElectrosalesVoucher = hasElectrosalesVoucher && orderItemProductsSkus.Count == 1;

                if (hasOnlyElectrosalesVoucher)
                {
                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Vat", languageId)}: {await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Tax.NoVatOnElectrosales", languageId)}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Vat", languageId)}: {await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Tax.TaxMessage", languageId)}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);

                    if (hasElectrosalesVoucher)
                    {
                        p = GetPdfCell($" {await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Tax.NoVatOnElectrosales", languageId)}", font);
                        p.HorizontalAlignment = Element.ALIGN_RIGHT;
                        p.Border = Rectangle.NO_BORDER;
                        totalsTable.AddCell(p);
                    }
                }


            }

            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    var taxRate = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.TaxRate", languageId),
                        _priceFormatter.FormatTaxRate(item.Key));
                    var taxValue = await _priceFormatter.FormatPriceAsync(
                        _currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode,
                        false, languageId);

                    var p = GetPdfCell($"{taxRate} {taxValue}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //discount (applied to order total)
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                var orderDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(-orderDiscountInCustomerCurrency,
                    true, order.CustomerCurrencyCode, false, languageId);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderDiscountInCustomerCurrencyStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //gift cards
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                var gcTitle = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.GiftCardInfo", languageId),
                    (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId))?.GiftCardCouponCode);
                var gcAmountStr = await _priceFormatter.FormatPriceAsync(
                    -_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true,
                    order.CustomerCurrencyCode, false, languageId);

                var p = GetPdfCell($"{gcTitle} {gcAmountStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //reward points
            if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                var rpTitle = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.RewardPoints", languageId),
                    -redeemedRewardPointsEntry.Points);
                var rpAmount = await _priceFormatter.FormatPriceAsync(
                    -_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, false, languageId);

                var p = GetPdfCell($"{rpTitle} {rpAmount}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //order total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var orderTotalStr = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);

            var pTotal = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.OrderTotal", languageId)} {orderTotalStr}", titleFont);
            pTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
            pTotal.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(pTotal);

            doc.Add(totalsTable);
        }

        protected override async Task PrintHeaderAsync(PdfSettings pdfSettingsByStore, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //logo
            var logoPicture = await _pictureService.GetPictureByIdAsync(pdfSettingsByStore.LogoPictureId);
            var logoExists = logoPicture != null;

            //header
            var headerTable = new PdfPTable(logoExists ? 2 : 1)
            {
                RunDirection = GetDirection(lang)
            };
            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;

            //store info
            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            var anchor = new Anchor(store.Url.Trim('/'), font)
            {
                Reference = store.Url
            };

            var cellHeader = GetPdfCell(string.Format(await _localizationService.GetResourceAsync("PDFInvoice.Order#", lang.Id), order.CustomOrderNumber), titleFont);
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(anchor));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(await GetParagraphAsync("PDFInvoice.OrderDate", lang, font, (await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc)).ToString("D", new CultureInfo(lang.LanguageCulture))));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.HorizontalAlignment = Element.ALIGN_LEFT;
            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);

            if (logoExists)
                headerTable.SetWidths(lang.Rtl ? new[] { 0.2f, 0.8f } : new[] { 0.8f, 0.2f });
            headerTable.WidthPercentage = 100f;

            //logo               
            if (logoExists)
            {
                var logoFilePath = await _pictureService.GetThumbLocalPathAsync(logoPicture, 0, false);
                var logo = Image.GetInstance(logoFilePath);
                logo.Alignment = GetAlignment(lang, true);
                //logo.ScaleToFit(65f, 65f);
                logo.ScaleToFit(100f, 100f);

                var cellLogo = new PdfPCell { Border = Rectangle.NO_BORDER };
                cellLogo.AddElement(logo);
                headerTable.AddCell(cellLogo);
            }

            doc.Add(headerTable);


            var customerInfoTable = new PdfPTable(2)
            {
                RunDirection = GetDirection(lang),
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            customerInfoTable.DefaultCell.Border = Rectangle.NO_BORDER;
            customerInfoTable.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;

            var customerCellTitle = GetPdfCell("Customer Info: ", titleFont);
            customerCellTitle.Phrase.Add(new Phrase(Environment.NewLine));
            customerCellTitle.HorizontalAlignment = Element.ALIGN_LEFT;
            customerCellTitle.Border = Rectangle.NO_BORDER;
            customerInfoTable.AddCell(customerCellTitle);
            customerInfoTable.AddCell(string.Empty);
            //adding customer info
            //cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            var customer = await _customerService.GetCustomerByIdAsync(order?.CustomerId ?? 0);
            var customerName = "Not Registered";


            //cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            var customerCellInfo = GetPdfCell("", font);
            customerCellInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            customerCellInfo.Border = Rectangle.NO_BORDER;
            if (customer != null && await _customerService.IsRegisteredAsync(customer))
            {
                customerName = await _customerService.GetCustomerFullNameAsync(customer);
                var phoneNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute);
                customerCellInfo.Phrase.Add(new Phrase(string.Format("Name: {0}", customerName)));
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    customerCellInfo.Phrase.Add(new Phrase(Environment.NewLine));
                    customerCellInfo.Phrase.Add(new Phrase(string.Format("Email: {0}", customer.Email)));
                }
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    customerCellInfo.Phrase.Add(new Phrase(Environment.NewLine));
                    customerCellInfo.Phrase.Add(new Phrase(string.Format("Phone Number: {0}", phoneNumber)));
                }
            }
            else
            {
                customerCellInfo.Phrase.Add(new Phrase(string.Format("Name: {0}", customerName)));
            }
            customerCellInfo.Phrase.Add(new Phrase(Environment.NewLine));
            customerCellInfo.Phrase.Add(new Phrase(Environment.NewLine));


            customerInfoTable.AddCell(customerCellInfo);
            customerInfoTable.AddCell(string.Empty);

            doc.Add(customerInfoTable);
        }

    }
}

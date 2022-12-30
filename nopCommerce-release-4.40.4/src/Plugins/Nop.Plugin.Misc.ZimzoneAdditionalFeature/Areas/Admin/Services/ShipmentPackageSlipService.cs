using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
//using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Shipping;
using Nop.Core.Html;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using QRCoder;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services
{
    public class ShipmentPackageSlipService : IShipmentPackageSlipService
    {
        private readonly PdfSettings _pdfSettings;
        private readonly INopFileProvider _fileProvider;
        private readonly IOrderService _orderService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IAddressService _addressService;
        private readonly ILocalizationService _localizationService;
        private readonly AddressSettings _addressSettings;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IShipmentService _shipmentService;
        private readonly IProductService _productService;

        public ShipmentPackageSlipService(PdfSettings pdfSettings,
            INopFileProvider fileProvider,
            IOrderService orderService,
            ILanguageService languageService,
            IWorkContext workContext,
            IAddressService addressService,
            ILocalizationService localizationService,
            AddressSettings addressSettings,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IAddressAttributeFormatter addressAttributeFormatter,
            IShipmentService shipmentService,
            IProductService productService)
        {
            _pdfSettings = pdfSettings;
            _fileProvider = fileProvider;
            _orderService = orderService;
            _languageService = languageService;
            _workContext = workContext;
            _addressService = addressService;
            _localizationService = localizationService;
            _addressSettings = addressSettings;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _addressAttributeFormatter = addressAttributeFormatter;
            _shipmentService = shipmentService;
            _productService = productService;
        }

        private iTextSharp.text.Font GetFont()
        {
            //nopCommerce supports Unicode characters
            //nopCommerce uses Free Serif font by default (~/App_Data/Pdf/FreeSerif.ttf file)
            //It was downloaded from http://savannah.gnu.org/projects/freefont
            return GetFont(_pdfSettings.FontFileName);
        }

        private iTextSharp.text.Font GetFont(string fontFileName)
        {
            if (fontFileName == null)
                throw new ArgumentNullException(nameof(fontFileName));

            var fontPath = _fileProvider.Combine(_fileProvider.MapPath("~/App_Data/Pdf/"), fontFileName);
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.NORMAL);
            return font;
        }

        private async Task<Paragraph> GetParagraphAsync(string resourceKey, Language lang, iTextSharp.text.Font font, params object[] args)
        {
            return await GetParagraphAsync(resourceKey, string.Empty, lang, font, args);
        }

        private async Task<Paragraph> GetParagraphAsync(string resourceKey, string indent, Language lang, iTextSharp.text.Font font, params object[] args)
        {
            var formatText = await _localizationService.GetResourceAsync(resourceKey, lang.Id);
            return new Paragraph(indent + (args.Any() ? string.Format(formatText, args) : formatText), font);
        }

        private async Task<PdfPCell> GetPdfCellAsync(string resourceKey, Language lang, iTextSharp.text.Font font)
        {
            return new PdfPCell(new Phrase(await _localizationService.GetResourceAsync(resourceKey, lang.Id), font));
        }

        private PdfPCell GetPdfCell(object text, iTextSharp.text.Font font)
        {
            return new PdfPCell(new Phrase(text.ToString(), font));
        }

        private async Task PackagingSlipsToPdfAsync(Stream stream, IList<Shipment> shipments, int languageId, iTextSharp.text.Rectangle pageSize, int fontSize)
        {
            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(iTextSharp.text.Font.BOLD);
            titleFont.Color = BaseColor.Black;
            titleFont.Size = fontSize;
            var font = GetFont();
            font.Size = fontSize;

            var shipmentCount = shipments.Count;
            var shipmentNum = 0;

            foreach (var shipment in shipments)
            {
                var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

                var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? order.CustomerLanguageId : languageId);
                if (lang == null || !lang.Published)
                    lang = await _workContext.GetWorkingLanguageAsync();

                var addressTable = new PdfPTable(1);
                if (lang.Rtl)
                    addressTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                addressTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                addressTable.WidthPercentage = 100f;
                addressTable.SkipLastFooter = true;


                addressTable.AddCell(new Paragraph(" "));
                addressTable.AddCell(new Paragraph(" "));

                if (pageSize == ShipmentPackageSlipDefaults.A5Portrait || pageSize == ShipmentPackageSlipDefaults.A5Landscape)
                {
                    addressTable.AddCell(new Paragraph(" "));
                }

                addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Shipment", lang, titleFont, shipment.Id));
                addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Order", lang, titleFont, order.CustomOrderNumber));

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddressId == null || await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is not Address shippingAddress)
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(shippingAddress.Company))
                        addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Company", lang, font, shippingAddress.Company));

                    addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Name", lang, font, shippingAddress.FirstName + " " + shippingAddress.LastName));
                    if (_addressSettings.PhoneEnabled)
                        addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Phone", lang, font, shippingAddress.PhoneNumber));

                    var addressList = new List<string>();

                    if (_addressSettings.StreetAddressEnabled)
                    {
                        addressList.Add(shippingAddress.Address1);
                        //addressList.Add((await GetParagraphAsync("PDFPackagingSlip.Address", lang, font, shippingAddress.Address1)).ToString());
                    }

                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                    {
                        addressList.Add(shippingAddress.Address2);
                        //addressList.Add((await GetParagraphAsync("PDFPackagingSlip.Address", lang, font, shippingAddress.Address2)).ToString());
                    }

                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{shippingAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(shippingAddress.County) ? $"{shippingAddress.County}, " : string.Empty)}" +
                            $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                            $"{shippingAddress.ZipPostalCode}";
                        //var line = new Paragraph(addressLine, font);
                        addressList.Add(addressLine);
                    }

                    if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(shippingAddress) is Country country)
                    {
                        //var line = (new Paragraph(await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id), font));
                        //addressList.Add(line.ToString());
                        addressList.Add(await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id));
                    }


                    var address = string.Join(", ", addressList);

                    var addressCell = await GetParagraphAsync("PDFPackagingSlip.Address", lang, font, address);

                    addressTable.AddCell(addressCell);

                    //custom attributes
                    var customShippingAddressAttributes = await _addressAttributeFormatter.FormatAttributesAsync(shippingAddress.CustomAttributes);
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        addressTable.AddCell(new Paragraph(HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true), font));
                    }
                }
                else
                    if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    addressTable.AddCell(new Paragraph(await _localizationService.GetResourceAsync("PDFInvoice.Pickup", lang.Id), titleFont));

                    var addressList = new List<string>();

                    if (!string.IsNullOrEmpty(pickupAddress.Address1))
                        addressList.Add(pickupAddress.Address1);

                    if (!string.IsNullOrEmpty(pickupAddress.City))
                        addressList.Add(pickupAddress.City);

                    if (!string.IsNullOrEmpty(pickupAddress.County))
                        addressList.Add(pickupAddress.County);

                    if (await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country)
                        addressList.Add(await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id));

                    if (!string.IsNullOrEmpty(pickupAddress.ZipPostalCode))
                        addressList.Add(pickupAddress.ZipPostalCode);

                    var address = string.Join(", ", addressList);

                    var addressCell = new Paragraph($"{string.Format(await _localizationService.GetResourceAsync("PDFInvoice.Address", lang.Id), address)}", font);

                    addressTable.AddCell(addressCell);
                }

                // QR Code
                var qrText = await PrepareQRCodeData(shipment, languageId);
                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);

                var cellLogo = new PdfPCell { Border = iTextSharp.text.Rectangle.NO_BORDER };

                var byteBitmapImage = BitmapToBytes(qrCodeImage);
                var image = iTextSharp.text.Image.GetInstance(byteBitmapImage);
                image.ScaleToFit(60f, 60f);

                cellLogo.AddElement(image);

                addressTable.AddCell(cellLogo);

                doc.Add(addressTable);

                shipmentNum++;
                if (shipmentNum < shipmentCount)
                    doc.NewPage();
            }

            doc.Close();
        }

        private async Task<string> GetLineAsync(string resourceKey, int languageId, params object[] args)
        {
            var formatText = await _localizationService.GetResourceAsync(resourceKey, languageId);
            string text = args.Any() ? string.Format(formatText, args) : formatText;
            return text;
        }

        private async Task<string> PrepareQRCodeData(Shipment shipment, int languageId)
        {
            var sb = new StringBuilder();

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? order.CustomerLanguageId : languageId);
            if (lang == null || !lang.Published)
                lang = await _workContext.GetWorkingLanguageAsync();

            sb.Append(await GetLineAsync("PDFPackagingSlip.Shipment", lang.Id, shipment.Id));
            sb.Append('\n');

            sb.Append(await GetLineAsync("PDFPackagingSlip.Order", lang.Id, order.Id));
            sb.Append('\n');

            if (!order.PickupInStore)
            {
                if (order.ShippingAddressId == null || await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is not Address shippingAddress)
                    throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(shippingAddress.Company))
                {
                    sb.Append(await GetLineAsync("PDFPackagingSlip.Company", lang.Id, shippingAddress.Company));
                    sb.Append('\n');
                }

                sb.Append(await GetLineAsync("PDFPackagingSlip.Name", lang.Id, shippingAddress.FirstName + " " + shippingAddress.LastName));
                sb.Append('\n');

                if (_addressSettings.PhoneEnabled)
                {
                    sb.Append(await GetLineAsync("PDFPackagingSlip.Phone", lang.Id, shippingAddress.PhoneNumber));
                    sb.Append('\n');
                }

                var address = "";

                if (_addressSettings.StreetAddressEnabled && _addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                {
                    address = await GetLineAsync("PDFPackagingSlip.Address", lang.Id, shippingAddress.Address1 + " " + shippingAddress.Address2);
                }
                else if (_addressSettings.StreetAddressEnabled)
                {
                    address = await GetLineAsync("PDFPackagingSlip.Address", lang.Id, shippingAddress.Address1);
                }
                sb.Append(address);
                sb.Append('\n');

                if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                    _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                {
                    var addressLine = $"{shippingAddress.City}, " +
                        $"{(!string.IsNullOrEmpty(shippingAddress.County) ? $"{shippingAddress.County}, " : string.Empty)}" +
                        $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                        $"{shippingAddress.ZipPostalCode}";
                    sb.Append(addressLine);
                    sb.Append('\n');
                }

                if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(shippingAddress) is Country country)
                    sb.Append(await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id));

                //custom attributes
                var customShippingAddressAttributes = await _addressAttributeFormatter.FormatAttributesAsync(shippingAddress.CustomAttributes);
                if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                {
                    sb.Append(HtmlHelper.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true));
                }
            }
            else
                if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
            {
                sb.Append(await _localizationService.GetResourceAsync("PDFInvoice.Pickup", lang.Id));

                if (!string.IsNullOrEmpty(pickupAddress.Address1))
                    sb.Append($"   {string.Format(await _localizationService.GetResourceAsync("PDFInvoice.Address", lang.Id), pickupAddress.Address1)}");

                if (!string.IsNullOrEmpty(pickupAddress.City))
                    sb.Append($"   {pickupAddress.City}");

                if (!string.IsNullOrEmpty(pickupAddress.County))
                    sb.Append($"   {pickupAddress.County}");

                if (await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country)
                    sb.Append($"   {await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id)}");

                if (!string.IsNullOrEmpty(pickupAddress.ZipPostalCode))
                    sb.Append($"   {pickupAddress.ZipPostalCode}");
            }

            return sb.ToString();
        }

        private byte[] BitmapToBytes(Bitmap img)
        {
            using var stream = new MemoryStream();
            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }

        public async Task PrintPackagingSlipsToPdfAsync(Stream stream, IList<Shipment> shipments, iTextSharp.text.Rectangle pageSize, int fontSize, int languageId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (shipments == null)
                throw new ArgumentNullException(nameof(shipments));

            await PackagingSlipsToPdfAsync(stream, shipments, languageId, pageSize, fontSize);
        }
    }
}

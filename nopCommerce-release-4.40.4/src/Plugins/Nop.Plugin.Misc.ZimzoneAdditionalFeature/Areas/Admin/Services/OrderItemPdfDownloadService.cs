using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Html;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services
{
    public class OrderItemPdfDownloadService : IOrderItemPdfDownloadService
    {
        private readonly PdfSettings _pdfSettings;
        private readonly INopFileProvider _fileProvider;
        private readonly IProductService _productService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorService _vendorService;
        private readonly IShipmentService _shipmentService;

        public OrderItemPdfDownloadService(PdfSettings pdfSettings,
                                           INopFileProvider fileProvider,
                                           IProductService productService,
                                           ILanguageService languageService,
                                           IWorkContext workContext,
                                           IOrderService orderService,
                                           ILocalizationService localizationService,
                                           IVendorService vendorService,
                                           IShipmentService shipmentService)
        {
            _pdfSettings = pdfSettings;
            _fileProvider = fileProvider;
            _productService = productService;
            _languageService = languageService;
            _workContext = workContext;
            _orderService = orderService;
            _localizationService = localizationService;
            _vendorService = vendorService;
            _shipmentService = shipmentService;
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

        protected virtual async Task<PdfPCell> GetPdfCellAsync(string resourceKey, Language lang, Font font)
        {
            return new PdfPCell(new Phrase(await _localizationService.GetResourceAsync(resourceKey, lang.Id), font));
        }

        protected virtual PdfPCell GetPdfCell(object text, Font font)
        {
            return new PdfPCell(new Phrase(text.ToString(), font));
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

        public async Task OrderItemsToPdfAsync(Stream stream, Order order, int languageId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

            await PrintOrderItemInformation(stream, orderItems, order, languageId);

        }

        public async Task ShipmentItemsToPdfAsync(Stream stream, Shipment shipment, int languageId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var shipmentItems = await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id);

            await PrintShipmentItemInformation(stream, shipmentItems, languageId);
        }
        public async Task PrintShipmentItemInformation(Stream stream, IList<ShipmentItem> shipmentItems, int languageId = 0)
        {
            if (shipmentItems == null)
                throw new ArgumentNullException(nameof(shipmentItems));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);
            var shipmentItemProductMaps = new Dictionary<int, Product>();
            Order order = null;
            var vendorIds = (await shipmentItems.SelectAwait(async x =>
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(x.OrderItemId);
                if (order == null && orderItem != null)
                {
                    order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
                }
                var product = await _productService.GetProductByIdAsync(orderItem?.ProductId ?? 0);
                shipmentItemProductMaps.Add(x.Id, product);
                return product?.VendorId ?? 0;
            }).ToListAsync()).Distinct().OrderBy(z => z);

            var vendorIdCount = vendorIds.Count();
            var vendorNumber = 0;
            var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? order.CustomerLanguageId : languageId);
            if (lang == null || !lang.Published)
                lang = await _workContext.GetWorkingLanguageAsync();

            foreach (var vendorId in vendorIds)
            {
                // add vendor information at the top of the table
                var vendorDetailsTable = new PdfPTable(1);
                if (lang.Rtl)
                    vendorDetailsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                vendorDetailsTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                vendorDetailsTable.WidthPercentage = 100f;
                vendorDetailsTable.SkipLastFooter = true;

                if (vendorId == 0) // admin
                {
                    vendorDetailsTable.AddCell(await GetParagraphAsync("Plugins.Misc.ZimzoneAdditionalFeature.OrderItemPdfDownload.Admin", lang, font));
                }
                else
                {
                    var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

                    vendorDetailsTable.AddCell(new Paragraph(vendor.Name, font));
                }

                vendorDetailsTable.AddCell(new Paragraph(" "));

                doc.Add(vendorDetailsTable);

                // product table

                var shipmentItemsfromVendor = shipmentItems.Where(x =>
              {
                  var product = shipmentItemProductMaps[x.Id];
                  return product.VendorId == vendorId;
              }).ToList();

                var productsTable = new PdfPTable(4) { WidthPercentage = 100f };
                if (lang.Rtl)
                {
                    productsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productsTable.SetWidths(new[] { 15, 15, 20, 50 });
                }
                else
                {
                    productsTable.SetWidths(new[] { 50, 20, 15, 15 });
                }

                //product name
                var cell = await GetPdfCellAsync("PDFPackagingSlip.ProductName", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //SKU
                cell = await GetPdfCellAsync("PDFPackagingSlip.SKU", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //qty
                cell = await GetPdfCellAsync("PDFPackagingSlip.QTY", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //product cost
                cell = await GetPdfCellAsync("Plugins.Misc.ZimzoneAdditionalFeature.OrderItemPdfDownload.ProductCost", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);


                foreach (var shipmentItem in shipmentItemsfromVendor)
                {
                    var orderItem = await _orderService.GetOrderItemByIdAsync(shipmentItem.OrderItemId);
                    var vendorProduct = await _productService.GetProductByIdAsync(orderItem?.ProductId ?? 0);

                    var productAttribTable = new PdfPTable(1);
                    if (lang.Rtl)
                        productAttribTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    var name = await _localizationService.GetLocalizedAsync(vendorProduct, x => x.Name, lang.Id);
                    productAttribTable.AddCell(new Paragraph(name, font));
                    //attributes
                    if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                    {
                        var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                        productAttribTable.AddCell(attributesParagraph);
                    }

                    //rental info
                    if (vendorProduct.IsRental)
                    {
                        var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                            ? _productService.FormatRentalDate(vendorProduct, orderItem.RentalStartDateUtc.Value) : string.Empty;
                        var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                            ? _productService.FormatRentalDate(vendorProduct, orderItem.RentalEndDateUtc.Value) : string.Empty;
                        var rentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                            rentalStartDate, rentalEndDate);

                        var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                        productAttribTable.AddCell(rentalInfoParagraph);
                    }

                    productsTable.AddCell(productAttribTable);

                    //SKU
                    var sku = await _productService.FormatSkuAsync(vendorProduct, orderItem.AttributesXml);
                    cell = GetPdfCell(sku ?? string.Empty, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //qty
                    cell = GetPdfCell(shipmentItem.Quantity, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //product cost per unit
                    if (Decimal.Compare(vendorProduct.ProductCost, 0m) == 0)
                    {
                        cell = GetPdfCell("N/A", font);
                    }
                    else
                    {
                        cell = GetPdfCell(vendorProduct.ProductCost, font);
                    }
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);
                }

                doc.Add(productsTable);

                vendorNumber++;
                if (vendorNumber < vendorIdCount)
                    doc.NewPage();
            }

            doc.Close();
        }
        public async Task PrintOrderItemInformation(Stream stream, IList<OrderItem> orderItems, Order order, int languageId = 0)
        {

            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));
            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var vendorIds = (await orderItems.SelectAwait(async x =>
            {
                var product = await _productService.GetProductByIdAsync(x.ProductId);
                return product.VendorId;
            }).ToListAsync()).Distinct().OrderBy(z => z);

            var vendorIdCount = vendorIds.Count();
            var vendorNumber = 0;

            var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? order.CustomerLanguageId : languageId);
            if (lang == null || !lang.Published)
                lang = await _workContext.GetWorkingLanguageAsync();

            foreach (var vendorId in vendorIds)
            {
                // add vendor information at the top of the table
                var vendorDetailsTable = new PdfPTable(1);
                if (lang.Rtl)
                    vendorDetailsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                vendorDetailsTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                vendorDetailsTable.WidthPercentage = 100f;
                vendorDetailsTable.SkipLastFooter = true;

                if (vendorId == 0) // admin
                {
                    vendorDetailsTable.AddCell(await GetParagraphAsync("Plugins.Misc.ZimzoneAdditionalFeature.OrderItemPdfDownload.Admin", lang, font));
                }
                else
                {
                    var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

                    vendorDetailsTable.AddCell(new Paragraph(vendor.Name, font));
                }

                vendorDetailsTable.AddCell(new Paragraph(" "));

                doc.Add(vendorDetailsTable);

                // product table

                var orderItemsfromVendor = await orderItems.WhereAwait(async x =>
                {
                    var product = await _productService.GetProductByIdAsync(x.ProductId);
                    return product.VendorId == vendorId;
                }).ToListAsync();

                var productsTable = new PdfPTable(4) { WidthPercentage = 100f };
                if (lang.Rtl)
                {
                    productsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productsTable.SetWidths(new[] { 15, 15, 20, 50 });
                }
                else
                {
                    productsTable.SetWidths(new[] { 50, 20, 15, 15 });
                }

                //product name
                var cell = await GetPdfCellAsync("PDFPackagingSlip.ProductName", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //SKU
                cell = await GetPdfCellAsync("PDFPackagingSlip.SKU", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //qty
                cell = await GetPdfCellAsync("PDFPackagingSlip.QTY", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //product cost
                cell = await GetPdfCellAsync("Plugins.Misc.ZimzoneAdditionalFeature.OrderItemPdfDownload.ProductCost", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);


                foreach (var orderItem in orderItemsfromVendor)
                {
                    var vendorProduct = await _productService.GetProductByIdAsync(orderItem.ProductId);

                    var productAttribTable = new PdfPTable(1);
                    if (lang.Rtl)
                        productAttribTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    var name = await _localizationService.GetLocalizedAsync(vendorProduct, x => x.Name, lang.Id);
                    productAttribTable.AddCell(new Paragraph(name, font));
                    //attributes
                    if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                    {
                        var attributesParagraph = new Paragraph(HtmlHelper.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                        productAttribTable.AddCell(attributesParagraph);
                    }

                    //rental info
                    if (vendorProduct.IsRental)
                    {
                        var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                            ? _productService.FormatRentalDate(vendorProduct, orderItem.RentalStartDateUtc.Value) : string.Empty;
                        var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                            ? _productService.FormatRentalDate(vendorProduct, orderItem.RentalEndDateUtc.Value) : string.Empty;
                        var rentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                            rentalStartDate, rentalEndDate);

                        var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                        productAttribTable.AddCell(rentalInfoParagraph);
                    }

                    productsTable.AddCell(productAttribTable);

                    //SKU
                    var sku = await _productService.FormatSkuAsync(vendorProduct, orderItem.AttributesXml);
                    cell = GetPdfCell(sku ?? string.Empty, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //qty
                    cell = GetPdfCell(orderItem.Quantity, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //product cost per unit
                    if (Decimal.Compare(vendorProduct.ProductCost, 0m) == 0)
                    {
                        cell = GetPdfCell("N/A", font);
                    }
                    else
                    {
                        cell = GetPdfCell(vendorProduct.ProductCost, font);
                    }
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);
                }

                doc.Add(productsTable);

                vendorNumber++;
                if (vendorNumber < vendorIdCount)
                    doc.NewPage();
            }

            doc.Close();
        }
    }
}

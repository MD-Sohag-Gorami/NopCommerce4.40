using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Components
{
    public class GiftCardDeliveryDateViewComponent : NopViewComponent
    {
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly GiftVoucherSettings _giftVoucherSettings;

        public GiftCardDeliveryDateViewComponent(IProductService productService,
            ILocalizationService localizationService,
            GiftVoucherSettings giftVoucherSettings)
        {
            _productService = productService;
            _localizationService = localizationService;
            _giftVoucherSettings = giftVoucherSettings;
        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var productId = (int)additionalData;
            var product = _productService.GetProductByIdAsync(productId).Result;

            if (product == null || !product.IsGiftCard)
            {
                return Content(string.Empty);
            }
            var model = new GiftCardDeliveryDateModel
            {
                ProductSku = product.Sku,
                ProductId = productId,
                DeliveryDateMessage = string.Empty,
            };
            if (_giftVoucherSettings.ZimazonGiftProductSku.Equals(product.Sku))
            {
                model.IsZimazonGiftCard = true;
                model.ValidUpto = "365D";
                model.DeliveryDateMessage = _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Zimazon.DeliveryDateMessage").Result;
            }
            if (_giftVoucherSettings.ElectrosalesGiftProductSku.Equals(product.Sku))
            {
                model.IsElectrosalesCreditVoucher = true;
                model.ValidUpto = "90D";
                model.DeliveryDateMessage = _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Electrosales.DeliveryDateMessage").Result;
            }
            if (string.IsNullOrEmpty(model.DeliveryDateMessage))
            {
                return Content(string.Empty);
            }

            return View(model);
        }


    }
}

using System;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Components
{
    public class CustomGiftCardViewComponent : NopViewComponent
    {
        private readonly GiftVoucherSettings _giftVoucherSettings;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;

        public CustomGiftCardViewComponent(GiftVoucherSettings giftVoucherSettings,
            IProductService productService,
            ILocalizationService localizationService)
        {
            _giftVoucherSettings = giftVoucherSettings;
            _productService = productService;
            _localizationService = localizationService;
        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var customGiftCardModel = (CustomGiftCardModel)additionalData;
            if (!customGiftCardModel.ProductDetailsModel.GiftCard.IsGiftCard)
            {
                return Content(string.Empty);
            }
            var giftCardModel = customGiftCardModel.ProductDetailsModel.GiftCard;
            var product = _productService.GetProductByIdAsync(customGiftCardModel.ProductDetailsModel.Id).Result;
            var message = string.Empty;
            if (product != null)
            {
                if (product.Sku.Equals(_giftVoucherSettings.ZimazonGiftProductSku))
                {
                    message = _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Zimazon.DefaultMessage").Result;
                }
                if (product.Sku.Equals(_giftVoucherSettings.ElectrosalesGiftProductSku))
                {
                    message = _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Electrosales.DefaultMessage").Result;
                }
            }
            var model = new GiftCardAdditionalDataModel
            {
                ProductId = customGiftCardModel.ProductDetailsModel.Id,
                ProductSku = customGiftCardModel.ProductDetailsModel.Sku,
                GiftCardType = giftCardModel.GiftCardType,
                IsGiftCard = giftCardModel.IsGiftCard,
                SenderName = giftCardModel.SenderName,
                SenderEmail = giftCardModel.SenderEmail,
                Message = message,
                GiftCardDeliveryDate = DateTime.UtcNow
            };
            if (customGiftCardModel.ProductDetailsModel.CustomProperties.ContainsKey("GiftCardAdditionalDataModel"))
            {
                var cartItemGiftCardInfo = (GiftCardAdditionalDataModel)customGiftCardModel?.ProductDetailsModel?.CustomProperties["GiftCardAdditionalDataModel"];
                model.FirstName = cartItemGiftCardInfo.FirstName;
                model.LastName = cartItemGiftCardInfo.LastName;
                model.RecipientEmail = cartItemGiftCardInfo.RecipientEmail;
                model.Message = cartItemGiftCardInfo.Message;
                model.PhysicalAddress = cartItemGiftCardInfo.PhysicalAddress;
                model.CellPhoneNumber = cartItemGiftCardInfo.CellPhoneNumber;
                model.IdOrPassportNumber = cartItemGiftCardInfo.IdOrPassportNumber;
                model.GiftCardDeliveryDate = cartItemGiftCardInfo.GiftCardDeliveryDate;
            }
            return View(model);
        }
    }
}

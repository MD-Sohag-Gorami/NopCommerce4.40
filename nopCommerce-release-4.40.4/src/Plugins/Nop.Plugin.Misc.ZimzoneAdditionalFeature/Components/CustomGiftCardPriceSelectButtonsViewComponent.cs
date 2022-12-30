using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Components
{
    public class CustomGiftCardPriceSelectButtonsViewComponent : NopViewComponent
    {
        private readonly GiftVoucherSettings _giftVoucherSettings;
        private readonly IProductService _productService;

        public CustomGiftCardPriceSelectButtonsViewComponent(GiftVoucherSettings giftVoucherSettings,
            IProductService productService)
        {
            _giftVoucherSettings = giftVoucherSettings;
            _productService = productService;
        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var productId = (int)additionalData;
            var product = _productService.GetProductByIdAsync(productId).Result;
            if (product == null)
            {
                return Content(string.Empty);
            }
            if (!product.IsGiftCard)
            {
                return Content(string.Empty);
            }

            var avaiableAmounts = new List<int>();
            avaiableAmounts.AddRange(GetAvailableAmounts(product.Sku));

            if (avaiableAmounts.Count == 0)
            {
                return Content(string.Empty);
            }
            var model = new CustomPriceButtonsModel
            {
                AvailableAmounts = avaiableAmounts,
                ProductId = productId
            };
            return View(model);
        }

        protected IList<int> GetAvailableAmounts(string sku)
        {
            var avaiableAmounts = new List<int>();
            if (_giftVoucherSettings.ZimazonGiftProductSku.Equals(sku))
            {
                avaiableAmounts.AddRange(PrepareAvailableAmounts(_giftVoucherSettings.ZimazonGiftCardAvaiableAmounts));
            }
            if (_giftVoucherSettings.ElectrosalesGiftProductSku.Equals(sku))
            {
                avaiableAmounts.AddRange(PrepareAvailableAmounts(_giftVoucherSettings.ElectrosalesGiftVoucherAvaiableAmounts));
            }
            return avaiableAmounts;
        }
        protected IList<int> PrepareAvailableAmounts(string amounts)
        {
            var avaiableAmounts = new List<int>();
            if (!string.IsNullOrEmpty(amounts))
            {
                if (amounts.Contains(","))
                {
                    var ids = amounts.Split(",").Select(Int32.Parse).ToList();
                    avaiableAmounts.AddRange(ids);
                }
                else
                {
                    avaiableAmounts.Add(int.Parse(amounts));
                }
            }
            return avaiableAmounts;
        }
    }
}

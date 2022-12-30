using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Components
{
    public class GiftCardLinksViewComponent : NopViewComponent
    {
        private readonly ICategoryService _categoryService;
        private readonly GiftVoucherSettings _giftVoucherSettings;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ITopicService _topicService;
        private readonly IProductService _productService;

        public GiftCardLinksViewComponent(ICategoryService categoryService,
            GiftVoucherSettings giftVoucherSettings,
            ICatalogModelFactory catalogModelFactory,
            IUrlRecordService urlRecordService,
            ITopicService topicService,
            IProductService productService)
        {
            _categoryService = categoryService;
            _giftVoucherSettings = giftVoucherSettings;
            _catalogModelFactory = catalogModelFactory;
            _urlRecordService = urlRecordService;
            _topicService = topicService;
            _productService = productService;
        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_giftVoucherSettings.EnableGiftCardAndVoucherLinkOnMegaMenu)
            {
                return Content(string.Empty);
            }
            var zimazonGiftProduct = _productService.GetProductBySkuAsync(_giftVoucherSettings.ZimazonGiftProductSku).Result;
            var electrosalesGiftProduct = _productService.GetProductBySkuAsync(_giftVoucherSettings.ElectrosalesGiftProductSku).Result;

            var model = new GiftVoucherProductsModel
            {
                ZimazonProduct = new MegaMenuProductLinkModel
                {
                    ProductName = zimazonGiftProduct?.Name,
                    SeName = zimazonGiftProduct == null ? string.Empty : _urlRecordService.GetSeNameAsync(zimazonGiftProduct).Result
                },
                ElectroSalesProduct = new MegaMenuProductLinkModel
                {
                    ProductName = electrosalesGiftProduct?.Name,
                    SeName = electrosalesGiftProduct == null ? string.Empty : _urlRecordService.GetSeNameAsync(electrosalesGiftProduct).Result
                }
            };
            return View(model);
        }
    }
}

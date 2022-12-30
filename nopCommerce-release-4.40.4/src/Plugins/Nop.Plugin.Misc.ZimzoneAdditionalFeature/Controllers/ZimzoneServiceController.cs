//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Nop.Core;
//using Nop.Core.Domain.Catalog;
//using Nop.Core.Domain.Orders;
//using Nop.Data;
//using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
//using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;
//using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service;
//using Nop.Services.Catalog;
//using Nop.Services.Localization;
//using Nop.Services.Logging;
//using Nop.Services.Messages;
//using Nop.Services.Orders;
//using Nop.Services.Seo;
//using Nop.Web.Framework.Controllers;
//using Nop.Web.Models.Catalog;

//namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Controllers
//{
//    public class ZimzoneServiceController : BasePluginController
//    {
//        private readonly INotificationService _notificationService;
//        private readonly ILocalizationService _localizationService;
//        private readonly IProductService _productService;
//        private readonly IProductAttributeParser _productAttributeParser;
//        private readonly IShoppingCartService _shoppingCartService;
//        private readonly ICustomerActivityService _customerActivityService;
//        private readonly IStoreContext _storeContext;
//        private readonly IWorkContext _workContext;
//        private readonly ShoppingCartSettings _shoppingCartSettings;
//        private readonly IUrlRecordService _urlRecordService;
//        private readonly IServiceRequestService _serviceRequestService;
//        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;

//        public ZimzoneServiceController(INotificationService notificationService, 
//            ILocalizationService localizationService,
//            IProductService productService,
//            IProductAttributeParser productAttributeParser,
//            IShoppingCartService shoppingCartService,
//            ICustomerActivityService customerActivityService,
//            IStoreContext storeContext,
//            IWorkContext workContext,
//            ShoppingCartSettings shoppingCartSettings,
//            IUrlRecordService urlRecordService,
//            IServiceRequestService serviceRequestService,
//            IZimzoneServiceEntityService zimzoneServiceEntityService)
//        {
//            _notificationService = notificationService;
//            _localizationService = localizationService;
//            _productService = productService;
//            _productAttributeParser = productAttributeParser;
//            _shoppingCartService = shoppingCartService;
//            _customerActivityService = customerActivityService;
//            _storeContext = storeContext;
//            _workContext = workContext;
//            _shoppingCartSettings = shoppingCartSettings;
//            _urlRecordService = urlRecordService;
//            _serviceRequestService = serviceRequestService;
//            _zimzoneServiceEntityService = zimzoneServiceEntityService;
//        }
//        [HttpPost]
//        public async Task<IActionResult> ServiceRequest(int productId, IFormCollection form)
//        {
//            var product = await _productService.GetProductByIdAsync(productId);
//            if (product == null)
//            {
//                return Json(new
//                {
//                    redirect = Url.RouteUrl("Homepage")
//                });
//            }
//            var addToCartWarnings = new List<string>();

//            //entered quantity
//            var quantity = _productAttributeParser.ParseEnteredQuantity(product, form);

//            //product and gift card attributes
//            var attributes = await _productAttributeParser.ParseProductAttributesAsync(product, form, addToCartWarnings);
//            //new shopping cart item
//            addToCartWarnings.AddRange(await _shoppingCartService.GetShoppingCartItemWarningsAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, product,
//                    (await _storeContext.GetCurrentStoreAsync()).Id, attributes, 0,
//                    null, null,quantity, true));



//            if(addToCartWarnings.Any())
//            {
//                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("request.field.missing"));
//                return Redirect(Url.RouteUrl("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) }));
//            }
//            //await SaveItemAsync(addToCartWarnings, product, attributes, quantity);
//            var customer = await _workContext.GetCurrentCustomerAsync();
//            var service = (await _zimzoneServiceEntityService.GetAllZimzoneServiceAsync()).Where(x=>x.ServiceProductId==product.Id).FirstOrDefault();
//            var request = new ZimzoneServiceRequestEntity()
//            {
//                CustomerId = customer?.Id ?? 0,
//                CustomerEmail = customer?.Email,
//                CreatedOn = DateTime.UtcNow,
//                ZimZoneServiceId = service?.Id ?? 0,
//                ServiceDetailsAttr= attributes
//            };
//            await _serviceRequestService.InsertRequestAsync(request);
//            //return await GetProductToCartDetailsAsync(addToCartWarnings, product);
//             _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("request.submitted"));
//            return Redirect(Url.RouteUrl("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) }));
//        }
//        protected virtual async Task<IActionResult> GetProductToCartDetailsAsync(List<string> addToCartWarnings,
//           Product product)
//        {
//            if (addToCartWarnings.Any())
//            {
//                //cannot be added to the cart/wishlist
//                //let's display warnings
//                return Json(new
//                {
//                    success = false,
//                    message = addToCartWarnings.ToArray()
//                });
//            }

//            //activity log
//            await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
//                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);


//            //display notification message and update appropriate blocks
//            var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

//            var updateTopCartSectionHtml = string.Format(
//                await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
//                shoppingCarts.Sum(item => item.Quantity));

//            var updateFlyoutCartSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
//                ? await RenderViewComponentToStringAsync("FlyoutShoppingCart")
//                : string.Empty;

//            return Json(new
//            {
//                success = true,
//                message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"),
//                    Url.RouteUrl("ShoppingCart")),
//                updatetopcartsectionhtml = updateTopCartSectionHtml,
//                updateflyoutcartsectionhtml = updateFlyoutCartSectionHtml
//            });
//        }
//    }
//}

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Web.Controllers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Controllers
{
    public class ErpStockSyncController : BasePublicController
    {
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IZimZoneErpSyncService _zimZoneErpSyncService;
        private readonly IProductService _productService;

        public ErpStockSyncController(IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IZimZoneErpSyncService zimZoneErpSyncService,
            IProductService productService)
        {
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _zimZoneErpSyncService = zimZoneErpSyncService;
            _productService = productService;
        }
        public virtual async Task<IActionResult> SyncStock()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(customer, Core.Domain.Orders.ShoppingCartType.ShoppingCart);
            if (!shoppingCart.Any())
                return Json("{\"success\"=true}");
            var skus = (await _productService.GetProductsByIdsAsync(shoppingCart.Select(x => x.ProductId).ToArray())).Select(x => x.Sku);
            await _zimZoneErpSyncService.SyncErpStocksAsync(skus.ToList());
            return Json("{\"success\"=true}");
        }
    }
}

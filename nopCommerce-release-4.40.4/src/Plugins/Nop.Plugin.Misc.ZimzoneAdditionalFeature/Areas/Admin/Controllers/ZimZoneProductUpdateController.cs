using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.PriceUpdate;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.PriceUpdate;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class ZimZoneProductUpdateController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly ICustomerService _customer;
        private readonly ILocalizationService _localizationService;
        private readonly IZimZonePriceAndStockUpdateService _zimZonePriceAndStockUpdateService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IVendorService _vendorService;
        private readonly IProductPriceAndStockUpdateModelFactory _productPriceAndStockUpdateModelFactory;

        public ZimZoneProductUpdateController(IPermissionService permissionService,
            IWorkContext workContext,
            INotificationService notificationService,
            ICustomerService customer,
            ILocalizationService localizationService,
            IZimZonePriceAndStockUpdateService zimZonePriceAndStockUpdateService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IVendorService vendorService,
            IProductPriceAndStockUpdateModelFactory productPriceAndStockUpdateModelFactory)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _notificationService = notificationService;
            _customer = customer;
            _localizationService = localizationService;
            _zimZonePriceAndStockUpdateService = zimZonePriceAndStockUpdateService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _vendorService = vendorService;
            _productPriceAndStockUpdateModelFactory = productPriceAndStockUpdateModelFactory;
        }

        public virtual async Task<IActionResult> UpdatePriceAndStock()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var isVendor = await _customerService.IsVendorAsync(customer);
            var vendorId = 0;
            var attributeValue = string.Empty;
            if (isVendor)
            {
                vendorId = customer.VendorId;
                var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
                attributeValue = await _genericAttributeService.GetAttributeAsync<string>(vendor, "ProductUpdateExcelImport.LastSuccess");
            }
            var model = new PriceUpdateErrorSearchModel
            {
                LastUpdatedInformation = string.IsNullOrEmpty(attributeValue) ? string.Empty : attributeValue,

            };
            model.SetGridPageSize();
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ImportFromXlsx(IFormFile importexcelfile)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await _zimZonePriceAndStockUpdateService.ImportPriceAndStockInformationsFromXlsxAsync(importexcelfile.OpenReadStream());
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                    return RedirectToAction("UpdatePriceAndStock");
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.UpdatePriceAndStock.Success"));
                return RedirectToAction("UpdatePriceAndStock");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                return RedirectToAction("UpdatePriceAndStock");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ErrorProductList(PriceUpdateErrorSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();
            if (searchModel == null)
            {
                return RedirectToAction("UpdatePriceAndStock");
            }
            //prepare model
            var customer = await _workContext.GetCurrentCustomerAsync();
            var isVendor = await _customerService.IsVendorAsync(customer);
            if (isVendor)
            {
                searchModel.VendorId = customer.VendorId;
            }
            var model = await _productPriceAndStockUpdateModelFactory.PrepareProductUpdateErrorListModelAsync(searchModel);

            return Json(model);
        }
        [HttpGet]
        public virtual async Task<IActionResult> ClearLog()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _zimZonePriceAndStockUpdateService.DeleteImportPriceAndStockErrorLogAsync(customer.VendorId);
            return Content("");
        }
    }
}

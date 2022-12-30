using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class AdditionalFeatureController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IZimzoneServiceModelFactory _zimzoneServiceModelFactory;
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;
        private readonly IProductService _productService;

        public AdditionalFeatureController(IPermissionService permissionService,
            IZimzoneServiceModelFactory zimzoneServiceModelFactory,
            IZimzoneServiceEntityService zimzoneServiceEntityService,
            IProductService productService)
        {
            _permissionService = permissionService;
            _zimzoneServiceModelFactory = zimzoneServiceModelFactory;
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
            _productService = productService;
        }

        public async Task<IActionResult> ServiceList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = new ServiceSearchModel()
            {
                AvailablePageSizes = "10"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Services(ServiceSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();
            if (searchModel == null)
            {
                return RedirectToAction("ServiceList");
            }
            //prepare model
            var model = await _zimzoneServiceModelFactory.PrepareServiceListModelAsync(searchModel);

            return Json(model);
        }
        public async Task<IActionResult> Create()
        {
            var model = new ZimzoneServiceModel();
            return await Task.FromResult(View(model));
        }
        [HttpPost]
        public async Task<IActionResult> Create(ZimzoneServiceModel model)
        {
            if (ModelState.IsValid)
            {
                var zimzoneService = model.ToEntity<ZimzoneServiceEntity>();
                var serviceProduct = await _productService.GetProductByIdAsync(model.ServiceProductId);
                if (serviceProduct != null)
                {
                    zimzoneService.ServiceProductName = serviceProduct.Name;
                    zimzoneService.ServiceProductId = model.ServiceProductId;
                }
                var paymentProduct = await _productService.GetProductByIdAsync(model.ServicePaymentProductId);
                if (serviceProduct != null)
                {
                    zimzoneService.PaymentProductName = paymentProduct.Name;
                    zimzoneService.ServicePaymentProductId = model.ServicePaymentProductId;
                }
                await _zimzoneServiceEntityService.CreateAsync(zimzoneService);
                return await Task.FromResult(RedirectToAction("ServiceList"));
            }
            return View(model);
        }

        public async Task<IActionResult> ChooseService()
        {
            var model = new CustomerRoleProductSearchModel();
            return await Task.FromResult(View(model));
        }
        public async Task<IActionResult> EditService(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var zimzoneService = await _zimzoneServiceEntityService.GetById(id);

            if (zimzoneService == null)
            {
                return RedirectToAction("ServiceList");
            }
            var model = new ZimzoneServiceModel
            {
                ServicePaymentProductId = zimzoneService.ServicePaymentProductId,
                Id = zimzoneService.Id,
                ServiceProductId = zimzoneService.ServiceProductId,
                IsActive = zimzoneService.IsActive,
                ServiceProductName = zimzoneService.ServiceProductName,
                ServicePaymentProductName = zimzoneService.PaymentProductName
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditService(ZimzoneServiceModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            if (ModelState.IsValid)
            {
                var zimzoneService = await _zimzoneServiceEntityService.GetById(model.Id);
                if (zimzoneService != null)
                {
                    zimzoneService.IsActive = model.IsActive;
                    zimzoneService.ServicePaymentProductId = model.ServicePaymentProductId;
                    zimzoneService.ServiceProductId = model.ServiceProductId;

                    var serviceProduct = await _productService.GetProductByIdAsync(model.ServiceProductId);
                    if (serviceProduct != null)
                    {
                        zimzoneService.ServiceProductName = serviceProduct.Name;
                    }
                    var paymentProduct = await _productService.GetProductByIdAsync(model.ServicePaymentProductId);
                    if (serviceProduct != null)
                    {
                        zimzoneService.PaymentProductName = paymentProduct.Name;

                    }
                    await _zimzoneServiceEntityService.UpdateAsync(zimzoneService);
                }
                return RedirectToAction("ServiceList");
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteService(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var service = await _zimzoneServiceEntityService.GetById(id);
            if (service != null)
            {
                await _zimzoneServiceEntityService.DeleteAsync(service);
            }
            return RedirectToAction("ServiceList");
        }
    }
}

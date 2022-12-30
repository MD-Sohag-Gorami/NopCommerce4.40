using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class CustomOrderStatusController : BaseAdminController
    {
        private readonly ICustomOrderStatusModelFactory _customOrderStatusModelFactory;
        private readonly ICustomOrderStatusService _customOrderStatusService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;

        public CustomOrderStatusController(ICustomOrderStatusModelFactory customOrderStatusModelFactory, ICustomOrderStatusService customOrderStatusService, IPermissionService permissionService, ILocalizationService localizationService, ILogger logger)
        {
            _customOrderStatusModelFactory = customOrderStatusModelFactory;
            _customOrderStatusService = customOrderStatusService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _logger = logger;
        }
        public async Task<IActionResult> CustomOrderStatusList()
        {
            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneConnfigurationPanel))
                return AccessDeniedView();

            var searchModel = _customOrderStatusModelFactory.PrepareCustomOrderStatusSearchModel(new CustomOrderStatusSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public async Task<IActionResult> CustomOrderStatusList(CustomOrderStatusSearchModel searchModel) //string customOrderStatusName
        {
            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneConnfigurationPanel))
                return AccessDeniedView();

            var listModel = await _customOrderStatusModelFactory.PrepareCustomOrderStatusListModelAsync(searchModel);
            return Json(listModel);
        }

        public async Task<IActionResult> CustomOrderStatusInsert()
        {
            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneConnfigurationPanel))
                return AccessDeniedView();

            var model = new CustomOrderStatusModel();
            model = _customOrderStatusModelFactory.PrepareCustomOrderStatusModel(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CustomOrderStatusInsert(CustomOrderStatusModel model)
        {
            if (ModelState.IsValid)
            {
                if (!await _customOrderStatusService.ValidateNameAndParentStatus(model))
                {
                    model.Errors.Add(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.CustomOrderStatusInsert.Errors"));
                }
                if (model.Errors.Count > 0)
                {
                    model = _customOrderStatusModelFactory.PrepareCustomOrderStatusModel(model);
                    return View(model);
                }
                await _customOrderStatusService.InsertCustomOrderStatusAsync(model);

                ViewBag.RefreshPage = true;
                return View(model);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CustomOrderStatusUpdate(CustomOrderStatusModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneConnfigurationPanel))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                await _customOrderStatusService.UpdateCustomOrderStatusAsync(model);
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> CustomOrderStatusDelete(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneConnfigurationPanel))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();
            await _customOrderStatusService.DeleteCustomOrderStatusAsync(selectedIds.ToList());
            return Json(new { Result = true });
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomOrderStatusInOrder(CustomStatusListWithOrderModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id > 0)
                {
                    await _customOrderStatusService.UpdateOrderWithCustomStatusAsync(model);
                }
                else
                {
                    model = await _customOrderStatusService.InsertOrderWithCustomStatusAsync(model);
                }
                return Json(new { Result = true, UpdatedModel = model });
            }
            return Json(new { Result = false });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomOrderStatusInOrder(int id)
        {
            if (id > 0)
            {
                var isDeleted = await _customOrderStatusService.DeleteOrderWithCustomStatusAsync(id: id);
                if (isDeleted)
                {
                    return Json(new { Result = true });
                }
            }
            return Json(new { Result = false });
        }

        public async Task<IActionResult> OrderWithCustomStatusList(List<int> orderStatuses = null, List<int> customOrderStatues = null, List<int> paymentStatuses = null, List<int> shippingStatuses = null)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = await _customOrderStatusModelFactory.PrepareOrderWithCustomStatusSearchModelAsync(new OrderWithCustomStatusSearchModel()
            {
                OrderStatusIds = orderStatuses,
                CustomOrderStatusIds = customOrderStatues,
                PaymentStatusIds = paymentStatuses,
                ShippingStatusIds = shippingStatuses
            });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> OrderWithCustomStatusList(OrderWithCustomStatusSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return await AccessDeniedDataTablesJson();

            var model = await _customOrderStatusModelFactory.PrepareOrderWithCustomStatusListModelAsync(searchModel);
            return Json(model);
        }

    }
}

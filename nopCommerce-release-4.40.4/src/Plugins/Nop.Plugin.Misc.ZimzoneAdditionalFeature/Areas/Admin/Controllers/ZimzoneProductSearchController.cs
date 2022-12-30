using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Product;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class ZimzoneProductSearchController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IZimzoneProductModelFactory _zimzoneProductModelFactory;

        public ZimzoneProductSearchController(IPermissionService permissionService,
                                              IZimzoneProductModelFactory zimzoneProductModelFactory)
        {
            _permissionService = permissionService;
            _zimzoneProductModelFactory = zimzoneProductModelFactory;
        }
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = await _zimzoneProductModelFactory.PrepareProductSearchModelAsync(new ZimzoneProductSearchModel());

            return View(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> ProductList(ZimzoneProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _zimzoneProductModelFactory.PrepareProductListModelAsync(searchModel);

            return Json(model);
        }
    }
}

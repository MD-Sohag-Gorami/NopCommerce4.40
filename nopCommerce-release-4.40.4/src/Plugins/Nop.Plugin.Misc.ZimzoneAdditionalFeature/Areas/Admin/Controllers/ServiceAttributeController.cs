using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Service;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class ServiceAttributeController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IProductAttributeService _productAttributeService;

        public ServiceAttributeController(IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IProductAttributeService productAttributeService)
        {
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _productAttributeService = productAttributeService;
        }
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var serviceAttributeSetting = await _settingService.LoadSettingAsync<ServiceAttributeSettings>();

            var model = new ServiceAttributeConfigurationModel
            {
                ServiceNameAttributeId = serviceAttributeSetting.NameAttributeId,
                ServiceEmailAttributeId = serviceAttributeSetting.EmailAttributeId,
                ServiceAddressAttributeId = serviceAttributeSetting.AddressAttributeId,
                ServiceDescriptionAttributeId = serviceAttributeSetting.DescriptionAttributeId,
                ServiceFileAttributeId = serviceAttributeSetting.FileAttributeId,
                AvailableAttributes = await PrepareAvailableAttributesAsync()
            };
            return View(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(ServiceAttributeConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var serviceAttributeSetting = await _settingService.LoadSettingAsync<ServiceAttributeSettings>();

            //save settings
            serviceAttributeSetting.NameAttributeId = model.ServiceNameAttributeId;
            serviceAttributeSetting.EmailAttributeId = model.ServiceEmailAttributeId;
            serviceAttributeSetting.AddressAttributeId = model.ServiceAddressAttributeId;
            serviceAttributeSetting.DescriptionAttributeId = model.ServiceDescriptionAttributeId;
            serviceAttributeSetting.FileAttributeId = model.ServiceFileAttributeId;

            await _settingService.SaveSettingAsync(serviceAttributeSetting);
            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        protected async Task<List<SelectListItem>> PrepareAvailableAttributesAsync()
        {
            var availableAttributes = (await _productAttributeService.GetAllProductAttributesAsync())
                .Select(x =>
                {
                    var item = new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    };
                    return item;
                }).ToList();
            availableAttributes.Insert(0, new SelectListItem
            {
                Text = "None",
                Value = "0"
            });
            return availableAttributes;
        }
    }
}

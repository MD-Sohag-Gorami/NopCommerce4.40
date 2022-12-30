using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Topics;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class GiftVoucherController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ITopicService _topicService;
        private readonly IProductService _productService;
        private readonly IZimzoneServiceModelFactory _zimzoneServiceModelFactory;
        private readonly IZimzoneServiceEntityService _zimzoneServiceEntityService;

        public GiftVoucherController(IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ITopicService topicService,
            IProductService productService,
            IZimzoneServiceModelFactory zimzoneServiceModelFactory,
            IZimzoneServiceEntityService zimzoneServiceEntityService)
        {
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _topicService = topicService;
            _productService = productService;
            _zimzoneServiceModelFactory = zimzoneServiceModelFactory;
            _zimzoneServiceEntityService = zimzoneServiceEntityService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var voucherSetting = await _settingService.LoadSettingAsync<GiftVoucherSettings>(storeScope);

            var model = new VoucherConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,

                EnableGiftCardAndVoucherLinkOnMegaMenu = voucherSetting.EnableGiftCardAndVoucherLinkOnMegaMenu,
                RequireRecipientEmailZimazon = voucherSetting.RequireRecipientEmailZimazon,
                RequireRecipientEmailElectrosales = voucherSetting.RequireRecipientEmailElectrosales,

                EnableCellPhoneNumberZimazon = voucherSetting.EnableCellPhoneNumberZimazon,
                EnableCellPhoneNumberElectrosales = voucherSetting.EnableCellPhoneNumberElectrosales,
                RequireCellPhoneNumberZimazon = voucherSetting.RequireCellPhoneNumberZimazon,
                RequireCellPhoneNumberElectrosales = voucherSetting.RequireCellPhoneNumberElectrosales,
                EnableIdOrPassportNumberZimazon = voucherSetting.EnableIdOrPassportNumberZimazon,
                EnableIdOrPassportNumberElectrosales = voucherSetting.EnableIdOrPassportNumberElectrosales,
                RequireIdOrPassportNumberZimazon = voucherSetting.RequireIdOrPassportNumberZimazon,
                RequireIdOrPassportNumberElectrosales = voucherSetting.RequireIdOrPassportNumberElectrosales,
                EnablePhysicalAddressZimazon = voucherSetting.EnablePhysicalAddressZimazon,
                EnablePhysicalAddressElectrosales = voucherSetting.EnablePhysicalAddressElectrosales,
                RequirePhysicalAddressZimazon = voucherSetting.RequirePhysicalAddressZimazon,
                RequirePhysicalAddressElectrosales = voucherSetting.RequirePhysicalAddressElectrosales,
                ZimazonGiftProductSku = voucherSetting.ZimazonGiftProductSku,
                ElectrosalesGiftProductSku = voucherSetting.ElectrosalesGiftProductSku,
                ZimazonGiftCardAvaiableAmounts = voucherSetting.ZimazonGiftCardAvaiableAmounts,
                ElectrosalesGiftVoucherAvaiableAmounts = voucherSetting.ElectrosalesGiftVoucherAvaiableAmounts
            };
            if (storeScope > 0)
            {
                model.EnableGiftCardAndVoucherLinkOnMegaMenu_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.EnableGiftCardAndVoucherLinkOnMegaMenu, storeScope);
                model.RequireRecipientEmailZimazon_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.RequireRecipientEmailZimazon, storeScope);
                model.RequireRecipientEmailElectrosales_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.RequireRecipientEmailElectrosales, storeScope);

                model.EnableCellPhoneNumberZimazon_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.EnableCellPhoneNumberZimazon, storeScope);
                model.EnableCellPhoneNumberElectrosales_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.EnableCellPhoneNumberElectrosales, storeScope);
                model.RequireCellPhoneNumberZimazon_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.RequireCellPhoneNumberZimazon, storeScope);
                model.RequireCellPhoneNumberElectrosales_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.RequireCellPhoneNumberElectrosales, storeScope);
                model.EnableIdOrPassportNumberZimazon_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.EnableIdOrPassportNumberZimazon, storeScope);
                model.EnableIdOrPassportNumberElectrosales_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.EnableIdOrPassportNumberElectrosales, storeScope);
                model.RequireIdOrPassportNumberZimazon_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.RequireIdOrPassportNumberZimazon, storeScope);
                model.RequireIdOrPassportNumberElectrosales_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.RequireIdOrPassportNumberElectrosales, storeScope);
                model.EnablePhysicalAddressZimazon_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.EnablePhysicalAddressZimazon, storeScope);
                model.EnablePhysicalAddressElectrosales_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.EnablePhysicalAddressElectrosales, storeScope);
                model.RequirePhysicalAddressZimazon_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.RequirePhysicalAddressZimazon, storeScope);
                model.RequirePhysicalAddressElectrosales_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.RequirePhysicalAddressElectrosales, storeScope);
                model.ZimazonGiftProductSku_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.ZimazonGiftProductSku, storeScope);
                model.ElectrosalesGiftProductSku_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.ElectrosalesGiftProductSku, storeScope);

                model.ZimazonGiftCardAvaiableAmounts_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.ZimazonGiftCardAvaiableAmounts, storeScope);
                model.ElectrosalesGiftVoucherAvaiableAmounts_OverrideForStore = await _settingService.SettingExistsAsync(voucherSetting, x => x.ElectrosalesGiftVoucherAvaiableAmounts, storeScope);
            }

            return View(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(VoucherConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var voucherSetting = await _settingService.LoadSettingAsync<GiftVoucherSettings>(storeScope);

            //save settings
            voucherSetting.EnableGiftCardAndVoucherLinkOnMegaMenu = model.EnableGiftCardAndVoucherLinkOnMegaMenu;

            voucherSetting.RequireRecipientEmailZimazon = model.RequireRecipientEmailZimazon;
            voucherSetting.RequireRecipientEmailElectrosales = model.RequireRecipientEmailElectrosales;


            voucherSetting.EnableCellPhoneNumberZimazon = model.EnableCellPhoneNumberZimazon;
            voucherSetting.EnableCellPhoneNumberElectrosales = model.EnableCellPhoneNumberElectrosales;
            voucherSetting.RequireCellPhoneNumberZimazon = model.RequireCellPhoneNumberZimazon;
            voucherSetting.RequireCellPhoneNumberElectrosales = model.RequireCellPhoneNumberElectrosales;

            voucherSetting.EnableIdOrPassportNumberZimazon = model.EnableIdOrPassportNumberZimazon;
            voucherSetting.EnableIdOrPassportNumberElectrosales = model.EnableIdOrPassportNumberElectrosales;
            voucherSetting.RequireIdOrPassportNumberZimazon = model.RequireIdOrPassportNumberZimazon;
            voucherSetting.RequireIdOrPassportNumberElectrosales = model.RequireIdOrPassportNumberElectrosales;

            voucherSetting.EnablePhysicalAddressZimazon = model.EnablePhysicalAddressZimazon;
            voucherSetting.EnablePhysicalAddressElectrosales = model.EnablePhysicalAddressElectrosales;
            voucherSetting.RequirePhysicalAddressZimazon = model.RequirePhysicalAddressZimazon;
            voucherSetting.RequirePhysicalAddressElectrosales = model.RequirePhysicalAddressElectrosales;

            voucherSetting.ZimazonGiftProductSku = model.ZimazonGiftProductSku;
            voucherSetting.ElectrosalesGiftProductSku = model.ElectrosalesGiftProductSku;
            voucherSetting.ZimazonGiftCardAvaiableAmounts = model.ZimazonGiftCardAvaiableAmounts;
            voucherSetting.ElectrosalesGiftVoucherAvaiableAmounts = model.ElectrosalesGiftVoucherAvaiableAmounts;

            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.EnableGiftCardAndVoucherLinkOnMegaMenu, model.EnableGiftCardAndVoucherLinkOnMegaMenu_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.RequireRecipientEmailZimazon, model.RequireRecipientEmailZimazon_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.RequireRecipientEmailElectrosales, model.RequireRecipientEmailElectrosales_OverrideForStore, storeScope, false);


            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.EnableCellPhoneNumberZimazon, model.EnableCellPhoneNumberZimazon_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.EnableCellPhoneNumberElectrosales, model.EnableCellPhoneNumberElectrosales_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.RequireCellPhoneNumberZimazon, model.RequireCellPhoneNumberZimazon_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.RequireCellPhoneNumberElectrosales, model.RequireCellPhoneNumberElectrosales_OverrideForStore, storeScope, false);


            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.EnableIdOrPassportNumberZimazon, model.EnableIdOrPassportNumberZimazon_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.EnableIdOrPassportNumberElectrosales, model.EnableIdOrPassportNumberElectrosales_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.RequireIdOrPassportNumberZimazon, model.RequireIdOrPassportNumberZimazon_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.RequireIdOrPassportNumberElectrosales, model.RequireIdOrPassportNumberElectrosales_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.EnablePhysicalAddressZimazon, model.EnablePhysicalAddressZimazon_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.EnablePhysicalAddressElectrosales, model.EnablePhysicalAddressElectrosales_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.RequirePhysicalAddressZimazon, model.RequirePhysicalAddressZimazon_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.RequirePhysicalAddressElectrosales, model.RequirePhysicalAddressElectrosales_OverrideForStore, storeScope, false);


            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.ZimazonGiftProductSku, model.ZimazonGiftProductSku_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.ElectrosalesGiftProductSku, model.ElectrosalesGiftProductSku_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.ZimazonGiftCardAvaiableAmounts, model.ZimazonGiftCardAvaiableAmounts_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(voucherSetting, x => x.ElectrosalesGiftVoucherAvaiableAmounts, model.ElectrosalesGiftVoucherAvaiableAmounts_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }
    }
}
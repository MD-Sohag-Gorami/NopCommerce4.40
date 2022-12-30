using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.ErpSync.Areas.Admin.Models;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.ErpSync.Areas.Admin.Controllers
{
    public class ErpSyncController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IZimZoneErpSyncService _zimZoneErpSyncService;

        #endregion

        #region Ctor

        public ErpSyncController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IZimZoneErpSyncService zimZoneErpSyncService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _zimZoneErpSyncService = zimZoneErpSyncService;
        }

        #endregion

        #region Methods

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var erpSyncSettings = await _settingService.LoadSettingAsync<ErpSyncSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ProductSyncUrl = erpSyncSettings.ProductSyncUrl,
                CategorySyncUrl = erpSyncSettings.CategorySyncUrl,
                ManufacturerSyncUrl = erpSyncSettings.ManufacturerSyncUrl,
                StockSyncUrl = erpSyncSettings.StockSyncUrl,
                ImageUrlEndpoint = erpSyncSettings.ImageUrlEndpoint,
                BufferTime = erpSyncSettings.BufferTime,
                ManualSyncFrom = erpSyncSettings.ManualSyncFrom,
                SyncTime = erpSyncSettings.SyncTime,
                RequestTimeOutInSeconds = erpSyncSettings.RequestTimeOutInSeconds,

                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.ProductSyncUrl_OverrideForStore = await _settingService.SettingExistsAsync(erpSyncSettings, x => x.ProductSyncUrl, storeScope);
                model.CategorySyncUrl_OverrideForStore = await _settingService.SettingExistsAsync(erpSyncSettings, x => x.CategorySyncUrl, storeScope);
                model.ManufacturerSyncUrl_OverrideForStore = await _settingService.SettingExistsAsync(erpSyncSettings, x => x.ManufacturerSyncUrl, storeScope);
                model.StockSyncUrl_OverrideForStore = await _settingService.SettingExistsAsync(erpSyncSettings, x => x.StockSyncUrl, storeScope);
                model.ImageUrlEndpoint_OverrideForStore = await _settingService.SettingExistsAsync(erpSyncSettings, x => x.ImageUrlEndpoint, storeScope);
                model.BufferTime_OverrideForStore = await _settingService.SettingExistsAsync(erpSyncSettings, x => x.BufferTime, storeScope);
                model.ManualSyncFrom_OverrideForStore = await _settingService.SettingExistsAsync(erpSyncSettings, x => x.ManualSyncFrom, storeScope);
                model.SyncTime_OverrideForStore = await _settingService.SettingExistsAsync(erpSyncSettings, x => x.SyncTime, storeScope);
                model.RequestTimeOutInSeconds_OverrideForStore = await _settingService.SettingExistsAsync(erpSyncSettings, x => x.RequestTimeOutInSeconds, storeScope);
            }

            return View("~/Plugins/Misc.ZimzoneAdditionalFeature/Areas/Admin/Views/ErpSync/Configure.cshtml", model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var erpSyncSettings = await _settingService.LoadSettingAsync<ErpSyncSettings>(storeScope);

            erpSyncSettings.ProductSyncUrl = model.ProductSyncUrl;
            erpSyncSettings.CategorySyncUrl = model.CategorySyncUrl;
            erpSyncSettings.ManufacturerSyncUrl = model.ManufacturerSyncUrl;
            erpSyncSettings.StockSyncUrl = model.StockSyncUrl;
            erpSyncSettings.ImageUrlEndpoint = model.ImageUrlEndpoint;
            erpSyncSettings.BufferTime = model.BufferTime;
            erpSyncSettings.ManualSyncFrom = model.ManualSyncFrom;
            erpSyncSettings.SyncTime = model.SyncTime;
            erpSyncSettings.RequestTimeOutInSeconds = model.RequestTimeOutInSeconds;


            if (!string.IsNullOrWhiteSpace(erpSyncSettings.SyncTime) && erpSyncSettings.SyncTime.Contains(':'))
            {
                var syncTime = erpSyncSettings.SyncTime.Split(':').ToList();
                var currentTime = DateTime.Now;
                if (int.TryParse(syncTime[0], out var hour) && int.TryParse(syncTime[1], out var minute))
                {
                    var nextExecutionTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, hour, minute, 0);

                    if (erpSyncSettings.NextSync.HasValue && DateTime.Compare(erpSyncSettings.NextSync.Value, nextExecutionTime) > 0)
                    {
                        nextExecutionTime = new DateTime(erpSyncSettings.NextSync.Value.Year, erpSyncSettings.NextSync.Value.Month, erpSyncSettings.NextSync.Value.Day, hour, minute, 0);
                    }

                    erpSyncSettings.NextSync = nextExecutionTime;
                }
            }

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.ProductSyncUrl, model.ProductSyncUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.CategorySyncUrl, model.CategorySyncUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.ManufacturerSyncUrl, model.ManufacturerSyncUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.StockSyncUrl, model.StockSyncUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.ImageUrlEndpoint, model.ImageUrlEndpoint_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.BufferTime, model.BufferTime_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.ManualSyncFrom, model.ManualSyncFrom_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.SyncTime, model.SyncTime_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.RequestTimeOutInSeconds, model.RequestTimeOutInSeconds_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.NextSync, true, storeScope, true);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        public IActionResult RunSync()
        {
            _zimZoneErpSyncService.SyncProductsAsync();
            return RedirectToAction("Configure");
        }
        #endregion
    }
}

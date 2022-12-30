using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Plugin.NopStation.Core.Services;
using Nop.Services.Authentication.External;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace Nop.Plugin.NopStation.Core.Helpers
{
    public static class NopStationHelpers
    {
        private static async Task<PermissionRecord> GetPermissionRecordBySystemNameAsync(string systemName, IRepository<PermissionRecord> permissionRecordRepository)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = from pr in permissionRecordRepository.Table
                        where pr.SystemName == systemName
                        orderby pr.Id
                        select pr;

            var permissionRecord = await query.FirstOrDefaultAsync();
            return permissionRecord;
        }

        private static async Task DeletePermissionRecordAsync(PermissionRecord permission, IRepository<PermissionRecord> permissionRecordRepository)
        {
            await permissionRecordRepository.DeleteAsync(permission);
        }

        public static async Task NopStationPluginInstallAsync<TPlugin>(this TPlugin plugin, IPermissionProvider provider = null, bool autoEnable = true) where TPlugin : class, INopStationPlugin
        {
            var settingService = NopInstance.Load<ISettingService>();
            var localizationService = NopInstance.Load<ILocalizationService>();
            var keyValuePairs = plugin.PluginResouces();
            foreach (var keyValuePair in keyValuePairs)
            {
                await localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
            }

            if (provider != null)
            {
                var permissionService = NopInstance.Load<IPermissionService>();
                await permissionService.InstallPermissionsAsync(provider);
            }

            var nopStationCoreSettings = NopInstance.Load<NopStationCoreSettings>();
            if (!nopStationCoreSettings.ActiveNopStationSystemNames?.Any(x => x == plugin.PluginDescriptor.SystemName) ?? false)
            {
                nopStationCoreSettings.ActiveNopStationSystemNames.Add(plugin.PluginDescriptor.SystemName);
                await settingService.SaveSettingAsync(nopStationCoreSettings);
            }

            if (autoEnable)
                await EnablePlugin(plugin, settingService);
        }

        public static async Task NopStationPluginUninstallAsync<TPlugin>(this TPlugin plugin, IPermissionProvider provider = null) where TPlugin : class, INopStationPlugin
        {
            var localizationService = NopInstance.Load<ILocalizationService>();
            var keyValuePairs = plugin.PluginResouces();
            foreach (var keyValuePair in keyValuePairs)
            {
                await localizationService.DeleteLocaleResourceAsync(keyValuePair.Key);
            }

            if (provider != null)
            {
                var permissionService = NopInstance.Load<IPermissionService>();
                var permissionRecordRepository = NopInstance.Load<IRepository<PermissionRecord>>();
                var permissions = provider.GetPermissions();

                foreach (var permission in permissions)
                {
                    var permission1 = await GetPermissionRecordBySystemNameAsync(permission.SystemName, permissionRecordRepository);
                    if (permission1 == null)
                        continue;

                    await DeletePermissionRecordAsync(permission1, permissionRecordRepository);
                }
            }

            var nopStationCoreSettings = NopInstance.Load<NopStationCoreSettings>();
            if (nopStationCoreSettings.ActiveNopStationSystemNames.Any(x => x == plugin.PluginDescriptor.SystemName))
            {
                var settingService = NopInstance.Load<ISettingService>();
                nopStationCoreSettings.ActiveNopStationSystemNames.Remove(plugin.PluginDescriptor.SystemName);
                await settingService.SaveSettingAsync(nopStationCoreSettings);
            }
        }

        public static async Task EnablePlugin<TPlugin>(this TPlugin plugin, ISettingService settingService) where TPlugin : INopStationPlugin
        {
            try
            {
                var pluginIsActive = false;
                switch (plugin)
                {
                    case IPaymentMethod paymentMethod:
                        var paymentPluginManager = NopInstance.Load<IPaymentPluginManager>();
                        pluginIsActive = paymentPluginManager.IsPluginActive(paymentMethod);
                        if (!pluginIsActive)
                        {
                            var paymentSettings = NopInstance.Load<PaymentSettings>();
                            paymentSettings.ActivePaymentMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
                            await settingService.SaveSettingAsync(paymentSettings);
                        }

                        break;
                    case IShippingRateComputationMethod shippingRateComputationMethod:
                        var shippingPluginManager = NopInstance.Load<IShippingPluginManager>();
                        pluginIsActive = shippingPluginManager.IsPluginActive(shippingRateComputationMethod);
                        if (!pluginIsActive)
                        {
                            var shippingSettings = NopInstance.Load<ShippingSettings>();
                            shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
                            await settingService.SaveSettingAsync(shippingSettings);
                        }

                        break;
                    case IPickupPointProvider pickupPointProvider:
                        var pickupPluginManager = NopInstance.Load<IPickupPluginManager>();
                        pluginIsActive = pickupPluginManager.IsPluginActive(pickupPointProvider);
                        if (!pluginIsActive)
                        {
                            var shippingSettings = NopInstance.Load<ShippingSettings>();
                            shippingSettings.ActivePickupPointProviderSystemNames.Add(plugin.PluginDescriptor.SystemName);
                            await settingService.SaveSettingAsync(shippingSettings);
                        }

                        break;
                    case IExternalAuthenticationMethod externalAuthenticationMethod:
                        var authenticationPluginManager = NopInstance.Load<IAuthenticationPluginManager>();
                        pluginIsActive = authenticationPluginManager.IsPluginActive(externalAuthenticationMethod);
                        if (!pluginIsActive)
                        {
                            var externalAuthenticationSettings = NopInstance.Load<ExternalAuthenticationSettings>();
                            externalAuthenticationSettings.ActiveAuthenticationMethodSystemNames.Add(plugin.PluginDescriptor.SystemName);
                            await settingService.SaveSettingAsync(externalAuthenticationSettings);
                        }

                        break;
                    case IWidgetPlugin widgetPlugin:
                        var widgetPluginManager = NopInstance.Load<IWidgetPluginManager>();
                        pluginIsActive = widgetPluginManager.IsPluginActive(widgetPlugin);
                        if (!pluginIsActive)
                        {
                            var widgetSettings = NopInstance.Load<WidgetSettings>();
                            widgetSettings.ActiveWidgetSystemNames.Add(plugin.PluginDescriptor.SystemName);
                            await settingService.SaveSettingAsync(widgetSettings);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                await NopInstance.Load<ILogger>()
                      .ErrorAsync($"Failed to enable {plugin.PluginDescriptor.SystemName}: {ex.Message}", ex);
            }
        }
    }
}

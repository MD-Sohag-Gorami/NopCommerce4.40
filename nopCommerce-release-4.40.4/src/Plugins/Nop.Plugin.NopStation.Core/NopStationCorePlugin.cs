using Nop.Core;
using Nop.Services.Plugins;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using Nop.Plugin.NopStation.Core.Services;
using System.Collections.Generic;
using Nop.Plugin.NopStation.Core.Helpers;
using System.Threading.Tasks;
using Nop.Services.Configuration;
using System;
using System.Linq;
using Nop.Data;

namespace Nop.Plugin.NopStation.Core
{
    public class NopStationCorePlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly INopDataProvider _dataProvider;

        public NopStationCorePlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            INopDataProvider dataProvider)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _dataProvider = dataProvider;
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/NopStationCore/Configure";
        }

        public override async Task InstallAsync()
        {
            var settings = new NopStationCoreSettings()
            {
                AllowedCustomerRoleIds = new List<int> { 1, 2 }
            };
            await _settingService.SaveSettingAsync(settings);

            await this.NopStationPluginInstallAsync(new CorePermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            if(currentVersion == "4.4.1" && targetVersion == "4.4.5")
            {
                var dataSettings = DataSettingsManager.LoadSettings();
                if (dataSettings != null)
                {
                    if (dataSettings.DataProvider == DataProviderType.SqlServer)
                    {
                        await _dataProvider.ExecuteNonQueryAsync("CREATE TABLE NS_License ([Id][int] IDENTITY(1, 1) NOT NULL,[Key][nvarchar](2000) NOT NULL)");
                    }
                    else if (dataSettings.DataProvider == DataProviderType.MySql)
                    {
                        await _dataProvider.ExecuteNonQueryAsync(@"CREATE TABLE NS_License (Id int not null AUTO_INCREMENT ,`Key` varchar(2000) NOT NULL, PRIMARY KEY (Id))");
                    }
                }
            }
        }

        public override async Task UninstallAsync()
        {
            await this.NopStationPluginUninstallAsync(new CorePermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ManageConfiguration))
            {
                var config = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.Configuration"),
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/NopStationCore/Configure",
                    SystemName = "NopStationCore.Configure"
                };
                await _nopStationCoreService.ManageSiteMapAsync(rootNode, config, NopStationMenuType.Core);

                var resource = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.LocaleResources"),
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/NopStationCore/LocaleResource",
                    SystemName = "NopStationCore.LocaleResources"
                };
                await _nopStationCoreService.ManageSiteMapAsync(rootNode, resource, NopStationMenuType.Core);

                var system = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.PluginInfo"),
                    Visible = true,
                    IconClass = "fa fa-cog",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/NopStationCore/PluginInfo",
                    SystemName = "NopStationCore.PluginInfo"
                };
                await _nopStationCoreService.ManageSiteMapAsync(rootNode, system, NopStationMenuType.Root);
            }

            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
            {
                var acl = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.ACL"),
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/NopStationCore/Permissions",
                    SystemName = "NopStationCore.ACL"
                };
                await _nopStationCoreService.ManageSiteMapAsync(rootNode, acl, NopStationMenuType.Core);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ManageLicense))
            {
                var license = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.License"),
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/NopStationLicense/License",
                    SystemName = "NopStationCore.License"
                };
                await _nopStationCoreService.ManageSiteMapAsync(rootNode, license, NopStationMenuType.Core);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ManageConfiguration))
            {
                var reportBug = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.ReportBug"),
                    Visible = true,
                    IconClass = "fa fa-bug",
                    Url = "https://www.nop-station.com/report-bug?utm_source=admin-panel&utm_medium=products&utm_campaign=report-bug",
                    OpenUrlInNewTab = true
                };
                await _nopStationCoreService.ManageSiteMapAsync(rootNode, reportBug, NopStationMenuType.Root);
            }
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.PluginInfo", "Nop-Station plugin information"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Configuration", "Core settings"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.LocaleResources", "String resources"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.ACL", "Access control list"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.License", "License"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Menu.NopStation", "Nop Station"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Menu.PluginInfo", "Plugin information"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Menu.Configuration", "Configuration"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Menu.LocaleResources", "String resources"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Menu.ACL", "Access control list"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Menu.License", "License"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Menu.Core", "Core settings"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Menu.Themes", "Themes"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Menu.Plugins", "Plugins"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Menu.ReportBug", "Report a bug"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.License.InvalidProductKey", "Your product key is not valid."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.License.InvalidForDomain", "Your product key is not valid for this domain."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.License.InvalidForNOPVersion", "Your product key is not valid for this nopCommerce version."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.License.Saved", "Your product key has been saved successfully."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.License.LicenseString", "License string"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.License.LicenseString.Hint", "Nop-station plugin/theme license string."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Common.Menu.Documentation", "Documentation"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Resources.EditAccessDenied", "For security purposes, the feature you have requested is not available on this site."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Resources.FailedToSave", "Failed to save resource string."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Resources.Fields.Name", "Name"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Resources.Fields.Value", "Value"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Resources.List.SearchPluginSystemName", "Plugin"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Resources.List.SearchPluginSystemName.Hint", "Search resource string by plugin."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Resources.List.SearchResourceName", "Resource name"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Resources.List.SearchResourceName.Hint", "Search resource string by resource name."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Resources.List.SearchLanguageId", "Language"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Resources.List.SearchLanguageId.Hint", "Search resource string by language."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Resources.List.SearchPluginSystemName.All", "All"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Configuration.Fields.EnableCORS.ChangeHint", "Restart your application after changing this setting value."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Configuration.Fields.EnableCORS", "Enable CORS"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Configuration.Fields.EnableCORS.Hint", "Check to enable CORS. It will add \"Access-Control-Allow-Origin\" header for every api response."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Configuration.AdminCanNotBeRestricted", "Admin role can not be restricted."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Configuration.Fields.RestrictMainMenuByCustomerRoles", "Restrict main menu by customer roles"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Configuration.Fields.RestrictMainMenuByCustomerRoles.Hint", "Restrict main menu (Nop Station) by customer roles."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Configuration.Fields.AllowedCustomerRoles", "Allowed customer roles"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Core.Configuration.Fields.AllowedCustomerRoles.Hint", "Select allowed customer roles to access Nop Station plugin menus. Make sure proper access provided for these customer roles from 'Access control list' page."));
            
            list.Add(new KeyValuePair<string, string>("NopStation.Core.Request.Common.Ok", "Request success"));
            list.Add(new KeyValuePair<string, string>("NopStation.Core.Request.Common.BadRequest", "Bad request"));
            list.Add(new KeyValuePair<string, string>("NopStation.Core.Request.Common.Unauthorized", "Unauthorized"));
            list.Add(new KeyValuePair<string, string>("NopStation.Core.Request.Common.NotFound", "Not found"));
            list.Add(new KeyValuePair<string, string>("NopStation.Core.Request.Common.InternalServerError", "Internal server error"));

            return list;
        }
    }
}
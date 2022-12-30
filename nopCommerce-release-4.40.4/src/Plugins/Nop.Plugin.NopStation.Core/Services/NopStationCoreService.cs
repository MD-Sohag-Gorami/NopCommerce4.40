using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.NopStation.Core.Services
{
    public class NopStationCoreService : INopStationCoreService
    {
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly NopStationCoreSettings _coreSettings;

        public NopStationCoreService(IWorkContext workContext,
            ILocalizationService localizationService,
            ICustomerService customerService,
            NopStationCoreSettings coreSettings)
        {
            _workContext = workContext;
            _localizationService = localizationService;
            _customerService = customerService;
            _coreSettings = coreSettings;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode, SiteMapNode childNode, NopStationMenuType menuType = NopStationMenuType.Root)
        {
            var nopstationNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "NopStation");
            if (nopstationNode == null)
            {
                nopstationNode = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.NopStation"),
                    SystemName = "NopStation",
                    IconClass = "icon-nop-station",
                    Visible = !_coreSettings.RestrictMainMenuByCustomerRoles
                };
                rootNode.ChildNodes.Add(nopstationNode);
            }

            var crids = await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
            foreach (var crid in crids)
            {
                if (_coreSettings.AllowedCustomerRoleIds.Contains(crid) ||
                    await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                {
                    nopstationNode.Visible = true;
                    break;
                }
            }

            var pluginNode = nopstationNode.ChildNodes.FirstOrDefault(x => x.SystemName == "NopStationPlugin");
            if (pluginNode == null)
            {
                pluginNode = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.Plugins"),
                    SystemName = "NopStationPlugin",
                    IconClass = "icon-plugins"
                };
                nopstationNode.ChildNodes.Add(pluginNode);
            }

            var themeNode = nopstationNode.ChildNodes.FirstOrDefault(x => x.SystemName == "NopStationTheme");
            if (themeNode == null)
            {
                themeNode = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.Themes"),
                    SystemName = "NopStationTheme",
                    IconClass = "icon-themes"
                };
                nopstationNode.ChildNodes.Add(themeNode);
            }

            var coreNode = nopstationNode.ChildNodes.FirstOrDefault(x => x.SystemName == "NopStationCore");
            if (coreNode == null)
            {
                coreNode = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Core.Menu.Core"),
                    SystemName = "NopStationCore",
                    IconClass = "fa fa-wrench"
                };
                nopstationNode.ChildNodes.Add(coreNode);
            }

            if (menuType == NopStationMenuType.Plugin)
            {
                pluginNode.Visible = true;
                pluginNode.ChildNodes.Add(childNode);
            }
            else if (menuType == NopStationMenuType.Core)
            {
                coreNode.Visible = true;
                coreNode.ChildNodes.Add(childNode);
            }
            else if (menuType == NopStationMenuType.Theme)
            {
                themeNode.Visible = true;
                themeNode.ChildNodes.Add(childNode);
            }
            else if (menuType == NopStationMenuType.Root)
            {
                nopstationNode.ChildNodes.Add(childNode);
            }
            else
            {
                rootNode.ChildNodes.Add(childNode);
            }
        }
    }
}

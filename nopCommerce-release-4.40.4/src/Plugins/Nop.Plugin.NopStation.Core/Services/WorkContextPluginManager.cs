using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Plugin.NopStation.Core.Services;
using Nop.Services.Customers;
using Nop.Services.Plugins;

namespace Nop.Plugin.NopStation.Core.Infrastructure
{
    public class WorkContextPluginManager : PluginManager<IWorkContextPlugin>, IWorkContextPluginManager
    {
        #region Fields

        private readonly NopStationCoreSettings _nopStationCoreSettings;

        #endregion

        #region Ctor

        public WorkContextPluginManager(NopStationCoreSettings nopStationCoreSettings,
            IPluginService pluginService,
            ICustomerService customerService) : base(customerService, pluginService)
        {
            _nopStationCoreSettings = nopStationCoreSettings;
        }

        #endregion

        public async virtual Task<IList<IWorkContextPlugin>> LoadWorkContextPluginsAsync(Customer customer = null, string pluginSystemName = "", 
            int storeId = 0)
        {
            return (await LoadAllPluginsAsync(customer, storeId))
                .Where(plugin => string.IsNullOrWhiteSpace(pluginSystemName) ||
                    plugin.PluginDescriptor.SystemName.Equals(pluginSystemName))
                .ToList();
        }
    }
}

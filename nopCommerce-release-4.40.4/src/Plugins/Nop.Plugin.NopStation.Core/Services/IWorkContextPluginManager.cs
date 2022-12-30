using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.NopStation.Core.Services;

namespace Nop.Plugin.NopStation.Core.Infrastructure
{
    public interface IWorkContextPluginManager
    {
        Task<IList<IWorkContextPlugin>> LoadWorkContextPluginsAsync(Customer customer = null, string pluginSystemName = "",
            int storeId = 0);
    }
}
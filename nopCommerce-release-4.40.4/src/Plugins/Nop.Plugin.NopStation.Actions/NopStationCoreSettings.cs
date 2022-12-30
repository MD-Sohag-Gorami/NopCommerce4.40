using System.Collections.Generic;
using Nop.Core.Configuration;

namespace Nop.Plugin.NopStation.Core
{
    public class NopStationCoreSettings : ISettings
    {
        public NopStationCoreSettings()
        {
            ActiveNopStationSystemNames = new List<string>();
            AllowedCustomerRoleIds = new List<int>();
        }

        public List<string> ActiveNopStationSystemNames { get; set; }

        public bool RestrictMainMenuByCustomerRoles { get; set; }

        public List<int> AllowedCustomerRoleIds { get; set; }
    }
}

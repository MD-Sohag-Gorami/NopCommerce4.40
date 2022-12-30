using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature
{
    public class ZimZonePermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord AccessQueriesPanel = new PermissionRecord
        {
            Name = "Access Cusstomer Query",
            SystemName = "CustomerQueriesPanel",
            Category = "Zimzone"
        };

        public static readonly PermissionRecord ZimzoneConnfigurationPanel = new PermissionRecord
        {
            Name = "Access Zimzone Connfiguration",
            SystemName = "ZimzoneConnfigurationPanel",
            Category = "Zimzone"
        };

        public static readonly PermissionRecord ZimzoneServiceRequestPanel = new PermissionRecord
        {
            Name = "Access Service Request Area",
            SystemName = "ZimzoneServiceRequestPanel",
            Category = "Zimzone"
        };
        public static readonly PermissionRecord ZimzoneShipmentManage = new PermissionRecord
        {
            Name = "Access Shipment Management Area",
            SystemName = "ZimzoneShipmentManage",
            Category = "Zimzone"
        };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        AccessQueriesPanel,
                        ZimzoneConnfigurationPanel,
                        ZimzoneServiceRequestPanel,
                        ZimzoneShipmentManage
                    }
                ),
                (
                    NopCustomerDefaults.ForumModeratorsRoleName,
                    new[]
                    {
                       AccessQueriesPanel
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                AccessQueriesPanel,
                ZimzoneConnfigurationPanel,
                ZimzoneServiceRequestPanel,
                ZimzoneShipmentManage
            };
        }
    }
}

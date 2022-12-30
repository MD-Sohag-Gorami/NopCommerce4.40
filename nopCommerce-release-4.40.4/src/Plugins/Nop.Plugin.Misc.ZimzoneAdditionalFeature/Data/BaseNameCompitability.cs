using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.ErpSync.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Erp;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Query;

namespace Nop.Plugin.Misc.ErpSync.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(ZimzoneServiceEntity), "NS_ZimzoneServiceEntity" },
            { typeof(ZimzoneServiceRequestEntity), "NS_ZimzoneServiceRequestEntity" },
            { typeof(ErpProductPicture), "NS_ErpProductPicture" },
            { typeof(ErpCategory), "NS_ErpCategory" },
            { typeof(Question), "NS_Question" },
            { typeof(CustomOrderStatus), "NS_CustomOrderStatus" },
            { typeof(OrderWithCustomStatus), "NS_OrderWithCustomStatus" },
            { typeof(ErpManufacturer), "NS_ErpManufacturer" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}

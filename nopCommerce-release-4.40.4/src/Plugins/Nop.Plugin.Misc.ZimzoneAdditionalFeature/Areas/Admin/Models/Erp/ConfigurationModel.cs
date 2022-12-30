using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ErpSync.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpSync.ProductSyncUrl")]
        public string ProductSyncUrl { get; set; }
        public bool ProductSyncUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpSync.CategorySyncUrl")]
        public string CategorySyncUrl { get; set; }

        public bool CategorySyncUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpSync.ManufacturerSyncUrl")]
        public string ManufacturerSyncUrl { get; set; }

        public bool ManufacturerSyncUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpSync.StockSyncUrl")]
        public string StockSyncUrl { get; set; }

        public bool StockSyncUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpSync.ImageUrlEndpoint")]
        public string ImageUrlEndpoint { get; set; }

        public bool ImageUrlEndpoint_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpSync.BufferTime")]
        public int BufferTime { get; set; }

        public bool BufferTime_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpSync.ManualSyncFrom")]
        public DateTime ManualSyncFrom { get; set; }
        public bool ManualSyncFrom_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpSync.SyncTime")]
        public string SyncTime { get; set; }
        public bool SyncTime_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ErpSync.RequestTimeOutInSeconds")]
        public int? RequestTimeOutInSeconds { get; set; }
        public bool RequestTimeOutInSeconds_OverrideForStore { get; set; }
    }
}

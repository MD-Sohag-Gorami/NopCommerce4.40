using System;
using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.ErpSync
{
    public class ErpSyncSettings : ISettings
    {
        public string ProductSyncUrl { get; set; }
        public string CategorySyncUrl { get; set; }
        public string ManufacturerSyncUrl { get; set; }
        public string StockSyncUrl { get; set; }
        public string ImageUrlEndpoint { get; set; }
        public int BufferTime { get; set; }
        public DateTime ManualSyncFrom { get; set; }
        public string SyncTime { get; set; }
        public bool IsRunningSync { get; set; }
        public DateTime? LastSync { get; set; }
        public DateTime? NextSync { get; set; }
        public int? RequestTimeOutInSeconds { get; set; }
    }
}
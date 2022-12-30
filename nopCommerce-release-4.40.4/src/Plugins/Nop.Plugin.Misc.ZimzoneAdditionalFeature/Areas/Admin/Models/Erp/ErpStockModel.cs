using Newtonsoft.Json;

namespace Nop.Plugin.Misc.ErpSync.Areas.Admin.Models
{
    public class ErpStockModel
    {
        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("stockOnHand")]
        public int? StockOnHand { get; set; }
    }
}

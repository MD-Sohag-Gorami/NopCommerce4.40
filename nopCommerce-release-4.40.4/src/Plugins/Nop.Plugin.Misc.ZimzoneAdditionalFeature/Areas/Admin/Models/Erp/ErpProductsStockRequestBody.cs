using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Erp
{
    public class ErpProductsStockRequestBody
    {
        [JsonProperty("skus")]
        public IList<string> Skus { get; set; }
    }
}

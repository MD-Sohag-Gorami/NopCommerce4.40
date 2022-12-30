using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ErpSync.Areas.Admin.Models
{
    public record ErpProductModel : BaseNopEntityModel
    {
        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("published")]
        public bool? Published { get; set; }

        [JsonProperty("uom")]
        public string Uom { get; set; }

        [JsonProperty("specifications")]
        public List<Specification> Specifications { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("shortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("fullDescription")]
        public string FullDescription { get; set; }

        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [JsonProperty("productBrand")]
        public string ProductBrand { get; set; }

        [JsonProperty("brandId")]
        public string BrandId { get; set; }

        [JsonProperty("depth")]
        public decimal? Depth { get; set; }

        [JsonProperty("height")]
        public decimal? Height { get; set; }

        [JsonProperty("length")]
        public decimal? Length { get; set; }

        [JsonProperty("weight")]
        public decimal? Weight { get; set; }

        [JsonProperty("customerUnit")]
        public string CustomerUnit { get; set; }

        [JsonProperty("metric")]
        public string Metric { get; set; }

        [JsonProperty("metricValue")]
        public string MetricValue { get; set; }

        [JsonProperty("warranty")]
        public string Warranty { get; set; }

        [JsonProperty("imagePaths")]
        public List<string> ImagePaths { get; set; }

        [JsonProperty("categoryPath")]
        public object CategoryPath { get; set; }

        [JsonProperty("categories")]
        public List<string> Categories { get; set; }

        [JsonProperty("stockOnHand")]
        public int? StockOnHand { get; set; }
    }
}

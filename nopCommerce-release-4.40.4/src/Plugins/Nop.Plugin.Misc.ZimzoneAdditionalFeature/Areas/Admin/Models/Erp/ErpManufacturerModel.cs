using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Erp
{
    public class ErpManufacturerModel
    {
        [JsonProperty("_id")]
        public string Id { get; set; }
        [JsonProperty("brandName")]
        public string BrandName { get; set; }
        [JsonProperty("active")]
        public bool Active { get; set; }
        [JsonProperty("imageName")]
        public string? ImageName { get; set; }
        [JsonProperty("updatedAt")]
        public DateTime? UpdatedAt { get; set; }
        [JsonProperty("updatedBy")]
        public string? UpdatedBy { get; set; }
        [JsonProperty("imagePath")]
        public string? ImagePath { get; set; }
    }
}

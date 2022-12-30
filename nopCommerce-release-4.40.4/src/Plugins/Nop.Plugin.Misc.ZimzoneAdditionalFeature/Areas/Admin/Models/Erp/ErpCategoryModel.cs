using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.ErpSync.Areas.Admin.Models
{
    public class ErpCategoryModel
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("orgHierarchy")]
        public List<object> OrgHierarchy { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lowestLevelCategory")]
        public bool LowestLevelCategory { get; set; }

        [JsonProperty("rootCategory")]
        public string RootCategory { get; set; }

        [JsonProperty("__v")]
        public float V { get; set; }

        [JsonProperty("parentId")]
        public string ParentId { get; set; }
    }
}

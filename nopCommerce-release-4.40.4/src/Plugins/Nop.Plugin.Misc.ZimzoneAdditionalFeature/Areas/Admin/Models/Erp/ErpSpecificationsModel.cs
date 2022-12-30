using Newtonsoft.Json;

namespace Nop.Plugin.Misc.ErpSync.Areas.Admin.Models
{
    public class Specification
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}

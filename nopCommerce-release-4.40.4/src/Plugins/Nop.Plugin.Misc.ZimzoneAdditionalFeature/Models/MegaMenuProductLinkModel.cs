using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models
{
    public record MegaMenuProductLinkModel : BaseNopModel
    {
        public string ProductName { get; set; }
        public string SeName { get; set; }
    }
}

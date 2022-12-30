using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.PriceUpdate
{
    public record PriceUpdateErrorSearchModel : BaseSearchModel
    {
        public string LastUpdatedInformation { get; set; }
        public int VendorId { get; set; }
    }
}

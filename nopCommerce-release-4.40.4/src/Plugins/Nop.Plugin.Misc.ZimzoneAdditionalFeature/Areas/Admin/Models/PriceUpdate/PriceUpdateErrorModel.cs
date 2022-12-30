using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.PriceUpdate
{
    public record PriceUpdateErrorModel : BaseNopEntityModel
    {
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int VendorId { get; set; }
        public string ErrorMessage { get; set; }
    }
}

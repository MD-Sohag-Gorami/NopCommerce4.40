using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models
{
    public record GiftVoucherProductsModel : BaseNopModel
    {
        public MegaMenuProductLinkModel ZimazonProduct { get; set; }
        public MegaMenuProductLinkModel ElectroSalesProduct { get; set; }
    }

}

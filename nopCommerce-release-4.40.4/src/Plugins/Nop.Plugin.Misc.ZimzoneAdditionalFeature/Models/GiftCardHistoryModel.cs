using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models
{
    public record GiftCardHistoryModel : BaseNopEntityModel
    {
        public string Coupon { get; set; }
        public string TotalAmount { get; set; }
        public string RemainingAmount { get; set; }
    }
}

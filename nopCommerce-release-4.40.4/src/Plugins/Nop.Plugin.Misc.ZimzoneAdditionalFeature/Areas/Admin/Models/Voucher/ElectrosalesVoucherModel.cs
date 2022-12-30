using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Voucher
{
    public record ElectrosalesVoucherModel : GiftCardModel
    {
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVoucher.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }
    }
}

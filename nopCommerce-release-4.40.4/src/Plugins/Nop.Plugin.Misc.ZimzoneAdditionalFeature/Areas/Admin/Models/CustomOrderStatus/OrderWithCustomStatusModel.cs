using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus
{
    public record OrderWithCustomStatusModel : OrderModel
    {
        public int CustomOrderStatusId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.CustomOrderStatusName")]
        public string CustomOrderStatusName { get; set; }
    }
}

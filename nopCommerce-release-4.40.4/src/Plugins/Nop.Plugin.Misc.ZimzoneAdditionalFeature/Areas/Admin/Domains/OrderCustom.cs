using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains
{
    public class OrderCustom
    {
        public Order Order { get; set; }
        public int CustomOrderStatusId { get; set; }
        public string CustomOrderStatusName { get; set; }
    }
}

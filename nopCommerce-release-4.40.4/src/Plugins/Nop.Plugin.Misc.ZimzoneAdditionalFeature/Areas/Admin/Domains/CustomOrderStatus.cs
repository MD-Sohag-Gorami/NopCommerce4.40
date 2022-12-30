using Nop.Core;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains
{
    public class CustomOrderStatus : BaseEntity
    {
        public string CustomOrderStatusName { get; set; }
        public int DisplayOrder { get; set; }
        public int ParentOrderStatusId { get; set; }
    }
}

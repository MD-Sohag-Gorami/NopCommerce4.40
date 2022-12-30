using Nop.Core;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains
{
    public class OrderWithCustomStatus : BaseEntity
    {
        public int CustomOrderStatusId { get; set; }
        public int ParentOrderStatusId { get; set; }
        public int OrderId { get; set; }
    }
}

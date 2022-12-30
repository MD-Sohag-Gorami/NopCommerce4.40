using Nop.Core;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains
{
    public class ZimzoneServiceEntity : BaseEntity
    {
        public int ServiceProductId { get; set; }
        public string ServiceProductName { get; set; }
        public int ServicePaymentProductId { get; set; }
        public string PaymentProductName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}

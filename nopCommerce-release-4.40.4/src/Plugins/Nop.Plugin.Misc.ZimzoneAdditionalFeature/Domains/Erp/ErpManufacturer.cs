using System;
using Nop.Core;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Erp
{
    public class ErpManufacturer : BaseEntity
    {
        public string? BrandName { get; set; }
        public bool Active { get; set; }
        public int NopManufacturerId { get; set; }
        public string ErpManufacturerId { get; set; }
        public string? ImageLink { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}

using System;
using Nop.Core;

namespace Nop.Plugin.Misc.ErpSync.Domains
{
    public class ErpProductPicture : BaseEntity
    {
        public int NopProductId { get; set; }
        public string Sku { get; set; }
        public string ImageLink { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}

using System;
using Nop.Core;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains
{
    public class ZimzoneServiceRequestEntity : BaseEntity
    {
        public int ZimZoneServiceId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerName { get; set; }
        public string ServiceDetailsAttr { get; set; }
        public string Description { get; set; }
        public string CustomerAddress { get; set; }
        public int ServiceStatusTypeId { get; set; }
        public bool IsAgreed { get; set; }
        public decimal AgreedAmount { get; set; }
        public int PaidByOrderItemId { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }

        public int QuoteDownloadId { get; set; }
        public string CustomName { get; set; }
    }
}

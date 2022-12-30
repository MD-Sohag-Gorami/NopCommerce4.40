using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models
{
    public record ServiceRequestModel : BaseNopEntityModel
    {
        public int ZimZoneServiceId { get; set; }
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.ServiceName")]
        public string ServiceName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomerName")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomerAddress")]
        public string CustomerAddress { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.DownloadGuid")]
        public string DownloadGuid { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.Status")]
        public string Status { get; set; }
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomerEmail")]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomerName")]

        public string ServiceDetailsAttr { get; set; }
        public int ServiceStatusTypeId { get; set; }
        public bool IsAgreed { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.AgreedAmount")]
        public decimal AgreedAmount { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.AgreedAmount")]
        public string AgreedAmountInCustomerCurrency { get; set; }
        public int PaidByOrderItemId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        public bool IsDeleted { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.QuoteDownloadId")]
        [UIHint("Download")]
        public int QuoteDownloadId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomName")]
        public string CustomName { get; set; }
    }
}

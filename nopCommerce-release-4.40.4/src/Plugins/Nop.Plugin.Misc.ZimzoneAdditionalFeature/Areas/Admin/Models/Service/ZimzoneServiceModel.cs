using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models
{
    public record ZimzoneServiceModel : BaseNopEntityModel
    {
        [Required]
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.ZimzoneServiceProductName")]
        public int ServiceProductId { get; set; }
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.ZimzoneServiceProductName")]
        public string ServiceProductName { get; set; }
        [Required]
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.ZimzoneServicePaymentProductName")]
        public int ServicePaymentProductId { get; set; }
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.ZimzoneServicePaymentProductName")]
        public string ServicePaymentProductName { get; set; }
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.IsActive")]
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}

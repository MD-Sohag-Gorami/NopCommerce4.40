using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus
{
    public record CustomOrderStatusModel : BaseNopEntityModel
    {
        public CustomOrderStatusModel()
        {
            OrderStatusList = new List<SelectListItem>();
            Errors = new List<string>();
        }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.CustomOrderStatusName")]
        [Required]
        public string CustomOrderStatusName { get; set; }
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ParentOrderStatusId")]
        public int ParentOrderStatusId { get; set; }
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ParentOrderStatus")]
        public string ParentOrderStatus { get; set; }
        public IList<SelectListItem> OrderStatusList { get; set; }
        public IList<string> Errors { get; set; }
    }
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus
{
    public record CustomStatusListWithOrderModel : BaseNopEntityModel
    {
        public CustomStatusListWithOrderModel()
        {
            CustomOrderStatus = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.CustomOrderStatusId")]
        public int CustomOrderStatusId { get; set; }
        public int ParentOrderStatusId { get; set; }
        public int OrderId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.IsMarkedToSendEmail")]
        public bool IsMarkedToSendEmail { get; set; }
        public IList<SelectListItem> CustomOrderStatus { get; set; }
    }
}

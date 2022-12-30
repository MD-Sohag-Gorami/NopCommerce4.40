using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus
{
    public record CustomOrderStatusSearchModel : BaseSearchModel
    {
        public CustomOrderStatusSearchModel()
        {
            OrderStatusList = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.CustomOrderStatusName")]
        public string CustomOrderStatusName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ParentOrderStatusId")]
        public int ParentOrderStatusId { get; set; }

        public IList<SelectListItem> OrderStatusList { get; set; }
    }
}

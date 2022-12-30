using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus
{
    public record OrderWithCustomStatusSearchModel : OrderSearchModel
    {
        public OrderWithCustomStatusSearchModel()
        {
            CustomOrderStatusList = new List<SelectListItem>();
            CustomOrderStatusIds = new List<int>();
        }
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.CustomOrderStatusId")]
        public IList<int> CustomOrderStatusIds { get; set; }
        public IList<SelectListItem> CustomOrderStatusList { get; set; }
    }
}

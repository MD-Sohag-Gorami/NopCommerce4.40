using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Product
{
    public record ZimzoneProductSearchModel : ProductSearchModel
    {
        public ZimzoneProductSearchModel()
        {
            AvailableStockOptions = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.SearchStockOptionId")]
        public int SearchStockOptionId { get; set; }
        public IList<SelectListItem> AvailableStockOptions { get; set; }
    }
}

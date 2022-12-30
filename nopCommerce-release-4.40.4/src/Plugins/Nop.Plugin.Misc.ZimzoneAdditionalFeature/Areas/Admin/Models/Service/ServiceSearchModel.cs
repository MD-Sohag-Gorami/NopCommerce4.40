using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models
{
    public partial record ServiceSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.SearchServiceName")]
        public string SearchServiceName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.SearchIsActive")]
        public bool SearchIsActive { get; set; }
    }
}

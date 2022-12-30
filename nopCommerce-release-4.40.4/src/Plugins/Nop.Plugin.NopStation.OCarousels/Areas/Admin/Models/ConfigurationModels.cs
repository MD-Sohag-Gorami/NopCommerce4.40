using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.NopStation.OCarousels.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.OCarousels.Configuration.Fields.EnableOCarousel")]
        public bool EnableOCarousel { get; set; }
        public bool EnableOCarousel_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
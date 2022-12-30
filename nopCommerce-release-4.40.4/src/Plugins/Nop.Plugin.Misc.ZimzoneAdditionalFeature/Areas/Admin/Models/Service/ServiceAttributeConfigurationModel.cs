using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Service
{
    public record ServiceAttributeConfigurationModel : BaseNopModel
    {
        public ServiceAttributeConfigurationModel()
        {
            AvailableAttributes = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailableAttributes { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Name")]
        public int ServiceNameAttributeId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Email")]
        public int ServiceEmailAttributeId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Address")]
        public int ServiceAddressAttributeId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Description")]
        public int ServiceDescriptionAttributeId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.File")]
        public int ServiceFileAttributeId { get; set; }
    }
}

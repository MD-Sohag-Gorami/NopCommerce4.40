using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models
{
    public record ServiceRequestSearchModel : BaseSearchModel
    {
        public ServiceRequestSearchModel()
        {
            AvailableServices = new List<SelectListItem>();
            AvailableStatus = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.CustomerEmail")]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.CustomerName")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.ServiceId")]
        public int ServiceId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.StatusId")]
        public int StatusId { get; set; }

        public IList<SelectListItem> AvailableStatus { get; set; }
        public IList<SelectListItem> AvailableServices { get; set; }
    }
}

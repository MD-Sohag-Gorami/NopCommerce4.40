using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Query
{
    public record QuestionSearchModel : BaseSearchModel
    {
        public QuestionSearchModel()
        {
            AvailableStatus = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Search.CustomerEmail")]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Search.CustomerName")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Search.StatusId")]
        public int StatusId { get; set; }

        public IList<SelectListItem> AvailableStatus { get; set; }
    }
}

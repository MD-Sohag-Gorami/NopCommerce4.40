using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models
{
    public record CustomGiftCardModel : BaseNopModel
    {
        public ProductDetailsModel ProductDetailsModel { get; set; }
        public ViewDataDictionary ViewDataDictionary { get; set; }

    }
}

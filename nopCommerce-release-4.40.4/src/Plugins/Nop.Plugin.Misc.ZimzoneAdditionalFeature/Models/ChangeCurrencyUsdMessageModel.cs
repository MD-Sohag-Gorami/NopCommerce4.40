using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models
{
    public record ChangeCurrencyUsdMessageModel : BaseNopModel
    {
        public int ProductId { get; set; }
        public int DestinationCurrencyId { get; set; }
    }
}

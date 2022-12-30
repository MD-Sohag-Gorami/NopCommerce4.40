using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models
{
    public record CustomPriceButtonsModel : BaseNopModel
    {
        public CustomPriceButtonsModel()
        {
            AvailableAmounts = new List<int>();
        }
        public int ProductId { get; set; }
        public IList<int> AvailableAmounts { get; set; }
    }
}

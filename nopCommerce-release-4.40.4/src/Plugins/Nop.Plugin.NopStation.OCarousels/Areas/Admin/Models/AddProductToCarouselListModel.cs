using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.NopStation.OCarousels.Areas.Admin.Models
{
    /// <summary>
    /// Represents a product list model to add to the discount
    /// </summary>
    public partial record AddProductToCarouselListModel : BasePagedListModel<ProductModel>
    {
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public interface ICustomOrderStatusModelFactory
    {
        Task<CustomOrderStatusListModel> PrepareCustomOrderStatusListModelAsync(CustomOrderStatusSearchModel searchModel);
        CustomOrderStatusSearchModel PrepareCustomOrderStatusSearchModel(CustomOrderStatusSearchModel searchModel);
        CustomOrderStatusModel PrepareCustomOrderStatusModel(CustomOrderStatusModel model);
        Task<CustomStatusListWithOrderModel> PrepareCustomOrderStatusListAsync(CustomStatusListWithOrderModel model);
        Task<OrderWithCustomStatusListModel> PrepareOrderWithCustomStatusListModelAsync(OrderWithCustomStatusSearchModel searchModel);
        Task<OrderWithCustomStatusSearchModel> PrepareOrderWithCustomStatusSearchModelAsync(OrderWithCustomStatusSearchModel searchModel);
        Task PrepareCustomOrderStatusAsync(IList<SelectListItem> items);
    }
}
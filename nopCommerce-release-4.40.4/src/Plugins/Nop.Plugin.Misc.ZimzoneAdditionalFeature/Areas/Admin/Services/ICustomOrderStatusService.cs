using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services
{
    public interface ICustomOrderStatusService
    {
        Task<IPagedList<CustomOrderStatus>> GetAllPagedCustomOrderStatusAsync(string customOrderStatusName = null, int parentOrderStatusId = 0);
        Task DeleteCustomOrderStatusAsync(List<int> ids);
        Task DeleteCustomOrderStatusAsync(int id);
        Task<CustomOrderStatus> GetCustomOrderStatusByIdAsync(int id);
        Task UpdateCustomOrderStatusAsync(CustomOrderStatusModel model);
        Task InsertCustomOrderStatusAsync(CustomOrderStatusModel model);
        Task<bool> ValidateNameAndParentStatus(CustomOrderStatusModel model);
        Task<List<CustomOrderStatus>> GetCustomOrderStatusByParentOrderStatusIdAsync(int parentOrderStatusId);
        Task<CustomStatusListWithOrderModel> InsertOrderWithCustomStatusAsync(CustomStatusListWithOrderModel model);
        Task<OrderWithCustomStatus> GetOrderWithCustomStatusAsync(int id = 0, int parentOrderStatusId = 0, int orderId = 0);
        Task UpdateOrderWithCustomStatusAsync(CustomStatusListWithOrderModel model);
        Task<bool> DeleteOrderWithCustomStatusAsync(int id = 0, OrderWithCustomStatus orderWithCustomStatus = null);
        Task<List<OrderWithCustomStatus>> GetAllOrderWithCustomStatusAsync(int customOrderStatusId = 0, int parentOrderStatusId = 0);
        Task<IPagedList<OrderCustom>> SearchOrderWithCustomStatusAsync(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> cosIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

        Task<List<CustomOrderStatus>> GetAllCustomOrderStatusAsync();
    }
}
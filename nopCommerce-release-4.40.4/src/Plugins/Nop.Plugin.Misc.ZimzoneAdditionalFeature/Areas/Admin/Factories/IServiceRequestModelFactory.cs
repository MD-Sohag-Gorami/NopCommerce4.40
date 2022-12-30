using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public interface IServiceRequestModelFactory
    {
        Task<ServiceRequestListModel> PrepareRequestListModelAsync(ServiceRequestSearchModel searchModel);
        Task<ServiceRequestSearchModel> PrepareRequestSearchModelAsync();
        Task<ServiceRequestModel> PrepareServiceRequestModelAsync(ServiceRequestModel model, ZimzoneServiceRequestEntity request);
        Task<List<ServiceRequestModel>> PrepareServiceRequestModelListAsync(List<ServiceRequestModel> requests, Customer customer, Currency currentCurrency);
    }
}
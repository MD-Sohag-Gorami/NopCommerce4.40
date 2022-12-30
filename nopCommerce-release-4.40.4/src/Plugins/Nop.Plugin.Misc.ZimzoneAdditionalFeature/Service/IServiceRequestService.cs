using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public interface IServiceRequestService
    {
        Task<IList<ZimzoneServiceRequestEntity>> GetAllRequestsAsync(int customerId = 0);
        Task<IList<ZimzoneServiceRequestEntity>> GetAllRequestsAsync(Customer customer);
        Task<IList<ZimzoneServiceRequestEntity>> GetAllRequestsAsync(ServiceRequestSearchModel searchModel);
        Task<ZimzoneServiceRequestEntity> GetRequestByIdAsync(int id);
        Task InsertRequestAsync(ZimzoneServiceRequestEntity request);
        Task UpdateRequestAsync(ZimzoneServiceRequestEntity request);
        Task DeleteRequestAsync(ZimzoneServiceRequestEntity request);

        Task<bool> AddToCartAsync(ZimzoneServiceRequestEntity request, Customer customer);

        Task<(string name, string email, string address, string description, string downloadId)> ParseServiceRequestAttributeAsync(string attributeXml, Product product, string description = "");
        Task<(string name, string email, string address, string description, string downloadId)> ParseServiceRequestAttributeAsync(ZimzoneServiceRequestEntity request);

    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services
{
    public interface IZimzoneServiceEntityService
    {
        Task<IPagedList<ZimzoneServiceEntity>> GetAllInfoAsync(ServiceSearchModel searchModel);
        Task<IList<ZimzoneServiceEntity>> GetAllZimzoneServiceAsync();
        Task<IList<int>> GetAllPaymentProductIdAsync(bool showHidden = false);
        Task<IList<int>> GetAllServiceProductIdAsync(bool showHidden = false);
        Task<ZimzoneServiceEntity> GetById(int id);
        Task CreateAsync(ZimzoneServiceEntity zimzoneService);
        Task UpdateAsync(ZimzoneServiceEntity zimzoneService);
        Task DeleteAsync(ZimzoneServiceEntity zimzoneService);
        Task DeleteAsync(IList<int> ids);
    }
}
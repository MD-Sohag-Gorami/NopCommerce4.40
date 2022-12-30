using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpSync.Domains;

namespace Nop.Plugin.Misc.ErpSync.Services
{
    public interface IErpCategoryService
    {
        Task InsertErpCategoryAsync(ErpCategory erpCategory);
        Task UpdateErpCategoryAsync(ErpCategory erpCategory);
        Task DeleteErpCategoryAsync(ErpCategory erpCategory);
        Task DeleteErpCategoriesAsync(IList<string> erpIds);
        Task<ErpCategory> GetErpCategoryByErpIdAsync(string erpId);
        Task<ErpCategory> GetErpCategoryByIdAsync(int id);
        Task<IList<ErpCategory>> GetAllErpCategoriesAsync();
        Task<IList<string>> GetErpCategoryIdsByNopCategoryIdsAsync(IList<int> nopCategoryIds);
        Task<IList<int>> GetNopCategoryIdsByErpCategoryIdsAsync(IList<string> erpCategoryIds);
    }
}
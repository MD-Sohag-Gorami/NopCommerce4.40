using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Misc.ErpSync.Domains;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.ErpSync.Services
{
    public class ErpCategoryService : IErpCategoryService
    {
        private readonly IRepository<ErpCategory> _repository;
        private readonly ICategoryService _categoryService;

        public ErpCategoryService(IRepository<ErpCategory> repository,
            ICategoryService categoryService)
        {
            _repository = repository;
            _categoryService = categoryService;
        }

        public async Task DeleteErpCategoriesAsync(IList<string> erpIds)
        {
            foreach (var erpId in erpIds)
            {
                var erpCategory = await GetErpCategoryByErpIdAsync(erpId);
                if (erpCategory != null)
                {
                    await DeleteErpCategoryAsync(erpCategory);
                }
            }
        }

        public async Task DeleteErpCategoryAsync(ErpCategory erpCategory)
        {
            var nopCategory = await _categoryService.GetCategoryByIdAsync(erpCategory?.NopCategoryId ?? 0);
            var productCategories = await _categoryService.GetProductCategoriesByCategoryIdAsync(nopCategory?.Id ?? 0);
            if (!productCategories.Any())
            {
                if (nopCategory != null)
                {
                    await _categoryService.DeleteCategoryAsync(nopCategory);
                }
                await _repository.DeleteAsync(erpCategory);
            }

        }

        public async Task<IList<ErpCategory>> GetAllErpCategoriesAsync()
        {
            var query = _repository.Table;
            var allCategories = query.ToList();
            return await Task.FromResult(allCategories);
        }

        public async Task<ErpCategory> GetErpCategoryByErpIdAsync(string erpId)
        {
            var query = _repository.Table;
            query = query.Where(x => x.ErpId == erpId);
            var erpCategory = query.FirstOrDefault();
            return await Task.FromResult(erpCategory);
        }

        public async Task<ErpCategory> GetErpCategoryByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IList<string>> GetErpCategoryIdsByNopCategoryIdsAsync(IList<int> nopCategoryIds)
        {
            if (nopCategoryIds == null || nopCategoryIds.Count == 0)
            {
                return new List<string>();
            }
            var query = _repository.Table;
            query = query.Where(x => nopCategoryIds.Contains(x.NopCategoryId));
            var erpCategoryIds = query.Select(x => x.ErpId).ToList();
            return await Task.FromResult(erpCategoryIds);
        }

        public async Task<IList<int>> GetNopCategoryIdsByErpCategoryIdsAsync(IList<string> erpCategoryIds)
        {
            if (erpCategoryIds == null || erpCategoryIds.Count == 0)
            {
                return new List<int>();
            }
            var query = _repository.Table;
            query = query.Where(x => erpCategoryIds.Contains(x.ErpId));
            var nopCategoryIds = query.Select(x => x.NopCategoryId).ToList();
            return await Task.FromResult(nopCategoryIds);

        }

        public async Task InsertErpCategoryAsync(ErpCategory erpCategory)
        {
            await _repository.InsertAsync(erpCategory);
        }

        public async Task UpdateErpCategoryAsync(ErpCategory erpCategory)
        {
            await _repository.UpdateAsync(erpCategory);
        }
    }
}

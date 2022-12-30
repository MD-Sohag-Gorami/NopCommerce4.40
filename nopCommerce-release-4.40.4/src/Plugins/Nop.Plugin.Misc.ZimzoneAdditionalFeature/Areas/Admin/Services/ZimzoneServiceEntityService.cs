using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services
{
    public class ZimzoneServiceEntityService : IZimzoneServiceEntityService
    {
        private readonly IRepository<ZimzoneServiceEntity> _repository;
        private readonly IProductService _productService;
        private readonly ServiceSettings _serviceSettings;

        public ZimzoneServiceEntityService(IRepository<ZimzoneServiceEntity> repository,
            IProductService productService,
            ServiceSettings serviceSettings)
        {
            _repository = repository;
            _productService = productService;
            _serviceSettings = serviceSettings;
        }

        public virtual async Task UpdateAsync(ZimzoneServiceEntity zimzoneService)
        {
            await _repository.UpdateAsync(zimzoneService);
        }
        public async Task CreateAsync(ZimzoneServiceEntity zimzoneService)
        {

            await _repository.InsertAsync(zimzoneService);
            var serviceProduct = await _productService.GetProductByIdAsync(zimzoneService.ServiceProductId);
            var paymentProduct = await _productService.GetProductByIdAsync(zimzoneService.ServicePaymentProductId);
            if (serviceProduct != null)
            {
                serviceProduct.ProductTemplateId = _serviceSettings.ProductTemplateId;
                await _productService.UpdateProductAsync(serviceProduct);
            }
            if (paymentProduct != null)
            {
                paymentProduct.CustomerEntersPrice = true;
                paymentProduct.VisibleIndividually = false;
                await _productService.UpdateProductAsync(paymentProduct);
            }
        }

        public async Task<IPagedList<ZimzoneServiceEntity>> GetAllInfoAsync(ServiceSearchModel searchModel)
        {
            var query = _repository.Table.Where(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(searchModel.SearchServiceName))
            {
                query = query.Where(x => x.ServiceProductName == searchModel.SearchServiceName);

            }
            if (searchModel.SearchIsActive)
            {
                query = query.Where(x => x.IsActive);
            }

            var serviceEntities = await query.ToListAsync();
            var pageList = new PagedList<ZimzoneServiceEntity>(serviceEntities, 0, searchModel.PageSize);

            return pageList;
        }

        public async Task<ZimzoneServiceEntity> GetById(int id)
        {
            if (id == 0)
            {
                return null;
            }
            return await _repository.GetByIdAsync(id);
        }

        public async Task DeleteAsync(ZimzoneServiceEntity zimzoneService)
        {
            await _repository.DeleteAsync(zimzoneService);
        }

        public async Task DeleteAsync(IList<int> ids)
        {
            foreach (var id in ids)
            {
                var zimzoneService = await GetById(id);
                if (zimzoneService != null)
                {
                    await DeleteAsync(zimzoneService);
                }
            }
        }

        public async Task<IList<ZimzoneServiceEntity>> GetAllZimzoneServiceAsync()
        {
            var query = _repository.Table;
            var services = query.ToList();
            return await Task.FromResult(services);
        }

        public async Task<IList<int>> GetAllPaymentProductIdAsync(bool showHidden = false)
        {
            var query = _repository.Table;
            if (!showHidden)
            {
                query = query.Where(x => x.IsActive && !x.IsDeleted);
            }
            var paymentProductIds = query.Select(x => x.ServicePaymentProductId).ToList();
            return await Task.FromResult(paymentProductIds);
        }

        public async Task<IList<int>> GetAllServiceProductIdAsync(bool showHidden = false)
        {
            var query = _repository.Table;
            if (!showHidden)
            {
                query = query.Where(x => x.IsActive && !x.IsDeleted);
            }
            var serviceProductIds = query.Select(x => x.ServiceProductId).ToList();
            return await Task.FromResult(serviceProductIds);
        }
    }
}

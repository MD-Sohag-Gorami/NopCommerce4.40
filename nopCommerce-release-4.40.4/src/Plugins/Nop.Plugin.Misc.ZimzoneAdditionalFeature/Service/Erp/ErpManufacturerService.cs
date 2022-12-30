using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Erp;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.Erp
{
    public class ErpManufacturerService : IErpManufacturerService
    {
        private readonly IRepository<ErpManufacturer> _erpManufacturerRepository;
        private readonly IManufacturerService _manufacturerService;

        public ErpManufacturerService(IRepository<ErpManufacturer> erpManufacturerRepository,
                                      IManufacturerService manufacturerService)
        {
            _erpManufacturerRepository = erpManufacturerRepository;
            _manufacturerService = manufacturerService;
        }

        public async Task<IList<ErpManufacturer>> GetAllErpManufacturersAsync()
        {
            var query = _erpManufacturerRepository.Table;
            return await query.ToListAsync();
        }
        public async Task InsertErpManufacturerAsync(ErpManufacturer erpManufacturer)
        {
            await _erpManufacturerRepository.InsertAsync(erpManufacturer);
        }

        public async Task UpdateErpManufacturerAsync(ErpManufacturer erpManufacturer)
        {
            await _erpManufacturerRepository.UpdateAsync(erpManufacturer);
        }

        public async Task DeleteErpManufacturerAsync(ErpManufacturer erpManufacturer)
        {
            var nopManufacturer = await _manufacturerService.GetManufacturerByIdAsync(erpManufacturer?.NopManufacturerId ?? 0);
            var productManufacturers = await _manufacturerService.GetProductManufacturersByManufacturerIdAsync(nopManufacturer?.Id ?? 0);
            if (!productManufacturers.Any())
            {
                if (nopManufacturer != null)
                {
                    await _manufacturerService.DeleteManufacturerAsync(nopManufacturer);
                }
                await _erpManufacturerRepository.DeleteAsync(erpManufacturer);
            }
        }

        public async Task<ErpManufacturer> GetErpManufacturerByIdAsync(int id)
        {
            return await _erpManufacturerRepository.GetByIdAsync(id);
        }

        public async Task<ErpManufacturer> GetErpManufacturerByNopManufacturerIdAsync(int nopManufacturerId)
        {
            var query = _erpManufacturerRepository.Table;
            query = query.Where(x => x.NopManufacturerId == nopManufacturerId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ErpManufacturer> GetErpManufacturerByErpManufacturerIdAsync(string erpManufacturerId)
        {
            var query = _erpManufacturerRepository.Table;
            query = query.Where(x => x.ErpManufacturerId == erpManufacturerId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Manufacturer> GetNopManufacturerByErpManufacturerIdAsync(string erpManufacturerId)
        {
            var erpManufacturer = await GetErpManufacturerByErpManufacturerIdAsync(erpManufacturerId);
            return await _manufacturerService.GetManufacturerByIdAsync(erpManufacturer.NopManufacturerId);
        }

        public async Task DeleteErpManufacturersAsync(List<string> removeIds)
        {
            foreach (var erpId in removeIds)
            {
                var erpManufacturer = await GetErpManufacturerByErpManufacturerIdAsync(erpId);
                if (erpManufacturer != null)
                {
                    await DeleteErpManufacturerAsync(erpManufacturer);
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Misc.ErpSync.Domains;

namespace Nop.Plugin.Misc.ErpSync.Services
{
    public class ErpPictureService : IErpPictureService
    {
        private readonly IRepository<ErpProductPicture> _repository;

        public ErpPictureService(IRepository<ErpProductPicture> repository)
        {
            _repository = repository;
        }
        public async Task DeleteErpProductPictureAsync(ErpProductPicture erpProductPicture)
        {
            await _repository.DeleteAsync(erpProductPicture);
        }

        public async Task<ErpProductPicture> GetErpPictureByIdAsync(int id)
        {
            var erpProductPicture = await _repository.GetByIdAsync(id);
            return erpProductPicture;
        }

        public async Task<IList<ErpProductPicture>> GetErpPicturesByNopProductIdAsync(int nopProductId)
        {
            var query = _repository.Table;
            query = query.Where(x => x.NopProductId == nopProductId);
            var erpProductPictures = await query.ToListAsync();
            return erpProductPictures;
        }

        public async Task<IList<ErpProductPicture>> GetErpPicturesBySkuAsync(string sku)
        {
            var query = _repository.Table;
            query = query.Where(x => x.Sku == sku);
            var pictures = await query.ToListAsync();
            return pictures;
        }

        public async Task InsertErpProductPictureAsync(ErpProductPicture erpProductPicture)
        {
            await _repository.InsertAsync(erpProductPicture);
        }

        public async Task UpdateErpProductPictureAsync(ErpProductPicture erpProductPicture)
        {
            await _repository.UpdateAsync(erpProductPicture);
        }
    }
}

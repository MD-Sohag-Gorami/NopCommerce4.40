using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpSync.Domains;

namespace Nop.Plugin.Misc.ErpSync.Services
{
    public interface IErpPictureService
    {
        Task InsertErpProductPictureAsync(ErpProductPicture erpProductPicture);
        Task UpdateErpProductPictureAsync(ErpProductPicture erpProductPicture);
        Task DeleteErpProductPictureAsync(ErpProductPicture erpProductPicture);
        Task<IList<ErpProductPicture>> GetErpPicturesByNopProductIdAsync(int nopProductId);
        Task<IList<ErpProductPicture>> GetErpPicturesBySkuAsync(string sku);
        Task<ErpProductPicture> GetErpPictureByIdAsync(int id);
    }
}
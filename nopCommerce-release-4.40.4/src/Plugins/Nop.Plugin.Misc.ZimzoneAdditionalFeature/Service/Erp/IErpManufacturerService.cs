using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Erp;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.Erp
{
    public interface IErpManufacturerService
    {
        Task DeleteErpManufacturerAsync(ErpManufacturer erpManufacturer);
        Task DeleteErpManufacturersAsync(List<string> removeIds);
        Task<IList<ErpManufacturer>> GetAllErpManufacturersAsync();
        Task<ErpManufacturer> GetErpManufacturerByErpManufacturerIdAsync(string erpManufacturerId);
        Task<ErpManufacturer> GetErpManufacturerByIdAsync(int id);
        Task<ErpManufacturer> GetErpManufacturerByNopManufacturerIdAsync(int nopManufacturerId);
        Task<Manufacturer> GetNopManufacturerByErpManufacturerIdAsync(string erpManufacturerId);
        Task InsertErpManufacturerAsync(ErpManufacturer erpManufacturer);
        Task UpdateErpManufacturerAsync(ErpManufacturer erpManufacturer);
    }
}
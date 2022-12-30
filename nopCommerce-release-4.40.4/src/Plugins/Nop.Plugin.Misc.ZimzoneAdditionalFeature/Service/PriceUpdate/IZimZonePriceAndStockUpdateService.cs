using System.IO;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.PriceUpdate
{
    public interface IZimZonePriceAndStockUpdateService
    {
        Task ImportPriceAndStockInformationsFromXlsxAsync(Stream stream);
        Task DeleteImportPriceAndStockErrorLogAsync(int vendorId = 0);
        Task<int> ImportPriceAndStockErrorLogCountByVendorIdAsync(int vendorId);
        Task<int> WriteStreamInDatabaseAsync(Stream stream, string destinationTableName, bool hasHeader = true);
    }
}
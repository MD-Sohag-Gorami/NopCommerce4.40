using System.IO;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services
{
    public interface IOrderItemPdfDownloadService
    {
        Task OrderItemsToPdfAsync(Stream stream, Order order, int languageId = 0);
        Task ShipmentItemsToPdfAsync(Stream stream, Shipment shipment, int languageId = 0);
    }
}
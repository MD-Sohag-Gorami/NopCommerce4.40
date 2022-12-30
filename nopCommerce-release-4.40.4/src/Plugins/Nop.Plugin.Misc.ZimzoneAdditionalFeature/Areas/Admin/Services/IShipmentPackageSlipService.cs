using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using iTextSharp.text;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services
{
    public interface IShipmentPackageSlipService
    {
        Task PrintPackagingSlipsToPdfAsync(Stream stream, IList<Shipment> shipments, Rectangle pageSize, int fontSize, int languageId = 0);
    }
}
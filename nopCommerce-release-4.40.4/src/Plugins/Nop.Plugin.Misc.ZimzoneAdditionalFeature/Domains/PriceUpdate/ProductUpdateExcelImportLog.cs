using Nop.Core;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.PriceUpdate
{
    public class ProductUpdateExcelImportLog : BaseEntity
    {
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int VendorId { get; set; }
        public string ErrorMessage { get; set; }
    }
}

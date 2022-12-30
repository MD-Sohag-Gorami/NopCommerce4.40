using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models
{
    public record GiftCardDeliveryDateModel : BaseNopModel
    {
        public int ProductId { get; set; }

        public string ProductSku { get; set; }

        public string DeliveryDateMessage { get; set; }

        public bool IsZimazonGiftCard { get; set; }

        public bool IsElectrosalesCreditVoucher { get; set; }

        public string ValidUpto { get; set; }
    }
}

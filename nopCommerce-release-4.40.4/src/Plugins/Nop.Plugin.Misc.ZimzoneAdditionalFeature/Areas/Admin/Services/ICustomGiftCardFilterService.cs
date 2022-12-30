using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services
{
    public interface ICustomGiftCardFilterService
    {
        Task<IPagedList<GiftCard>> GetAllZimzoneGiftCardsAsync(int? purchasedWithOrderId = null, int? usedWithOrderId = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            bool? isGiftCardActivated = null, string giftCardCouponCode = null,
            string recipientName = null,
            int pageIndex = 0, int pageSize = int.MaxValue);
        Task<IPagedList<GiftCard>> GetAllElectrosalesVouchersAsync(int? purchasedWithOrderId = null, int? usedWithOrderId = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            bool? isGiftCardActivated = null, string giftCardCouponCode = null,
            string recipientName = null,
            int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
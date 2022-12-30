using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services
{
    public class CustomGiftCardFilterService : ICustomGiftCardFilterService
    {
        private readonly IRepository<GiftCard> _giftCardRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<GiftCardUsageHistory> _giftCardUsageHistoryRepository;
        private readonly GiftVoucherSettings _giftVoucherSettings;

        public CustomGiftCardFilterService(IRepository<GiftCard> giftCardRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Product> productRepository,
            IRepository<GiftCardUsageHistory> giftCardUsageHistoryRepository,
            GiftVoucherSettings giftVoucherSettings)
        {
            _giftCardRepository = giftCardRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _giftCardUsageHistoryRepository = giftCardUsageHistoryRepository;
            _giftVoucherSettings = giftVoucherSettings;
        }
        public async Task<IPagedList<GiftCard>> GetAllElectrosalesVouchersAsync(int? purchasedWithOrderId = null, int? usedWithOrderId = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, bool? isGiftCardActivated = null, string giftCardCouponCode = null, string recipientName = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var giftCards = await _giftCardRepository.GetAllPagedAsync(query =>
            {
                if (purchasedWithOrderId.HasValue)
                {
                    query = from gc in query
                            join oi in _orderItemRepository.Table on gc.PurchasedWithOrderItemId equals oi.Id
                            where oi.OrderId == purchasedWithOrderId.Value
                            select gc;
                }

                if (usedWithOrderId.HasValue)
                    query = from gc in query
                            join gcuh in _giftCardUsageHistoryRepository.Table on gc.Id equals gcuh.GiftCardId
                            where gcuh.UsedWithOrderId == usedWithOrderId
                            select gc;

                if (createdFromUtc.HasValue)
                    query = query.Where(gc => createdFromUtc.Value <= gc.CreatedOnUtc);
                if (createdToUtc.HasValue)
                    query = query.Where(gc => createdToUtc.Value >= gc.CreatedOnUtc);
                if (isGiftCardActivated.HasValue)
                    query = query.Where(gc => gc.IsGiftCardActivated == isGiftCardActivated.Value);
                if (!string.IsNullOrEmpty(giftCardCouponCode))
                    query = query.Where(gc => gc.GiftCardCouponCode == giftCardCouponCode);
                if (!string.IsNullOrWhiteSpace(recipientName))
                    query = query.Where(c => c.RecipientName.Contains(recipientName));

                query = from gc in query
                        join oi in _orderItemRepository.Table on gc.PurchasedWithOrderItemId equals oi.Id
                        join p in _productRepository.Table on oi.ProductId equals p.Id
                        where p.Sku == _giftVoucherSettings.ElectrosalesGiftProductSku
                        select gc;

                query = query.OrderByDescending(gc => gc.CreatedOnUtc);

                return query;
            }, pageIndex, pageSize);

            return giftCards;
        }

        public async Task<IPagedList<GiftCard>> GetAllZimzoneGiftCardsAsync(int? purchasedWithOrderId = null, int? usedWithOrderId = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, bool? isGiftCardActivated = null, string giftCardCouponCode = null, string recipientName = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var giftCards = await _giftCardRepository.GetAllPagedAsync(query =>
            {
                if (purchasedWithOrderId.HasValue)
                {
                    query = from gc in query
                            join oi in _orderItemRepository.Table on gc.PurchasedWithOrderItemId equals oi.Id
                            where oi.OrderId == purchasedWithOrderId.Value
                            select gc;
                }

                if (usedWithOrderId.HasValue)
                    query = from gc in query
                            join gcuh in _giftCardUsageHistoryRepository.Table on gc.Id equals gcuh.GiftCardId
                            where gcuh.UsedWithOrderId == usedWithOrderId
                            select gc;

                if (createdFromUtc.HasValue)
                    query = query.Where(gc => createdFromUtc.Value <= gc.CreatedOnUtc);
                if (createdToUtc.HasValue)
                    query = query.Where(gc => createdToUtc.Value >= gc.CreatedOnUtc);
                if (isGiftCardActivated.HasValue)
                    query = query.Where(gc => gc.IsGiftCardActivated == isGiftCardActivated.Value);
                if (!string.IsNullOrEmpty(giftCardCouponCode))
                    query = query.Where(gc => gc.GiftCardCouponCode == giftCardCouponCode);
                if (!string.IsNullOrWhiteSpace(recipientName))
                    query = query.Where(c => c.RecipientName.Contains(recipientName));

                query = from gc in query
                        join oi in _orderItemRepository.Table on gc.PurchasedWithOrderItemId equals oi.Id
                        join p in _productRepository.Table on oi.ProductId equals p.Id
                        where p.Sku != _giftVoucherSettings.ElectrosalesGiftProductSku
                        select gc;

                var query1 = _giftCardRepository.Table.Where(x => x.PurchasedWithOrderItemId == null);

                query = query.Union(query1);

                query = query.OrderByDescending(gc => gc.CreatedOnUtc);

                return query;
            }, pageIndex, pageSize);

            return giftCards;
        }
    }
}

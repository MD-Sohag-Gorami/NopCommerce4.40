using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public class OverridenGiftCardService : GiftCardService
    {
        private readonly IWorkContext _workContext;
        private readonly GiftVoucherSettings _giftVoucherSettings;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public OverridenGiftCardService(ICustomerService customerService,
            IEventPublisher eventPublisher,
            IRepository<GiftCard> giftCardRepository,
            IRepository<GiftCardUsageHistory> giftCardUsageHistoryRepository,
            IRepository<OrderItem> orderItemRepository,
            IWorkContext workContext,
            GiftVoucherSettings giftVoucherSettings,
            IOrderService orderService,
            IProductService productService) : base(customerService,
                eventPublisher,
                giftCardRepository,
                giftCardUsageHistoryRepository,
                orderItemRepository)
        {
            _workContext = workContext;
            _giftVoucherSettings = giftVoucherSettings;
            _orderService = orderService;
            _productService = productService;
        }
        public override async Task<bool> IsGiftCardValidAsync(GiftCard giftCard)
        {
            if (giftCard == null)
                throw new ArgumentNullException(nameof(giftCard));

            if (!giftCard.IsGiftCardActivated)
                return false;
            var orderItem = await _orderService.GetOrderItemByIdAsync(giftCard.PurchasedWithOrderItemId.HasValue ? giftCard.PurchasedWithOrderItemId.Value : 0);

            if (orderItem != null)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                if (product != null && product.Sku.Equals(_giftVoucherSettings.ElectrosalesGiftProductSku))
                {
                    return false;
                }
            }
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null || !giftCard.RecipientEmail.Equals(customer.Email))
            {
                return false;
            }
            var remainingAmount = await GetGiftCardRemainingAmountAsync(giftCard);
            if (remainingAmount > decimal.Zero)
                return true;

            return false;
        }
    }
}

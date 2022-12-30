using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Factories
{
    public class GiftCardHistoryModelFactory : IGiftCardHistoryModelFactory
    {
        private readonly IGiftCardService _giftCardService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderService _orderService;
        private readonly GiftVoucherSettings _giftVoucherSettings;

        public GiftCardHistoryModelFactory(IGiftCardService giftCardService,
            IWorkContext workContext,
            ICustomerService customerService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IOrderService orderService,
            GiftVoucherSettings giftVoucherSettings)
        {
            _giftCardService = giftCardService;
            _workContext = workContext;
            _customerService = customerService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _orderService = orderService;
            _giftVoucherSettings = giftVoucherSettings;
        }
        public async Task<IList<GiftCardHistoryModel>> PrepareGiftCardHistoryModelsAsync()
        {
            var giftCardHistoryModels = new List<GiftCardHistoryModel>();
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null || !(await _customerService.IsRegisteredAsync(customer)) && !string.IsNullOrEmpty(customer?.Email))
            {
                return giftCardHistoryModels;
            }
            else
            {
                var giftCards = (await _giftCardService.GetAllGiftCardsAsync(isGiftCardActivated: true)).Where(x =>
                {
                    var product = _orderService.GetProductByOrderItemIdAsync(x?.PurchasedWithOrderItemId ?? 0).Result;

                    if (product != null && product.Sku == _giftVoucherSettings.ElectrosalesGiftProductSku)
                    {
                        return false;
                    }
                    return true;
                }).ToList();

                giftCardHistoryModels.AddRange(giftCards.Where(x => x.RecipientEmail == customer.Email).Select(x =>
                {

                    var amount = _currencyService.ConvertFromPrimaryStoreCurrencyAsync(x.Amount, _workContext.GetWorkingCurrencyAsync().Result).Result;
                    var remainingAmount = _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_giftCardService.GetGiftCardRemainingAmountAsync(x).Result, _workContext.GetWorkingCurrencyAsync().Result).Result;

                    var amountString = _priceFormatter.FormatPriceAsync(amount, true, false).Result;
                    var remainingAmountString = _priceFormatter.FormatPriceAsync(remainingAmount, true, false).Result;
                    var historyModel = new GiftCardHistoryModel
                    {
                        Id = x.Id,
                        Coupon = x.GiftCardCouponCode,
                        TotalAmount = amountString,
                        RemainingAmount = remainingAmountString
                    };
                    return historyModel;
                }));
            }

            return await Task.FromResult(giftCardHistoryModels);
        }
    }
}

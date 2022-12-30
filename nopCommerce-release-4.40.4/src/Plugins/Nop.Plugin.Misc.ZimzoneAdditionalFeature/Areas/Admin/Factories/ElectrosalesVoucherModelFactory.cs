using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Voucher;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public class ElectrosalesVoucherModelFactory : IElectrosalesVoucherModelFactory
    {
        private readonly ICustomGiftCardFilterService _customGiftCardFilterService;
        private readonly IGiftCardService _giftCardService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IGiftCardAttributeParser _giftCardAttributeParser;
        private readonly IOrderService _orderService;
        private readonly IGiftCardModelFactory _giftCardModelFactory;

        public ElectrosalesVoucherModelFactory(ICustomGiftCardFilterService customGiftCardFilterService,
            IGiftCardService giftCardService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IGiftCardAttributeParser giftCardAttributeParser,
            IOrderService orderService,
            IGiftCardModelFactory giftCardModelFactory)
        {
            _customGiftCardFilterService = customGiftCardFilterService;
            _giftCardService = giftCardService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _giftCardAttributeParser = giftCardAttributeParser;
            _orderService = orderService;
            _giftCardModelFactory = giftCardModelFactory;
        }
        public async Task<ElectrosalesVoucherListModel> PrepareElectrosalesVoucherListModelAsync(GiftCardSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter gift cards
            var isActivatedOnly = searchModel.ActivatedId == 0 ? null : searchModel.ActivatedId == 1 ? true : (bool?)false;

            //override for loading only zimzone gift card
            //get gift cards
            var giftCards = await _customGiftCardFilterService.GetAllElectrosalesVouchersAsync(isGiftCardActivated: isActivatedOnly,
                giftCardCouponCode: searchModel.CouponCode,
                recipientName: searchModel.RecipientName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new ElectrosalesVoucherListModel().PrepareToGridAsync(searchModel, giftCards, () =>
            {
                return giftCards.SelectAwait(async giftCard =>
                {
                    //fill in model values from the entity
                    var giftCardModel = giftCard.ToModel<GiftCardModel>();

                    //convert dates to the user time
                    giftCardModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(giftCard.CreatedOnUtc, DateTimeKind.Utc);

                    //fill in additional values (not existing in the entity)
                    var giftAmount = await _giftCardService.GetGiftCardRemainingAmountAsync(giftCard);
                    giftCardModel.RemainingAmountStr = await _priceFormatter.FormatPriceAsync(giftAmount, true, false);
                    giftCardModel.AmountStr = await _priceFormatter.FormatPriceAsync(giftCard.Amount, true, false);

                    return new ElectrosalesVoucherModel
                    {
                        Amount = giftCardModel.Amount,
                        AmountStr = giftCardModel.AmountStr,
                        CreatedOn = giftCardModel.CreatedOn,
                        GiftCardCouponCode = giftCardModel.GiftCardCouponCode,
                        GiftCardTypeId = giftCardModel.GiftCardTypeId,
                        GiftCardUsageHistorySearchModel = giftCardModel.GiftCardUsageHistorySearchModel,
                        Id = giftCardModel.Id,
                        IsGiftCardActivated = giftCardModel.IsGiftCardActivated,
                        IsRecipientNotified = giftCardModel.IsRecipientNotified,
                        Message = giftCardModel.Message,
                        SenderEmail = giftCardModel.SenderEmail,
                        PrimaryStoreCurrencyCode = giftCardModel.PrimaryStoreCurrencyCode,
                        PurchasedWithOrderId = giftCardModel.PurchasedWithOrderId,
                        PurchasedWithOrderNumber = giftCardModel.PurchasedWithOrderNumber,
                        RecipientEmail = giftCardModel.RecipientEmail,
                        SenderName = giftCardModel.SenderName,
                        RecipientName = giftCardModel.RecipientName,
                        RemainingAmountStr = giftCardModel.RemainingAmountStr,
                        PhoneNumber = await GetPhoneNumber(giftCard?.PurchasedWithOrderItemId ?? 0)
                    };
                });
            });

            return model;
        }

        public async Task<ElectrosalesVoucherModel> PrepareElectrosalesVoucherModelAsync(GiftCard giftCard)
        {
            var giftCardModel = await _giftCardModelFactory.PrepareGiftCardModelAsync(null, giftCard);
            return new ElectrosalesVoucherModel
            {
                Amount = giftCardModel.Amount,
                AmountStr = giftCardModel.AmountStr,
                CreatedOn = giftCardModel.CreatedOn,
                GiftCardCouponCode = giftCardModel.GiftCardCouponCode,
                GiftCardTypeId = giftCardModel.GiftCardTypeId,
                GiftCardUsageHistorySearchModel = giftCardModel.GiftCardUsageHistorySearchModel,
                Id = giftCardModel.Id,
                IsGiftCardActivated = giftCardModel.IsGiftCardActivated,
                IsRecipientNotified = giftCardModel.IsRecipientNotified,
                Message = giftCardModel.Message,
                SenderEmail = giftCardModel.SenderEmail,
                PrimaryStoreCurrencyCode = giftCardModel.PrimaryStoreCurrencyCode,
                PurchasedWithOrderId = giftCardModel.PurchasedWithOrderId,
                PurchasedWithOrderNumber = giftCardModel.PurchasedWithOrderNumber,
                RecipientEmail = giftCardModel.RecipientEmail,
                SenderName = giftCardModel.SenderName,
                RecipientName = giftCardModel.RecipientName,
                RemainingAmountStr = giftCardModel.RemainingAmountStr,
                PhoneNumber = await GetPhoneNumber(giftCard?.PurchasedWithOrderItemId ?? 0)
            };
        }

        async Task<string> GetPhoneNumber(int orderItemId)
        {
            var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);
            if (orderItem != null)
            {
                _giftCardAttributeParser.GetGiftCardAttribute(orderItem.AttributesXml, out _,
                   out _, out _, out _,
                   out _, out _, out _,
                   out var phoneNumber, out _, out _);
                if (!string.IsNullOrEmpty(phoneNumber))
                    return phoneNumber;
            }
            return string.Empty;
        }
    }

}

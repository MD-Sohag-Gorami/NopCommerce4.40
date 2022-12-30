using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public class OverridenGiftCardModelFactory : GiftCardModelFactory
    {
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGiftCardService _giftCardService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICustomGiftCardFilterService _customGiftCardFilterService;

        public OverridenGiftCardModelFactory(CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPriceFormatter priceFormatter,
            ICustomGiftCardFilterService customGiftCardFilterService) : base(currencySettings,
             currencyService,
             dateTimeHelper,
             giftCardService,
             localizationService,
             orderService,
             priceFormatter)
        {
            _dateTimeHelper = dateTimeHelper;
            _giftCardService = giftCardService;
            _priceFormatter = priceFormatter;
            _customGiftCardFilterService = customGiftCardFilterService;
        }

        public override async Task<GiftCardListModel> PrepareGiftCardListModelAsync(GiftCardSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter gift cards
            var isActivatedOnly = searchModel.ActivatedId == 0 ? null : searchModel.ActivatedId == 1 ? true : (bool?)false;

            //override for loading only zimzone gift card
            //get gift cards
            var giftCards = await _customGiftCardFilterService.GetAllZimzoneGiftCardsAsync(isGiftCardActivated: isActivatedOnly,
                giftCardCouponCode: searchModel.CouponCode,
                recipientName: searchModel.RecipientName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new GiftCardListModel().PrepareToGridAsync(searchModel, giftCards, () =>
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

                    return giftCardModel;
                });
            });

            return model;
        }
    }
}

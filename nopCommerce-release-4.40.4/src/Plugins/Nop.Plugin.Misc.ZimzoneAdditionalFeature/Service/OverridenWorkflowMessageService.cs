using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public class OverridenWorkflowMessageService : WorkflowMessageService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IGiftCardAttributeParser _giftCardAttributeParser;
        private readonly GiftVoucherSettings _giftVoucherSettings;

        public OverridenWorkflowMessageService(CommonSettings commonSettings,
            EmailAccountSettings emailAccountSettings,
            IAddressService addressService, IAffiliateService affiliateService,
            ICustomerService customerService, IEmailAccountService emailAccountService,
            IEventPublisher eventPublisher, ILanguageService languageService,
            ILocalizationService localizationService,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider, IOrderService orderService,
            IProductService productService, IQueuedEmailService queuedEmailService,
            IStoreContext storeContext,
            IStoreService storeService, ITokenizer tokenizer,
            IGiftCardAttributeParser giftCardAttributeParser,
            GiftVoucherSettings giftVoucherSettings) : base(commonSettings, emailAccountSettings, addressService, affiliateService, customerService, emailAccountService, eventPublisher, languageService, localizationService, messageTemplateService, messageTokenProvider, orderService, productService, queuedEmailService, storeContext, storeService, tokenizer)
        {
            _eventPublisher = eventPublisher;
            _messageTokenProvider = messageTokenProvider;
            _orderService = orderService;
            _productService = productService;
            _storeContext = storeContext;
            _storeService = storeService;
            _giftCardAttributeParser = giftCardAttributeParser;
            _giftVoucherSettings = giftVoucherSettings;
        }

        public override async Task<IList<int>> SendGiftCardNotificationAsync(GiftCard giftCard, int languageId)
        {
            if (giftCard == null)
                throw new ArgumentNullException(nameof(giftCard));

            var order = await _orderService.GetOrderByOrderItemAsync(giftCard.PurchasedWithOrderItemId ?? 0);

            var store = order != null ? await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync() : await _storeContext.GetCurrentStoreAsync();

            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var orderItem = await _orderService.GetOrderItemByIdAsync(giftCard?.PurchasedWithOrderItemId ?? 0);
            var isElectrosalesVoucher = false;
            var product = await _productService.GetProductByIdAsync(orderItem?.ProductId ?? 0);
            if (product != null && product.Sku == _giftVoucherSettings.ElectrosalesGiftProductSku)
            {
                isElectrosalesVoucher = true;
            }
            var messageTemplates = await GetActiveMessageTemplatesAsync(isElectrosalesVoucher ? GiftVoucherDefaults.ElectrosalesCreditVoucher_CustomerNotification : MessageTemplateSystemNames.GiftCardNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddGiftCardTokensAsync(commonTokens, giftCard);


            _giftCardAttributeParser.GetGiftCardAttribute(orderItem?.AttributesXml ?? "", out var giftCardFirstName,
                out var giftCardLastName, out var giftCardRecipientEmail, out var giftCardSenderName,
                out var giftCardSenderEmail, out var giftCardMessage, out var giftCardPhysicalAddress,
                out var giftCardCellPhoneNumber, out var giftCardIdOrPassportNumber, out var giftCardDeliveryDate);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = giftCard.RecipientEmail;
                var toName = giftCard.RecipientName;
                var dayDiffer = 0;
                if (giftCardDeliveryDate.HasValue)
                {
                    dayDiffer = (int)(giftCardDeliveryDate.Value - DateTime.UtcNow.Date).TotalDays;
                    var todaysSpentHour = (DateTime.UtcNow - DateTime.UtcNow.Date).TotalHours;
                    if (dayDiffer > 0)
                    {
                        messageTemplate.DelayBeforeSend = (dayDiffer * 24) - (int)todaysSpentHour;
                        messageTemplate.DelayPeriod = MessageDelayPeriod.Hours;
                        messageTemplate.DelayPeriodId = 0;
                    }
                }

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

    }
}

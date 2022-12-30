using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class OverriddenGiftCardController : GiftCardController
    {
        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGiftCardModelFactory _giftCardModelFactory;
        private readonly IGiftCardService _giftCardService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;

        public OverriddenGiftCardController(CurrencySettings currencySettings,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper,
            IGiftCardModelFactory giftCardModelFactory,
            IGiftCardService giftCardService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IOrderService orderService,
            IPermissionService permissionService,
            IPriceFormatter priceFormatter,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings) : base(currencySettings,
                currencyService,
                customerActivityService,
                dateTimeHelper,
                giftCardModelFactory,
                giftCardService,
                languageService,
                localizationService,
                notificationService,
                orderService,
                permissionService,
                priceFormatter,
                workflowMessageService,
                localizationSettings)
        {
            _currencySettings = currencySettings;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _dateTimeHelper = dateTimeHelper;
            _giftCardModelFactory = giftCardModelFactory;
            _giftCardService = giftCardService;
            _languageService = languageService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _priceFormatter = priceFormatter;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
        }

        public async override Task<IActionResult> NotifyRecipient(GiftCardModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            //try to get a gift card with the specified id
            var giftCard = await _giftCardService.GetGiftCardByIdAsync(model.Id);
            if (giftCard == null)
                return RedirectToAction("List");

            try
            {
                if (!CommonHelper.IsValidEmail(giftCard.RecipientEmail))
                    throw new NopException("Recipient email is not valid");

                //if (!CommonHelper.IsValidEmail(giftCard.SenderEmail))
                //    throw new NopException("Sender email is not valid");

                var languageId = 0;
                var order = await _orderService.GetOrderByOrderItemAsync(giftCard.PurchasedWithOrderItemId ?? 0);

                if (order != null)
                {
                    var customerLang = await _languageService.GetLanguageByIdAsync(order.CustomerLanguageId);
                    if (customerLang == null)
                        customerLang = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
                    if (customerLang != null)
                        languageId = customerLang.Id;
                }
                else
                {
                    languageId = _localizationSettings.DefaultAdminLanguageId;
                }

                var queuedEmailIds = await _workflowMessageService.SendGiftCardNotificationAsync(giftCard, languageId);
                if (queuedEmailIds.Any())
                {
                    giftCard.IsRecipientNotified = true;
                    await _giftCardService.UpdateGiftCardAsync(giftCard);
                    model.IsRecipientNotified = true;
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.GiftCards.RecipientNotified"));
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
            }

            //prepare model
            model = await _giftCardModelFactory.PrepareGiftCardModelAsync(model, giftCard);

            return View(model);
        }
    }
}

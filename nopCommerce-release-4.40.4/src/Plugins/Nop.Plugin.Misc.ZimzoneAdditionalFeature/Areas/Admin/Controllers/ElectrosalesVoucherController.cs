using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Voucher;
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
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class ElectrosalesVoucherController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IGiftCardModelFactory _giftCardModelFactory;
        private readonly IElectrosalesVoucherModelFactory _electrosalesVoucherModelFactory;
        private readonly IGiftCardService _giftCardService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly CurrencySettings _currencySettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly INotificationService _notificationService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ILanguageService _languageService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IProductService _productService;
        private readonly GiftVoucherSettings _giftVoucherSettings;

        public ElectrosalesVoucherController(IPermissionService permissionService,
            IGiftCardModelFactory giftCardModelFactory,
            IElectrosalesVoucherModelFactory electrosalesVoucherModelFactory,
            IGiftCardService giftCardService,
            IOrderService orderService,
            IPriceFormatter priceFormatter,
            CurrencySettings currencySettings,
            IDateTimeHelper dateTimeHelper,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            INotificationService notificationService,
            LocalizationSettings localizationSettings,
            ILanguageService languageService,
            IWorkflowMessageService workflowMessageService,
            IProductService productService,
            GiftVoucherSettings giftVoucherSettings)
        {
            _permissionService = permissionService;
            _giftCardModelFactory = giftCardModelFactory;
            _electrosalesVoucherModelFactory = electrosalesVoucherModelFactory;
            _giftCardService = giftCardService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _currencySettings = currencySettings;
            _dateTimeHelper = dateTimeHelper;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _notificationService = notificationService;
            _localizationSettings = localizationSettings;
            _languageService = languageService;
            _workflowMessageService = workflowMessageService;
            _productService = productService;
            _giftVoucherSettings = giftVoucherSettings;
        }

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            //prepare model
            var model = await _giftCardModelFactory.PrepareGiftCardSearchModelAsync(new GiftCardSearchModel());

            return View(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> GiftVoucherList(GiftCardSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _electrosalesVoucherModelFactory.PrepareElectrosalesVoucherListModelAsync(searchModel);

            return Json(model);
        }



        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            //try to get a gift card with the specified id
            var giftCard = await _giftCardService.GetGiftCardByIdAsync(id);


            if (giftCard == null)
                return RedirectToAction("List");
            var orderItem = await _orderService.GetOrderItemByIdAsync(giftCard?.PurchasedWithOrderItemId ?? 0);
            if (orderItem == null)
                return RedirectToAction("List");
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            if (product == null)
                return RedirectToAction("List");
            if (product.Sku != _giftVoucherSettings.ElectrosalesGiftProductSku)
                return RedirectToAction("List");


            //prepare model
            var model = await _electrosalesVoucherModelFactory.PrepareElectrosalesVoucherModelAsync(giftCard);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Edit(ElectrosalesVoucherModel voucherModel, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();
            var model = (GiftCardModel)voucherModel;
            //try to get a gift card with the specified id
            var giftCard = await _giftCardService.GetGiftCardByIdAsync(model.Id);
            if (giftCard == null)
                return RedirectToAction("List");

            var order = await _orderService.GetOrderByOrderItemAsync(giftCard.PurchasedWithOrderItemId ?? 0);

            model.PurchasedWithOrderId = order?.Id;
            model.RemainingAmountStr = await _priceFormatter.FormatPriceAsync(await _giftCardService.GetGiftCardRemainingAmountAsync(giftCard), true, false);
            model.AmountStr = await _priceFormatter.FormatPriceAsync(giftCard.Amount, true, false);
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(giftCard.CreatedOnUtc, DateTimeKind.Utc);
            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            model.PurchasedWithOrderNumber = order?.CustomOrderNumber;

            if (ModelState.IsValid)
            {
                var couponCode = giftCard.GiftCardCouponCode;
                giftCard = model.ToEntity(giftCard);
                giftCard.GiftCardCouponCode = couponCode;
                await _giftCardService.UpdateGiftCardAsync(giftCard);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditGiftCard",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditGiftCard"), giftCard.GiftCardCouponCode), giftCard);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVoucher.Update.Success"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = giftCard.Id });
            }

            //prepare model
            model = await _giftCardModelFactory.PrepareGiftCardModelAsync(model, giftCard, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }


        [HttpPost, ActionName("Edit")]
        [FormValueRequired("notifyRecipient")]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> NotifyRecipient(ElectrosalesVoucherModel model)
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
            model = await _electrosalesVoucherModelFactory.PrepareElectrosalesVoucherModelAsync(giftCard);
            return View(model);
        }

        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            //try to get a gift card with the specified id
            var giftCard = await _giftCardService.GetGiftCardByIdAsync(id);
            if (giftCard == null)
                return RedirectToAction("List");

            await _giftCardService.DeleteGiftCardAsync(giftCard);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteGiftCard",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteGiftCard"), giftCard.GiftCardCouponCode), giftCard);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVoucher.Delete.Success"));

            return RedirectToAction("List");
        }

    }
}

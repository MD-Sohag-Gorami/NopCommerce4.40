using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Query;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Events
{
    public class ServiceRequestEventConsumer : IConsumer<ServiceRequestAcceptedEvent>, IConsumer<ServiceRequestSubmittedEvent>, IConsumer<OrderPaidEvent>, IConsumer<QuestionSubmittedEvent>, IConsumer<EntityInsertedEvent<GiftCard>>, IConsumer<EntityUpdatedEvent<Order>>, IConsumer<CustomOrderStatusChangedEvent>, IConsumer<EntityUpdatedEvent<Product>>,
      IConsumer<EntityDeletedEvent<ZimzoneServiceRequestEntity>>, 
        IConsumer<ShipmentUpdatedEvent>//, IConsumer<EntityUpdatedEvent<ZimzoneServiceRequestEntity>>
    {
        private readonly IOrderService _orderService;
        private readonly IZimzoneServiceEntityService _zimzoneService;
        private readonly IServiceRequestNotificationService _serviceRequestNotificationService;
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IGiftCardAttributeParser _giftCardAttributeParser;
        private readonly IGiftCardService _giftCardService;
        private readonly ICustomOrderStatusService _customOrderStatusService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly ILogger _logger;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IEventPublisher _eventPublisher;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly IShipmentService _shipmentService;
        private readonly IVendorService _vendorService;

        public ServiceRequestEventConsumer(IOrderService orderService,
            IZimzoneServiceEntityService zimzoneService,
            IServiceRequestNotificationService serviceRequestNotificationService,
            IServiceRequestService serviceRequestService,
            IGiftCardAttributeParser giftCardAttributeParser,
            IGiftCardService giftCardService,
            ICustomOrderStatusService customOrderStatusService,
            IMessageTemplateService messageTemplateService,
            ILocalizationService localizationService,
            IWorkflowMessageService workflowMessageService,
            IStoreService storeService,
            IStoreContext storeContext,
            ILanguageService languageService,
            IWorkContext workContext,
            IMessageTokenProvider messageTokenProvider,
            IEmailAccountService emailAccountService,
            EmailAccountSettings emailAccountSettings,
            IAddressService addressService,
            ICustomerService customerService,
            IProductService productService,
            ILogger logger,
            IShoppingCartService shoppingCartService,
            IEventPublisher eventPublisher,
            MessageTemplatesSettings templatesSettings,
            CatalogSettings catalogSettings,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            IShipmentService shipmentService,
            IVendorService vendorService)
        {
            _orderService = orderService;
            _zimzoneService = zimzoneService;
            _serviceRequestNotificationService = serviceRequestNotificationService;
            _serviceRequestService = serviceRequestService;
            _giftCardAttributeParser = giftCardAttributeParser;
            _giftCardService = giftCardService;
            _customOrderStatusService = customOrderStatusService;
            _messageTemplateService = messageTemplateService;
            _localizationService = localizationService;
            _workflowMessageService = workflowMessageService;
            _storeService = storeService;
            _storeContext = storeContext;
            _languageService = languageService;
            _workContext = workContext;
            _messageTokenProvider = messageTokenProvider;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;
            _addressService = addressService;
            _customerService = customerService;
            _productService = productService;
            _logger = logger;
            _shoppingCartService = shoppingCartService;
            _eventPublisher = eventPublisher;
            _templatesSettings = templatesSettings;
            _catalogSettings = catalogSettings;
            _measureService = measureService;
            _measureSettings = measureSettings;
            _shipmentService = shipmentService;
            _vendorService = vendorService;
        }
        public async Task HandleEventAsync(ServiceRequestAcceptedEvent eventMessage)
        {
            var request = eventMessage.Request;
            var emailIds = await _serviceRequestNotificationService.SendServiceRequestAcceptedNotificationAsync(request);
            await Task.CompletedTask;
        }

        public async Task HandleEventAsync(ServiceRequestSubmittedEvent eventMessage)
        {
            var request = eventMessage.Request;
            var emailIds = await _serviceRequestNotificationService.SendServiceRequestSubmittedNotificationAsync(request);

            await Task.CompletedTask;
        }

        public async Task HandleEventAsync(OrderPaidEvent eventMessage)
        {
            var order = eventMessage.Order;
            var orderItems = (await _orderService.GetOrderItemsAsync(order.Id));
            var servicePaymentProductIds = await _zimzoneService.GetAllPaymentProductIdAsync();
            var requestId = 0;
            foreach (var orderItem in orderItems)
            {
                if (servicePaymentProductIds.Contains(orderItem.ProductId))
                {
                    if (int.TryParse(orderItem.AttributesXml, out requestId))
                    {
                        var request = await _serviceRequestService.GetRequestByIdAsync(requestId);
                        if (request == null)
                            continue;
                        request.PaidByOrderItemId = orderItem.Id;
                        await _serviceRequestService.UpdateRequestAsync(request);
                    }
                }
            }
        }

        public async Task HandleEventAsync(QuestionSubmittedEvent eventMessage)
        {
            var query = eventMessage.Query;
            _ = await _serviceRequestNotificationService.SendQuerySubmittedNotificationAsync(query);
            _ = await _serviceRequestNotificationService.SendQuerySubmittedAdminNotificationAsync(query);
            await Task.CompletedTask;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<GiftCard> eventMessage)
        {
            if (eventMessage.Entity.PurchasedWithOrderItemId.HasValue)
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(eventMessage.Entity.PurchasedWithOrderItemId.Value);
                if (orderItem != null)
                {
                    _giftCardAttributeParser.GetGiftCardAttribute(orderItem.AttributesXml, out var giftCardFirstName,
               out var giftCardLastName, out _, out _,
               out _, out _, out _,
               out _, out _, out _);

                    eventMessage.Entity.RecipientName = $"{giftCardFirstName} {giftCardLastName}";
                    await _giftCardService.UpdateGiftCardAsync(eventMessage.Entity);
                }

            }
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Order> eventMessage)
        {
            var order = eventMessage.Entity;
            var orderWithCustomStatus = await _customOrderStatusService.GetOrderWithCustomStatusAsync(orderId: order.Id);
            if (orderWithCustomStatus != null)
            {
                if (order.OrderStatusId != orderWithCustomStatus.ParentOrderStatusId)
                {
                    await _customOrderStatusService.DeleteOrderWithCustomStatusAsync(orderWithCustomStatus: orderWithCustomStatus);
                }
            }
        }

        public async Task HandleEventAsync(CustomOrderStatusChangedEvent eventMessage)
        {
            var orderWithCustomStatus = eventMessage.OrderWithCustomStatus;
            var customOrderStatus = await _customOrderStatusService.GetCustomOrderStatusByIdAsync(orderWithCustomStatus.CustomOrderStatusId);
            var order = await _orderService.GetOrderByIdAsync(orderWithCustomStatus.OrderId);

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();

            var language = await _workContext.GetWorkingLanguageAsync();

            var languageId = await EnsureLanguageIsActiveAsync(language.Id, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(CustomOrderStatusDefaults.MessageTemplateName, store.Id);

            if (!messageTemplates.Any())
                return;

            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            commonTokens.Add(new Token("Order.CustomOrderStatusMessage", customOrderStatus?.CustomOrderStatusName ?? string.Empty));

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                if (await _customerService.IsRegisteredAsync(customer))
                {
                    toEmail = customer.Email;
                    toName = await _customerService.GetCustomerFullNameAsync(customer);
                }

                return await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
        {
            var product = eventMessage.Entity;

            var servicePaymentProductIds = await _zimzoneService.GetAllPaymentProductIdAsync();

            if (servicePaymentProductIds.Contains(product.Id))
            {
                return;
            }

            // operations when created group product

            if (product.ParentGroupedProductId != 0 && product.VisibleIndividually)
            {
                product.VisibleIndividually = false;
                await _productService.UpdateProductAsync(product);
                return;
            }
            //operations when deleted group product

            if (product.ParentGroupedProductId == 0 && !product.VisibleIndividually)
            {
                product.VisibleIndividually = true;
                await _productService.UpdateProductAsync(product);
            }
        }

        public async Task HandleEventAsync(EntityDeletedEvent<ZimzoneServiceRequestEntity> eventMessage)
        {
            var serviceRequest = eventMessage.Entity;
            // fetch the service product and payment product
            var serviceProduct = await _zimzoneService.GetById(serviceRequest.ZimZoneServiceId);
            var paymentProduct = await _productService.GetProductByIdAsync(serviceProduct.ServicePaymentProductId);

            // fetch item from the cart of the customer


            var shoppingCartItem = (await _shoppingCartService.GetShoppingCartAsync(await _customerService.GetCustomerByIdAsync(serviceRequest.CustomerId), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id)).Where(x => x.AttributesXml == serviceRequest.Id.ToString()).FirstOrDefault();

            if (shoppingCartItem != null)
            {
                // delete the shopping cart item
                await _shoppingCartService.DeleteShoppingCartItemAsync(shoppingCartItem);
            }
            
        }

        public async Task HandleEventAsync(ShipmentUpdatedEvent eventMessage)
        {
            var shipment = eventMessage.Entity;
            await SendShipmentCreatedAdminNotificationAsync(shipment);
        }

        protected virtual async Task<IList<int>> SendShipmentCreatedAdminNotificationAsync(Shipment shipment)
        {
            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            var languageId = await EnsureLanguageIsActiveAsync(order.CustomerLanguageId, store.Id);

            // copy message template(OrderPlaced.CustomerNotification) ID=21 to a new one
            // two local resource must be added

            var messageTemplates = await GetActiveMessageTemplatesAsync(AdditionalFeatureDefaults.ShipmentCreatedAdminNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            var orderBillingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            //tokens
            var commonTokens = new List<Token>();

            commonTokens.Add(new Token("Shipment.TotalWeight", $"{shipment.TotalWeight:F2} [{(await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name}]"));
            commonTokens.Add(new Token("Customer.Email", customer?.Email??orderBillingAddress?.Email??string.Empty));
            commonTokens.Add(new Token("Shipment.ShipmentNumber", shipment.Id));
            commonTokens.Add(new Token("Shipment.TrackingNumber", shipment.TrackingNumber));
            commonTokens.Add(new Token("Shipment.Product(s)", await ShipmentProductListToHtmlTableAsync(shipment, languageId), true));

            await _messageTokenProvider.AddShipmentTokensAsync(commonTokens, shipment, languageId);
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        protected virtual async Task<string> ShipmentProductListToHtmlTableAsync(Shipment shipment, int languageId)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color1};text-align:center;\">");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Name", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Vendor")}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Quantity", languageId)}</th>");
            sb.AppendLine("</tr>");

            var table = await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id);
            for (var i = 0; i <= table.Count - 1; i++)
            {
                var si = table[i];
                var orderItem = await _orderService.GetOrderItemByIdAsync(si.OrderItemId);

                if (orderItem == null)
                    continue;

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                if (product == null)
                    continue;

                sb.AppendLine($"<tr style=\"background-color: {_templatesSettings.Color2};text-align: center;\">");
                //product name
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }

                //rental info
                if (product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value) : string.Empty;
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value) : string.Empty;
                    var rentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                    sb.AppendLine("<br />");
                    sb.AppendLine(rentalInfo);
                }

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                    if (!string.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format(await _localizationService.GetResourceAsync("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
                    }
                }

                sb.AppendLine("</td>");

                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{vendor?.Name??await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Messages.Order.Product(s).VendorAdmin")}</td>");

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{si.Quantity}</td>");

                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            var result = sb.ToString();
            return result;
        }

        protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
        {
            //load language by specified ID
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            }

            if (language == null || !language.Published)
            {
                //load any language
                language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");

            return language.Id;
        }

        protected virtual async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
        {
            //get message templates by the name
            var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

            //no template found
            if (!messageTemplates?.Any() ?? true)
                return new List<MessageTemplate>();

            //filter active templates
            messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

            return messageTemplates;
        }

        protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId) ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                               (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            return emailAccount;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Events;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Services;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Query;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public class ServiceRequestNotificationService : IServiceRequestNotificationService
    {
        private readonly LocalizationSettings _localizationSettings;
        private readonly IStoreContext _storeContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILanguageService _languageService;
        private readonly IStoreService _storeService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILocalizationService _localizationService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerService _customerService;
        private readonly IServiceRequestService _requestService;
        private readonly IZimzoneServiceEntityService _serviceEntityService;
        //private readonly IUrlHelperFactory _urlHelperFactory;
        //private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IWebHelper _webHelper;
        private readonly IDownloadService _downloadService;

        public ServiceRequestNotificationService(LocalizationSettings localizationSettings,
            IStoreContext storeContext, IWorkflowMessageService workflowMessageService,
            ILanguageService languageService,
            IStoreService storeService,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService,
            ILocalizationService localizationService,
            EmailAccountSettings emailAccountSettings,
            IMessageTokenProvider messageTokenProvider,
            IEventPublisher eventPublisher,
            ICustomerService customerService,
            IServiceRequestService requestService,
            IZimzoneServiceEntityService serviceEntityService,
            //IUrlHelperFactory urlHelperFactory,
            //IActionContextAccessor actionContextAccessor,
            IWebHelper webHelper,
            IDownloadService downloadService
            )
        {
            _localizationSettings = localizationSettings;
            _storeContext = storeContext;
            _workflowMessageService = workflowMessageService;
            _languageService = languageService;
            _storeService = storeService;
            _messageTemplateService = messageTemplateService;
            _emailAccountService = emailAccountService;
            _localizationService = localizationService;
            _emailAccountSettings = emailAccountSettings;
            _messageTokenProvider = messageTokenProvider;
            _eventPublisher = eventPublisher;
            _customerService = customerService;
            _requestService = requestService;
            _serviceEntityService = serviceEntityService;
            //_urlHelperFactory = urlHelperFactory;
            //_actionContextAccessor = actionContextAccessor;
            _webHelper = webHelper;
            _downloadService = downloadService;
        }

        public async Task<IList<int>> SendServiceRequestSubmittedNotificationAsync(ZimzoneServiceRequestEntity serviceRequest)
        {
            if (serviceRequest == null)
                throw new ArgumentNullException(nameof(serviceRequest));

            //var affiliate = await _affiliateService.GetAffiliateByIdAsync(order.AffiliateId);

            //if (affiliate == null)
            //    throw new ArgumentNullException(nameof(affiliate));

            var store = await _storeContext.GetCurrentStoreAsync();
            var languageId = await EnsureLanguageIsActiveAsync(_localizationSettings.DefaultAdminLanguageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(ServiceRequestTemplateSystemNames.ServiceRequestSubmitted, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await AddServiceRequestTokensAsync(commonTokens, serviceRequest);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, serviceRequest.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
                var (name, email, _, _, _) = await _requestService.ParseServiceRequestAttributeAsync(serviceRequest);
                var customer = await _customerService.GetCustomerByIdAsync(serviceRequest.CustomerId);
                var toEmail = email;
                var toName = name;
                return await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }


        public async Task<IList<int>> SendServiceRequestAcceptedNotificationAsync(ZimzoneServiceRequestEntity serviceRequest)
        {
            if (serviceRequest == null)
                throw new ArgumentNullException(nameof(serviceRequest));

            //var affiliate = await _affiliateService.GetAffiliateByIdAsync(order.AffiliateId);

            //if (affiliate == null)
            //    throw new ArgumentNullException(nameof(affiliate));

            var store = await _storeContext.GetCurrentStoreAsync();
            var languageId = await EnsureLanguageIsActiveAsync(_localizationSettings.DefaultAdminLanguageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(ServiceRequestTemplateSystemNames.ServiceRequestAccepted, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await AddServiceRequestTokensAsync(commonTokens, serviceRequest);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, serviceRequest.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
                var (name, email, address, description, downloadId) = await _requestService.ParseServiceRequestAttributeAsync(serviceRequest);
                var customer = await _customerService.GetCustomerByIdAsync(serviceRequest.CustomerId);
                var toEmail = serviceRequest.CustomerEmail;
                var toName = name;
                return await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        public async Task<IList<int>> SendQuerySubmittedNotificationAsync(Question question)
        {
            //TODO
            if (question == null)
                throw new ArgumentNullException(nameof(question));

            var store = await _storeContext.GetCurrentStoreAsync();
            var languageId = await EnsureLanguageIsActiveAsync(_localizationSettings.DefaultAdminLanguageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(ServiceRequestTemplateSystemNames.QuerySubmittedCustomerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await AddQueryTokensAsync(commonTokens, question);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, question.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
                //var customer = await _customerService.GetCustomerByIdAsync(question.CustomerId);
                var toEmail = question.Email;
                var toName = $"{question.FirstName} {question.LastName}";
                return await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }


        public virtual async Task AddServiceRequestTokensAsync(IList<Token> tokens, ZimzoneServiceRequestEntity serviceRequest)
        {
            var service = await _serviceEntityService.GetById(serviceRequest.ZimZoneServiceId);
            var (name, email, address, description, _) = await _requestService.ParseServiceRequestAttributeAsync(serviceRequest);

            var descriotionText = description.Replace("<p>", string.Empty);
            descriotionText = descriotionText.Replace("</p>", string.Empty);

            // replace custom name with name
            var serviceRequestName = service?.ServiceProductName ?? string.Empty;
            if (!string.IsNullOrEmpty(serviceRequest.CustomName))
            {
                serviceRequestName = serviceRequest.CustomName;
            }

            tokens.Add(new Token("ServiceRequest.Name", serviceRequestName));
            tokens.Add(new Token("ServiceRequest.ReqestNumber", serviceRequest.Id));
            tokens.Add(new Token("ServiceRequest.CustomerName", name));
            tokens.Add(new Token("ServiceRequest.CustomerEmail", email));
            tokens.Add(new Token("ServiceRequest.CustomerAddress", address));
            tokens.Add(new Token("ServiceRequest.Description", descriotionText));
            tokens.Add(new Token("ServiceRequest.CreatedOn", serviceRequest.CreatedOn));
            if (serviceRequest != null && serviceRequest.QuoteDownloadId > 0)
            {
                var url = $"{_webHelper.GetStoreLocation()}service-request-quote-file-{serviceRequest.QuoteDownloadId}";
                tokens.Add(new Token("ServiceRequest.QuoteDownLoadInfo", url));
            }
            else
            {
                tokens.Add(new Token("ServiceRequest.QuoteDownLoadInfo", string.Empty));
            }

        }
        public virtual async Task AddQueryTokensAsync(IList<Token> tokens, Question question)
        {
            tokens.Add(new Token("Query.FirstName", question?.FirstName ?? string.Empty));
            tokens.Add(new Token("Query.LastName", question?.LastName ?? string.Empty));
            tokens.Add(new Token("Query.Email", question?.Email ?? string.Empty));
            tokens.Add(new Token("Query.PhoneNumber", question?.PhoneNumber ?? string.Empty));
            tokens.Add(new Token("Query.ProductName", question?.ProductName ?? string.Empty));
            tokens.Add(new Token("Query.ProductDescription", question?.ProductDescription ?? string.Empty));
            tokens.Add(new Token("Query.Message", question?.Message ?? string.Empty));
            await Task.CompletedTask;
        }
        public virtual async Task AddAdminQueryTokensAsync(IList<Token> tokens, Question question)
        {
            tokens.Add(new Token("Query.FirstName", question?.FirstName ?? string.Empty));
            tokens.Add(new Token("Query.LastName", question?.LastName ?? string.Empty));
            tokens.Add(new Token("Query.Email", question?.Email ?? string.Empty));
            tokens.Add(new Token("Query.PhoneNumber", question?.PhoneNumber ?? string.Empty));
            tokens.Add(new Token("Query.ProductName", question?.ProductName ?? string.Empty));
            tokens.Add(new Token("Query.ProductDescription", question?.ProductDescription ?? string.Empty));
            tokens.Add(new Token("Query.Message", question?.Message ?? string.Empty));
            tokens.Add(new Token("Query.AdditionalLink", question?.AdditionalLink ?? string.Empty));
            tokens.Add(new Token("Query.EditLink", question != null && question.Id > 0 ? $"{_webHelper.GetStoreLocation()}Admin/Query/Edit/{question.Id}" : string.Empty));
            if (question != null && !question.DownloadGuid.Equals(Guid.Empty))
            {
                var url = $"{_webHelper.GetStoreLocation()}Admin/Query/GetFile?downloadGuid={question.DownloadGuid}";
                tokens.Add(new Token("Query.FileDownloadLink", url));
            }
            else
            {
                tokens.Add(new Token("Query.FileDownloadLink", string.Empty));
            }
            await Task.CompletedTask;
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

        public async Task<IList<int>> SendQuerySubmittedAdminNotificationAsync(Question question)
        {
            if (question == null)
                throw new ArgumentNullException(nameof(question));

            var store = await _storeContext.GetCurrentStoreAsync();
            var languageId = await EnsureLanguageIsActiveAsync(_localizationSettings.DefaultAdminLanguageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(ServiceRequestTemplateSystemNames.QuerySubmittedAdminNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await AddAdminQueryTokensAsync(commonTokens, question);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, question.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
                //var customer = await _customerService.GetCustomerByIdAsync(question.CustomerId);
                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;
                return await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }
    }
}

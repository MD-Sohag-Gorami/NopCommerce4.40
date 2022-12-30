using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Query;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.Query;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Controllers
{
    public class QueryController : BaseAdminController
    {
        private readonly IQuestionService _questionService;
        private readonly IPermissionService _permissionService;
        private readonly IQueryModelFactory _queryModelFactory;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IDownloadService _downloadService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public QueryController(IQuestionService questionService,
            IPermissionService permissionService,
            IQueryModelFactory queryModelFactory,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IWorkContext workContext,
            IDownloadService downloadService,
            IDateTimeHelper dateTimeHelper)
        {
            _questionService = questionService;
            _permissionService = permissionService;
            _queryModelFactory = queryModelFactory;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _customerService = customerService;
            _workContext = workContext;
            _downloadService = downloadService;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<IActionResult> QueryList()
        {
            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.AccessQueriesPanel))
                return AccessDeniedView();

            //prepare model
            var model = await _queryModelFactory.PrepareQuerySearchModelAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> QueryList(QuestionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.AccessQueriesPanel))
                return await AccessDeniedDataTablesJson();
            if (searchModel == null)
            {
                return RedirectToAction("QueryList");
            }
            //prepare model
            var model = await _queryModelFactory.PrepareQueryListModelAsync(searchModel);

            return Json(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.AccessQueriesPanel))
                return AccessDeniedView();
            var query = await _questionService.GetQuestionByIdAsync(id);
            if (query == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.QueryNotFoundError"));
                return RedirectToAction("QueryList");
            }
            var model = new QuestionModel
            {
                FirstName = query.FirstName,
                LastName = query.LastName,
                FullName = $"{query.FirstName} {query.LastName}",
                Email = query.Email,
                PhoneNumber = query.PhoneNumber,
                ProductName = query.ProductName,
                ProductDescription = query.ProductDescription,
                Message = query.Message,
                AdditionalLink = query.AdditionalLink,
                DownloadGuid = query.DownloadGuid == Guid.Empty ? string.Empty : query.DownloadGuid.ToString(),
                AdminComment = query.AdminComment,
                MarkedAsRead = query.MarkedAsRead,
                MarkedBy = query.MarkedByUserId,
                CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(query.CreatedOnUtc, DateTimeKind.Utc)
            };

            if (query.MarkedByUserId > 0)
            {
                var customer = await _customerService.GetCustomerByIdAsync(query.MarkedByUserId);
                if (customer != null)
                {
                    model.MarkedByUserName = await _customerService.GetCustomerFullNameAsync(customer);
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(QuestionModel model)
        {
            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.AccessQueriesPanel))
                return AccessDeniedView();
            var query = await _questionService.GetQuestionByIdAsync(model.Id);
            if (query == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.QueryNotFoundError"));
                return RedirectToAction("QueryList");
            }
            else
            {
                query.AdminComment = model.AdminComment;
                var customer = await _workContext.GetCurrentCustomerAsync();
                //var customerName = await _customerService.GetCustomerFullNameAsync(customer);
                query.MarkedByUserId = customer?.Id ?? 0;
                query.MarkedAsRead = model.MarkedAsRead;
                await _questionService.UpdateQuestionAsync(query);
            }
            return RedirectToAction("QueryList");
        }
        public async Task<IActionResult> Delete(int id)
        {
            var query = await _questionService.GetQuestionByIdAsync(id);
            if (query == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.QueryNotFoundError"));
                return RedirectToAction("QueryList");
            }
            await _questionService.DeleteQuestionAsync(query);
            return RedirectToAction("QueryList");
        }

        public async Task<IActionResult> GetFile(Guid downloadGuid)
        {
            if (!await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.AccessQueriesPanel))
                return AccessDeniedView();
            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
            if (download == null)
            {
                return NotFound();
            }
            return File(download.DownloadBinary, download.ContentType, download.Filename + download.Extension);
        }
    }
}

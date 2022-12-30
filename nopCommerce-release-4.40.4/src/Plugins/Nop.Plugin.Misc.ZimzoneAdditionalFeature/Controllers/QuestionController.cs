using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Query;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Query;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.Query;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Controllers
{
    public class QuestionController : BasePluginController
    {
        private readonly ICustomerService _customerService;
        private readonly CaptchaSettings _captchaSettings;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IDownloadService _downloadService;
        private readonly INopFileProvider _fileProvider;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IQuestionService _questionService;

        public QuestionController(ICustomerService customerService,
            CaptchaSettings captchaSettings,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IDownloadService downloadService,
            INopFileProvider fileProvider,
            IGenericAttributeService genericAttributeService,
            IQuestionService questionService)
        {
            _customerService = customerService;
            _captchaSettings = captchaSettings;
            _workContext = workContext;
            _localizationService = localizationService;
            _downloadService = downloadService;
            _fileProvider = fileProvider;
            _genericAttributeService = genericAttributeService;
            _questionService = questionService;
        }

        public async Task<IActionResult> SubmitQuestion()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsGuestAsync(customer))
            {
                return RedirectToRoute("Login", new { returnUrl = Url.RouteUrl("Customer.AdditionalFeature") });
            }
            else
            {
                var model = new QuestionModel
                {
                    DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage
                };
                model.FirstName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                model.LastName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute);
                model.Email = customer.Email;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateCaptcha]
        public async Task<IActionResult> SubmitQuestion(QuestionModel model, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid)
            {
                ModelState.AddModelError("Captcha", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage;

            if (ModelState.IsValid)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var question = new Question
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    ProductName = model.ProductName,
                    ProductDescription = model.ProductDescription,
                    Message = model.Message,
                    CustomerId = customer?.Id ?? 0,
                    AdditionalLink = model.AdditionalLink,
                    DownloadGuid = string.IsNullOrEmpty(model.DownloadGuid) ? Guid.Empty : Guid.Parse(model.DownloadGuid),
                    AdditionalField = model.AdditionalField,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _questionService.InsertQuestionAsync(question);

                model.SuccessfullySubmitted = true;
                model.Result = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Question.YourEnquiryHasBeenSent");

                return View(model);
            }
            return View(model);
        }


        [HttpPost]
        [IgnoreAntiforgeryToken]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> UploadFileSubmitQuery()
        {
            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No file uploaded",
                    downloadGuid = Guid.Empty
                });
            }

            var fileBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile);

            var qqFileNameParameter = "qqfilename";
            var fileName = httpPostedFile.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = httpPostedFile.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();


            //compare in bytes

            var maximumFileSizeInMB = 3000;
            var maxFileSizeBytes = maximumFileSizeInMB * 1024;
            if (fileBinary.Length > maxFileSizeBytes)
            {
                //when returning JSON the mime-type must be set to text/plain
                //otherwise some browsers will pop-up a "Save As" dialog.
                return Json(new
                {
                    success = false,
                    message = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"), maximumFileSizeInMB),
                    downloadGuid = Guid.Empty
                });
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = string.Empty,
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };
            await _downloadService.InsertDownloadAsync(download);

            //when returning JSON the mime-type must be set to text/plain
            //otherwise some browsers will pop-up a "Save As" dialog.
            return Json(new
            {
                success = true,
                message = await _localizationService.GetResourceAsync("ShoppingCart.FileUploaded"),
                downloadUrl = Url.Action("GetFileUpload", "Download", new { downloadId = download.DownloadGuid }),
                downloadGuid = download.DownloadGuid
            });
        }
    }
}

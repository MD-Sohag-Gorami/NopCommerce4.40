using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Query
{
    public record QuestionModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.FirstName")]
        [Required]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.LastName")]
        [Required]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.Email")]
        [Required]
        public string Email { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.PhoneNumber")]
        [Required]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.ProductName")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.ProductDescription")]
        public string ProductDescription { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.Message")]
        [Required]
        public string Message { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.AdditionalLink")]
        public string AdditionalLink { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.DownloadGuid")]
        public string DownloadGuid { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.AdminComment")]
        public string AdminComment { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.MarkedAsRead")]
        public bool MarkedAsRead { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.MarkedBy")]
        public int MarkedBy { get; set; }

        public string MarkedByUserName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.AdditionalField")]
        public string AdditionalField { get; set; }

        public bool SuccessfullySubmitted { get; set; }

        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.FullName")]
        public string FullName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.Status")]
        public string Status { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Question.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}

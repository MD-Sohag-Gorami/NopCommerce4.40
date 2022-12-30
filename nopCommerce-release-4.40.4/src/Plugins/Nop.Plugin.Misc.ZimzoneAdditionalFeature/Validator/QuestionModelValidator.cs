using FluentValidation;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Query;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.NopStation.Theme.MikesChainsaw.Validators
{
    public class BookServiceModelValidator : BaseNopValidator<QuestionModel>
    {
        public BookServiceModelValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Question.FirstName.Required").Result);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Question.LastName.Required").Result);

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Question.Email.Required").Result);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Question.PhoneNumber.Required").Result);

            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage(localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Question.Message.Required").Result);

        }
    }
}

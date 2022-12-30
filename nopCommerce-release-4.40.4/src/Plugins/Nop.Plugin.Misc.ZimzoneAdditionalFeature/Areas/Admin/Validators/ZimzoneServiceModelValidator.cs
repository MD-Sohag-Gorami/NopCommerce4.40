using FluentValidation;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Validators
{
    public class ZimzoneServiceModelValidator : BaseNopValidator<ZimzoneServiceModel>
    {
        public ZimzoneServiceModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.ServiceProductId)
                 .NotEmpty()
                 .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.ServiceProduct.NotSelectedError"));
            RuleFor(model => model.ServicePaymentProductId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.PaymentProduct.NotSelectedError"));
        }
    }
}
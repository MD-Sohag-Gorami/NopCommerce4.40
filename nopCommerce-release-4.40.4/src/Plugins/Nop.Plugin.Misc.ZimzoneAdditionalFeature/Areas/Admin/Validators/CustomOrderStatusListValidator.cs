using FluentValidation;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.CustomOrderStatus;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Validators
{
    public class CustomOrderStatusListValidator : BaseNopValidator<CustomStatusListWithOrderModel>
    {
        public CustomOrderStatusListValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.CustomOrderStatusId)
                 .GreaterThan(0)
                 .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.ServiceProduct.NotSelectedError"));
        }
    }
}

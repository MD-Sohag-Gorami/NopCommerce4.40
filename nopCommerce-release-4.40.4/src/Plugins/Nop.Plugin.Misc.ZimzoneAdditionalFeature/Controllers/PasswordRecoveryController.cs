using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Customer;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Controllers
{
    public class PasswordRecoveryController : BasePluginController
    {
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;

        public PasswordRecoveryController(ICustomerModelFactory customerModelFactory,
                                          IWorkContext workContext,
                                          IGenericAttributeService genericAttributeService)
        {
            _customerModelFactory = customerModelFactory;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
        }

        public virtual async Task<IActionResult> RecoverPassword(string email)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!string.IsNullOrEmpty(email))
            {
                await _genericAttributeService.SaveAttributeAsync(customer, PasswordRecoveryDefaults.PasswordRecoveryKey, email);
            }
            else
            {
                await _genericAttributeService.SaveAttributeAsync(customer, PasswordRecoveryDefaults.PasswordRecoveryKey, string.Empty);
            }

            return RedirectToRoute("PasswordRecovery");
        }
    }
}

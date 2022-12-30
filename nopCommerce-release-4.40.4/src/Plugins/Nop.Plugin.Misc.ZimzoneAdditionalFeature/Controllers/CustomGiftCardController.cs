using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Factories;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Controllers
{
    public class CustomGiftCardController : BasePluginController
    {
        private readonly IGiftCardHistoryModelFactory _giftCardHistoryModelFactory;

        public CustomGiftCardController(IGiftCardHistoryModelFactory giftCardHistoryModelFactory)
        {
            _giftCardHistoryModelFactory = giftCardHistoryModelFactory;
        }
        public async Task<IActionResult> List()
        {
            var model = await _giftCardHistoryModelFactory.PrepareGiftCardHistoryModelsAsync();
            return View(model);
        }
    }
}

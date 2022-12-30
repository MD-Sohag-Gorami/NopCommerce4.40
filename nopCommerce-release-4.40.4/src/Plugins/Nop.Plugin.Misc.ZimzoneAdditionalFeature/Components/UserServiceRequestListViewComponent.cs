using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Components
{
    public class UserServiceRequestListViewComponent : NopViewComponent
    {


        public UserServiceRequestListViewComponent()
        {

        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View();
        }
    }
}

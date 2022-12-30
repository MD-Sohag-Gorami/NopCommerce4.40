using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == "Admin")
            {
                viewLocations = new[] {
                    $"~/Plugins/Misc.ZimzoneAdditionalFeature/Areas/Admin/Views/Shared/{{0}}.cshtml",
                    $"~/Plugins/Misc.ZimzoneAdditionalFeature/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
            else
            {
                viewLocations = new[] {
                    $"~/Plugins/Misc.ZimzoneAdditionalFeature/Views/Shared/{{0}}.cshtml",
                    $"~/Plugins/Misc.ZimzoneAdditionalFeature/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);

                if (context.Values.TryGetValue(THEME_KEY, out string theme))
                {
                    viewLocations = new[] {
                        $"~/Plugins/Misc.ZimzoneAdditionalFeature/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                        $"~/Plugins/Misc.ZimzoneAdditionalFeature/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                        $"~/Plugins/Misc.ZimzoneAdditionalFeature/Themes/{theme}/Views/{{0}}.cshtml"
                    }.Concat(viewLocations);
                }
            }

            return viewLocations;
        }
    }
}

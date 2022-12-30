using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.NopStation.Core.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";
        private readonly string _systemName;
        private readonly bool _rootAdmin;
        private readonly bool _excludepublicView;

        public ViewLocationExpander(string systemName, bool rootAdmin, bool excludepublicView)
        {
            _systemName = systemName;
            _rootAdmin = rootAdmin;
            _excludepublicView = excludepublicView;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == "Admin")
            {
                if (!_rootAdmin)
                {
                    viewLocations = new[] {
                        $"/Plugins/{_systemName}/Areas/Admin/Views/{{1}}/{{0}}.cshtml",
                        $"/Plugins/{_systemName}/Areas/Admin/Views/Shared/{{0}}.cshtml"
                    }.Concat(viewLocations);
                }
                else
                {
                    viewLocations = new[] {
                        $"/Plugins/{_systemName}/Views/{{1}}/{{0}}.cshtml",
                        $"/Plugins/{_systemName}/Views/Shared/{{0}}.cshtml"
                    }.Concat(viewLocations);
                }
            }
            else if (!_excludepublicView)
            {
                viewLocations = new[] {
                    $"/Plugins/{_systemName}/Views/{{1}}/{{0}}.cshtml",
                    $"/Plugins/{_systemName}/Views/Shared/{{0}}.cshtml"
                }.Concat(viewLocations);

                if (context.Values.TryGetValue(THEME_KEY, out string theme))
                {
                    viewLocations = new[] {
                        $"/Plugins/{_systemName}/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                        $"/Plugins/{_systemName}/Themes/{theme}/Views/Shared/{{0}}.cshtml"
                    }.Concat(viewLocations);
                }
            }

            return viewLocations;
        }
    }
}

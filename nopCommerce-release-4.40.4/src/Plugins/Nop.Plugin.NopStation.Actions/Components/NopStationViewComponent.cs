﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Nop.Core.Infrastructure;
using Nop.Plugin.NopStation.Core.Services;
using Nop.Web.Framework.Components;
using Nop.Plugin.NopStation.Core.Helpers;

namespace Nop.Plugin.NopStation.Core.Components
{
    public abstract class NopStationViewComponent : NopViewComponent
    {
        public new IViewComponentResult Content(string content)
        {
            if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(GetType().Assembly).Result)
                return base.Content("");

            //invoke the base method
            return base.Content(content);
        }

        public new IViewComponentResult View()
        {
            if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(GetType().Assembly).Result)
                return base.Content("");

            //invoke the base method
            return base.View();
        }

        public new IViewComponentResult View<TModel>(string viewName, TModel model)
        {
            if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(GetType().Assembly).Result)
                return base.Content("");

            //invoke the base method
            return base.View<TModel>(viewName, model);
        }

        /// <summary>
        /// Returns a result which will render the partial view
        /// </summary>
        /// <param name="model">The model object for the view.</param>
        /// <returns>A <see cref="ViewViewComponentResult"/>.</returns>
        public new IViewComponentResult View<TModel>(TModel model)
        {
            if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(GetType().Assembly).Result)
                return base.Content("");

            //invoke the base method
            return base.View<TModel>(model);
        }

        /// <summary>
        ///  Returns a result which will render the partial view with name viewName
        /// </summary>
        /// <param name="viewName">The name of the partial view to render.</param>
        /// <returns>A <see cref="ViewViewComponentResult"/>.</returns>
        public new IViewComponentResult View(string viewName)
        {
            if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(GetType().Assembly).Result)
                return base.Content("");

            //invoke the base method
            return base.View(viewName);
        }
    }
}
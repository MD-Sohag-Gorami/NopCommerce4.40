using Nop.Core;
using Nop.Services.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;
using Nop.Web.Framework.Infrastructure;
using Nop.Plugin.NopStation.Core.Services;
using Nop.Plugin.NopStation.Core;
using Nop.Plugin.NopStation.OCarousels.Helpers;
using Nop.Services.Security;
using Nop.Plugin.NopStation.Core.Helpers;
using Nop.Plugin.NopStation.OCarousels.Services;
using System;
using Nop.Plugin.NopStation.OCarousels.Domains;
using System.Threading.Tasks;

namespace Nop.Plugin.NopStation.OCarousels
{
    public class OCarouselPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        public bool HideInWidgetList => false;

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IOCarouselService _carouselService;

        #endregion

        #region Ctor

        public OCarouselPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IOCarouselService carouselService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _carouselService = carouselService;
        }

        #endregion

        #region Utilities

        private async void CreateSampleData()
        {
            var carouselSettings = new OCarouselSettings()
            {
                EnableOCarousel = true
            };
            await _settingService.SaveSettingAsync(carouselSettings);

            var carousel1 = new OCarousel()
            {
                Active = true,
                AutoPlay = true,
                AutoPlayHoverPause = true,
                AutoPlayTimeout = 3000,
                CreatedOnUtc = DateTime.UtcNow,
                DataSourceTypeEnum = DataSourceTypeEnum.HomePageCategories,
                DisplayTitle = true,
                Loop = true,
                LazyLoad = true,
                Name = "Featured Categories",
                Nav = true,
                UpdatedOnUtc = DateTime.UtcNow,
                NumberOfItemsToShow = 10,
                Title = "Featured Categories",
                WidgetZoneId = 2
            };
            await _carouselService.InsertCarouselAsync(carousel1);

            var carousel2 = new OCarousel()
            {
                Active = true,
                AutoPlay = true,
                AutoPlayHoverPause = true,
                AutoPlayTimeout = 3000,
                CreatedOnUtc = DateTime.UtcNow,
                DataSourceTypeEnum = DataSourceTypeEnum.NewProducts,
                DisplayTitle = true,
                Loop = true,
                LazyLoad = true,
                Name = "New Products",
                Nav = true,
                UpdatedOnUtc = DateTime.UtcNow,
                NumberOfItemsToShow = 10,
                Title = "New Products",
                WidgetZoneId = 3
            };
            await _carouselService.InsertCarouselAsync(carousel2);

            var carousel3 = new OCarousel()
            {
                Active = true,
                AutoPlay = true,
                AutoPlayHoverPause = true,
                AutoPlayTimeout = 3000,
                CreatedOnUtc = DateTime.UtcNow,
                DataSourceTypeEnum = DataSourceTypeEnum.BestSellers,
                DisplayTitle = true,
                Loop = true,
                LazyLoad = true,
                Name = "Best Sellers",
                Nav = true,
                UpdatedOnUtc = DateTime.UtcNow,
                NumberOfItemsToShow = 10,
                Title = "Best Sellers",
                WidgetZoneId = 4
            };
            await _carouselService.InsertCarouselAsync(carousel3);

            var carousel4 = new OCarousel()
            {
                Active = true,
                AutoPlay = true,
                AutoPlayHoverPause = true,
                AutoPlayTimeout = 3000,
                CreatedOnUtc = DateTime.UtcNow,
                DataSourceTypeEnum = DataSourceTypeEnum.Manufacturers,
                DisplayTitle = true,
                Loop = true,
                LazyLoad = true,
                Name = "Manufacturers",
                Nav = true,
                UpdatedOnUtc = DateTime.UtcNow,
                NumberOfItemsToShow = 10,
                Title = "Manufacturers",
                WidgetZoneId = 6
            };
            await _carouselService.InsertCarouselAsync(carousel4);
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/OCarousel/Configure";
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if(widgetZone== PublicWidgetZones.Footer)
                return "OCarouselFooter";

            return "OCarousel";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var widgetZones =OCarouselHelper.GetCustomWidgetZones();
            widgetZones.Add(PublicWidgetZones.Footer);
            return Task.FromResult<IList<string>>(widgetZones);
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await _permissionService.AuthorizeAsync(OCarouselPermissionProvider.ManageOCarousels))
            {
                var menuItem = new SiteMapNode()
                {
                    Title =await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Menu.OCarousel"),
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };

                var listItem = new SiteMapNode()
                {
                    Title =await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Menu.List"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/OCarousel/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "OCarousels"
                };
                menuItem.ChildNodes.Add(listItem);

                var configItem = new SiteMapNode()
                {
                    Title =await _localizationService.GetResourceAsync("Admin.NopStation.OCarousels.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/OCarousel/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "OCarousels.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);

                var documentation = new SiteMapNode()
                {
                    Title =await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/ocarousel-documentation?utm_source=admin-panel?utm_source=admin-panel&utm_medium=products&utm_campaign=ocarousel",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);

                await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
            }
        }

        public override async Task InstallAsync()
        {
            CreateSampleData();
            await this.NopStationPluginInstallAsync(new OCarouselPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.NopStationPluginUninstallAsync(new OCarouselPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchActive.Active", "Active"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchActive.Inactive", "Inactive"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Menu.OCarousel", "OCarousel"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Menu.Configuration", "Configuration"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Menu.List", "List"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Configuration", "Carousel settings"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Tab.Info", "Info"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Tab.Properties", "Properties"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Tab.OCarouselItems", "Carousel items"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.CarouselList", "Carousels"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.EditDetails", "Edit carousel details"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.BackToList", "back to carousel list"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.AddNew", "Add new carousel"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.AddNew", "Add new item"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.SaveBeforeEdit", "You need to save the carousel before you can add items for this carousel page."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.AddNew", "Add new item"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.AddNew", "Add new item"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Configuration.Fields.EnableOCarousel", "Enable carousel"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Configuration.Fields.EnableOCarousel.Hint", "Check to enable carousel for your store."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Configuration.Fields.RequireOCarouselPicture", "Require carousel picture"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Configuration.Fields.RequireOCarouselPicture.Hint", "Determines whether main picture is required for carousel (based on theme design)."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Configuration.Updated", "Carousel configuration updated successfully."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Created", "Carousel has been created successfully."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Updated", "Carousel has been updated successfully."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.Deleted", "Carousel has been deleted successfully."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.Fields.Product", "Product"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.Fields.OCarousel", "Carousel"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.Fields.DisplayOrder", "Display order"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarouselItems.Fields.Picture", "Picture"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Name", "Name"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Name.Hint", "The carousel name."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Title", "Title"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Title.Hint", "The carousel title."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DisplayTitle", "Display title"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DisplayTitle.Hint", "Determines whether title should be displayed on public site (depends on theme design)."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Active", "Active"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Active.Hint", "Determines whether this carousel is active (visible on public store)."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.WidgetZone", "Widget zone"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.WidgetZone.Hint", "The widget zone where this carousel will be displayed."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DataSourceType", "Data source type"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DataSourceType.Hint", "The data source for this carousel."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Picture", "Picture"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Picture.Hint", "The carousel picture."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CustomUrl", "Custom url"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CustomUrl.Hint", "The carousel custom url."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.NumberOfItemsToShow", "Number of items to show"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.NumberOfItemsToShow.Hint", "Specify the number of items to show for this carousel."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlay", "Auto play"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlay.Hint", "Check to enable auto play."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CustomCssClass", "Custom css class"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CustomCssClass.Hint", "Enter the custom CSS class to be applied."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DisplayOrder", "Display order"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.DisplayOrder.Hint", "Display order of the carousel. 1 represents the top of the list."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Loop", "Loop"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Loop.Hint", "heck to enable loop."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.StartPosition", "Start position"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.StartPosition.Hint", "TStarting position (e.g 0)"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Center", "Center"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Center.Hint", "Check to center item. It works well with even and odd number of items."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Nav", "NAV"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Nav.Hint", "Check to enable next/prev buttons."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoad", "Lazy load"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoad.Hint", "Check to enable lazy load."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoadEager", "Lazy load eager"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.LazyLoadEager.Hint", "Specify how many items you want to pre-load images to the right (and left when loop is enabled)."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayTimeout", "Auto play timeout"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayTimeout.Hint", "It's autoplay interval timeout. (e.g 5000)"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayHoverPause", "Auto play hover pause"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.AutoPlayHoverPause.Hint", "Check to enable pause on mouse hover."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CreatedOn", "Created on"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.CreatedOn.Hint", "The create date of this carousel."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.UpdatedOn", "Updated on"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.UpdatedOn.Hint", "The last update date of this carousel."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.SelectedStoreIds", "Limited to stores"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.SelectedStoreIds.Hint", "Option to limit this carousel to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.ShowBackgroundPicture", "Show Background Picture"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.ShowBackgroundPicture.Hint", "Check to enable show Background Picture"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.BackgroundPicture", "Background Picture"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.BackgroundPicture.Hint", "Background Picture of the carousel"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Name.Required", "The name field is required."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Title.Required", "The title field is required."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.NumberOfItemsToShow.Required", "The number of items to show field is required."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.Fields.Picture.Required", "The picture field is required."));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchWidgetZones", "Widget zones"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchWidgetZones.Hint", "The search widget zones."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchDataSources", "Data sources"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchDataSources.Hint", "The search data sources."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchStore", "Store"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchStore.Hint", "The search store."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchActive", "Active"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.OCarousels.OCarousels.List.SearchActive.Hint", "The search active."));

            list.Add(new KeyValuePair<string, string>("NopStation.OCarousels.LoadingFailed", "Failed to load carousel content."));

            return list;
        }

        #endregion
    }
}

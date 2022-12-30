using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.ErpSync;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Tasks;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature
{

    public class AdditionalFeaturePlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IRepository<ProductTemplate> _productTemplateRepository;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ILogger _logger;
        private readonly IPermissionService _permissionService;
        private readonly INopFileProvider _nopFileProvider;
        #endregion

        #region Ctor

        public AdditionalFeaturePlugin(
            ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            IProductTemplateService productTemplateService,
            IRepository<ProductTemplate> productTemplateRepository,
            IScheduleTaskService scheduleTaskService,
            IMessageTemplateService messageTemplateService,
            ILogger logger,
            IPermissionService permissionService,
            INopFileProvider nopFileProvider)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _productTemplateService = productTemplateService;
            _productTemplateRepository = productTemplateRepository;
            _scheduleTaskService = scheduleTaskService;
            _messageTemplateService = messageTemplateService;
            _logger = logger;
            _permissionService = permissionService;
            _nopFileProvider = nopFileProvider;
        }

        #endregion

        #region Methods

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { GiftVoucherDefaults.CUSTOM_WIDGET_GIFT_CARD, GiftVoucherDefaults.CUSTOM_WIDGET_ZONE_MEGA_MENU_BEFORE_ITEMS, GiftVoucherDefaults.CUSTOM_WIDGET_GIFT_VOUCHER_PRICE_SELCT_BUTTONS,
              GiftVoucherDefaults.CUSTOM_WIDGET_GIFT_VOUCHER_DELIVERYDATE,
              GiftVoucherDefaults.CUSTOM_WIDGET_GIFT_VOUCHER_CHANGE_TO_USD_MESSAGE,
              GiftVoucherDefaults.CUSTOM_WIDGET_GIFT_VOUCHER_CHANGE_TO_USD_BUTTON,
              PublicWidgetZones.AccountNavigationAfter,
              PublicWidgetZones.AccountNavigationBefore,
              AdminWidgetZones.OrderShipmentDetailsButtons,
              CustomOrderStatusDefaults.OrderStatusBottom,
              AdminWidgetZones.OrderDetailsButtons,
            });
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/AdditionalFeature/Configure";
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            if (widgetZone == GiftVoucherDefaults.CUSTOM_WIDGET_GIFT_CARD)
            {
                return "CustomGiftCard";
            }
            else if (widgetZone == GiftVoucherDefaults.CUSTOM_WIDGET_ZONE_MEGA_MENU_BEFORE_ITEMS)
            {
                return "GiftCardLinks";
            }
            else if (widgetZone == GiftVoucherDefaults.CUSTOM_WIDGET_GIFT_VOUCHER_PRICE_SELCT_BUTTONS)
            {
                return "CustomGiftCardPriceSelectButtons";
            }
            else if (widgetZone == GiftVoucherDefaults.CUSTOM_WIDGET_GIFT_VOUCHER_DELIVERYDATE)
            {
                return "GiftCardDeliveryDate";
            }
            else if (widgetZone == GiftVoucherDefaults.CUSTOM_WIDGET_GIFT_VOUCHER_CHANGE_TO_USD_MESSAGE)
            {
                return "ChangeCurrencyUsdMessage";
            }
            else if (widgetZone == GiftVoucherDefaults.CUSTOM_WIDGET_GIFT_VOUCHER_CHANGE_TO_USD_BUTTON)
            {
                return "ChangeCurrencyUsdButton";
            }
            else if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
            {
                return "GiftCardCustomerNavigation";
            }
            else if (widgetZone == PublicWidgetZones.AccountNavigationBefore)
            {
                return "UserServiceRequestList";
            }
            else if (widgetZone == AdminWidgetZones.OrderShipmentDetailsButtons)
            {
                return "ShipmentPackageSlipSize";
            }
            else if (widgetZone == CustomOrderStatusDefaults.OrderStatusBottom)
            {
                return "CustomOrderStatus";
            }
            else if (widgetZone == AdminWidgetZones.OrderDetailsButtons)
            {
                return "OrderItemPdfDownload";
            }
            else
            {
                return string.Empty;
            }
        }

        public override async Task InstallAsync()
        {
            var settings = new GiftVoucherSettings
            {

            };
            await _settingService.SaveSettingAsync(settings);

            await AddLocalResourceAsync();


            await AddTemplatesAsync();

            //ERP
            var erpSettings = new ErpSyncSettings
            {
                ImageUrlEndpoint = string.Empty,
                ManualSyncFrom = new DateTime(2022, 06, 01),
            };
            await _settingService.SaveSettingAsync(erpSettings);

            //install synchronization task
            if (await _scheduleTaskService.GetTaskByTypeAsync(ErpSyncDefaults.SynchronizationTaskType) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = 24 * 60 * 60,
                    Name = ErpSyncDefaults.SynchronizationTaskName,
                    Type = ErpSyncDefaults.SynchronizationTaskType,
                });
            }

            //auto full erp sync task install
            if (await _scheduleTaskService.GetTaskByTypeAsync(ErpSyncDefaults.AutoFullErpSyncTaskType) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = false,
                    Seconds = 30 * 24 * 60 * 60,
                    Name = ErpSyncDefaults.AutoFullErpSyncTaskName,
                    Type = ErpSyncDefaults.AutoFullErpSyncTaskType,
                });
            }

            //manual full erp sync task install
            if (await _scheduleTaskService.GetTaskByTypeAsync(ErpSyncDefaults.ManualFullErpSyncTaskType) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = false,
                    Seconds = 30 * 24 * 60 * 60,
                    Name = ErpSyncDefaults.ManualFullErpSyncTaskName,
                    Type = ErpSyncDefaults.ManualFullErpSyncTaskType,
                });
            }

            await InstallPermissionsRecordsAndMappingsAsync();

            //run all scripts
            await RunInstallationScriptsAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<GiftVoucherSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.ZimzoneAdditionalFeature");

            await _productTemplateRepository.DeleteAsync(x => x.Name == "Simple Service" && x.ViewPath == "ServiceTemplate.Simple");

            //ERP
            //settings
            await _settingService.DeleteSettingAsync<ErpSyncSettings>();
            //schedule task
            var task = await _scheduleTaskService.GetTaskByTypeAsync(ErpSyncDefaults.SynchronizationTaskType);
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);

            // full erp sync task
            var fullErpSyncTask = await _scheduleTaskService.GetTaskByTypeAsync(ErpSyncDefaults.AutoFullErpSyncTaskType);

            if (fullErpSyncTask != null)
                await _scheduleTaskService.DeleteTaskAsync(fullErpSyncTask);

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.ErpSync");


            var requestSubmittedTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(ServiceRequestTemplateSystemNames.ServiceRequestSubmitted)).FirstOrDefault();
            if (requestSubmittedTemplate != null)
            {
                await _messageTemplateService.DeleteMessageTemplateAsync(requestSubmittedTemplate);
            }
            var requestAcceptedTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(ServiceRequestTemplateSystemNames.ServiceRequestAccepted)).FirstOrDefault();
            if (requestAcceptedTemplate != null)
            {
                await _messageTemplateService.DeleteMessageTemplateAsync(requestAcceptedTemplate);
            }

            var querySubmittedCustomerEmailTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(ServiceRequestTemplateSystemNames.QuerySubmittedCustomerNotification)).FirstOrDefault();
            if (querySubmittedCustomerEmailTemplate != null)
            {
                await _messageTemplateService.DeleteMessageTemplateAsync(querySubmittedCustomerEmailTemplate);
            }

            var querySubmittedAdminEmailTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(ServiceRequestTemplateSystemNames.QuerySubmittedAdminNotification)).FirstOrDefault();
            if (querySubmittedAdminEmailTemplate != null)
            {
                await _messageTemplateService.DeleteMessageTemplateAsync(querySubmittedAdminEmailTemplate);
            }
            await UninstallPermissionsRecordsAndMappingsAsync();

            //run all scripts
            await RunUninstallationScriptsAsync();

            await base.UninstallAsync();
        }


        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            {
                var catalogNode = rootNode.ChildNodes.Where(x => x.Title == "Catalog").FirstOrDefault();
                if (catalogNode != null)
                {
                    var serviceRequestsNode = new SiteMapNode
                    {
                        Title = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.UpdtatePriceAndStock.NodeTitle"),
                        Url = $"{_webHelper.GetStoreLocation()}Admin/ZimZoneProductUpdate/UpdatePriceAndStock",
                        Visible = true,
                        IconClass = "far fa-circle",
                        SystemName = "AdditionalFeature.UpdtatePriceAndStock"
                    };
                    catalogNode.ChildNodes.Add(serviceRequestsNode);
                }
            }
            if (await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneShipmentManage))
            {
                var salesNode = rootNode.ChildNodes.Where(x => x.Title == "Sales").FirstOrDefault();
                var shipmentsNode = salesNode.ChildNodes.Where(x => x.Title == "Shipments" && x.Visible).FirstOrDefault();
                if (shipmentsNode == null)
                {
                    rootNode.ChildNodes.Add(new SiteMapNode
                    {
                        Title = await _localizationService.GetResourceAsync("admin.orders.shipments"),
                        Url = $"{_webHelper.GetStoreLocation()}Admin/Order/ShipmentList",
                        Visible = true,
                        IconClass = "far fa-dot-circle",
                        SystemName = "AdditionalFeature.ShipmentManage"
                    });
                }
            }
            if (await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneConnfigurationPanel))
            {
                var salesNode = rootNode.ChildNodes.Where(x => x.Title == "Sales").FirstOrDefault();
                if (salesNode != null)
                {
                    var serviceRequestsNode = new SiteMapNode
                    {
                        Title = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Services.Requests"),
                        Url = $"{_webHelper.GetStoreLocation()}Admin/ServiceRequest/RequestList",
                        Visible = true,
                        IconClass = "far fa-circle",
                        SystemName = "AdditionalFeature.Service.Requests"
                    };
                    salesNode.ChildNodes.Add(serviceRequestsNode);

                    var electrosalesVouchersNode = new SiteMapNode
                    {
                        Title = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVouchers.PageTitle"),
                        Url = $"{_webHelper.GetStoreLocation()}Admin/ElectrosalesVoucher/List",
                        Visible = true,
                        IconClass = "far fa-circle",
                        SystemName = "AdditionalFeature.ElectrosalesVoucher"
                    };
                    salesNode.ChildNodes.Insert(5, electrosalesVouchersNode);
                }
                var menuItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Name"),
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                };
                var voucherConfigure = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.GiftVoucher.Configure"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/GiftVoucher/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "AdditionalFeature.GiftVoucher.Configure"
                };
                menuItem.ChildNodes.Add(voucherConfigure);

                var serviceAttributeConfigure = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.ServiceAttribute.Configure"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ServiceAttribute/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "AdditionalFeature.ServiceAttribute.Configure"
                };
                menuItem.ChildNodes.Add(serviceAttributeConfigure);

                var erpConfigure = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Misc.ErpSync.Configure"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ErpSync/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "AdditionalFeature.ErpSync.Configure"
                };
                menuItem.ChildNodes.Add(erpConfigure);


                var serviceList = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.ZimzoneServiceList"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/AdditionalFeature/ServiceList",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "AdditionalFeature.ServiceList"
                };
                menuItem.ChildNodes.Add(serviceList);

                var customOrderStatusNode = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.CustomOrderStatusNode"),
                    Visible = true,
                    Url = $"{_webHelper.GetStoreLocation()}Admin/CustomOrderStatus/CustomOrderStatusList",
                    IconClass = "far fa-dot-circle",
                    SystemName = "AdditionalFeature.CustomOrderStatus"
                };

                menuItem.ChildNodes.Add(customOrderStatusNode);

                rootNode.ChildNodes.Add(menuItem);

                var orderWithCustomStatusNode = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.OrderWithCustomStatusNode"),
                    Visible = true,
                    Url = $"{_webHelper.GetStoreLocation()}Admin/CustomOrderStatus/OrderWithCustomStatusList",
                    IconClass = "far fa-dot-circle",
                    SystemName = "AdditionalFeature.OrderWithCustomStatus"
                };
                salesNode.ChildNodes.Insert(1, orderWithCustomStatusNode);
                var orderNode = salesNode.ChildNodes.Where(x => x.Title == "Orders").FirstOrDefault();
                salesNode.ChildNodes.Remove(orderNode);
            }

            if (await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.AccessQueriesPanel))
            {
                var queryNode = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.QureryNode"),
                    Visible = true,
                    Url = $"{_webHelper.GetStoreLocation()}Admin/Query/QueryList",
                    IconClass = "far fa-dot-circle",
                    SystemName = "AdditionalFeature.Queries"
                };
                rootNode.ChildNodes.Add(queryNode);
            }

            if (await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneServiceRequestPanel) && !await _permissionService.AuthorizeAsync(ZimZonePermissionProvider.ZimzoneConnfigurationPanel))
            {
                var serviceRequestsNode = new SiteMapNode
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Misc.ZimzoneAdditionalFeature.Services.Requests"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ServiceRequest/RequestList",
                    Visible = true,
                    IconClass = "far fa-dot-circle",
                    SystemName = "AdditionalFeature.Service.Requests"
                };
                rootNode.ChildNodes.Add(serviceRequestsNode);
            }

        }

        #endregion

        public bool HideInWidgetList => false;

        #region utilities

        async Task AddLocalResourceAsync()
        {
            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableGiftCardAndVoucherLinkOnMegaMenu"] = "Show Link On Mega Menu",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableGiftCardAndVoucherLinkOnMegaMenu.Hint"] = "Show gift card and Electrosales voucher link on mega menu",

                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireRecipientEmailZimazon"] = "Require Recipient Email for Zimazon Voucher",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireRecipientEmailZimazon.Hint"] = "Recipient Email will mandatory for Zimazon voucher if it's required",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireRecipientEmailElectrosales"] = "Require Recipient Email for Electrosales Voucher",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireRecipientEmailElectrosales.Hint"] = "Recipient Email will mandatory for Electrosales voucher if it's required",

                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnablePhysicalAddressZimazon"] = "Enable Physical Address Field For Zimazon",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnablePhysicalAddressZimazon.Hint"] = "EnablePhysicalAddressField is use for enable or disable 'Physical Address' in product details page",

                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnablePhysicalAddressElectrosales"] = "Enable Physical Address Field For Electrosales",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnablePhysicalAddressElectrosales.Hint"] = "EnablePhysicalAddressField is use for enable or disable 'Physical Address' in product details page",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequirePhysicalAddressZimazon"] = "Required Physical Address for Zimazon Voucher",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequirePhysicalAddressZimazon.Hint"] = "Physical Address will mandatory for Zimazon voucher if it's required",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequirePhysicalAddressElectrosales"] = "Required Physical Address for Electrosales",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequirePhysicalAddressElectrosales.Hint"] = "Physical Address will mandatory for Electrosales if it's required",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableCellPhoneNumberZimazon"] = "Enable Cell Phone Number Field For Zimazon",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableCellPhoneNumberZimazon.Hint"] = "EnableCellPhoneNumberField is use for enable or disable 'Cell Phone Number' in product details page",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableCellPhoneNumberElectrosales"] = "Enable Cell Phone Number Field For Electrosales",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableCellPhoneNumberElectrosales.Hint"] = "EnableCellPhoneNumberField is use for enable or disable 'Cell Phone Number' in product details page",

                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireCellPhoneNumberZimazon"] = "Required Cell Phone Number for Zimazon",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireCellPhoneNumberZimazon.Hint"] = "Cell Phone Number will mandatory for Zimazon if it's required",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireCellPhoneNumberElectrosales"] = "Required Cell Phone Number for Electrosales",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireCellPhoneNumberElectrosales.Hint"] = "Cell Phone Number will mandatory for Electrosales if it's required",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableIdOrPassportNumberZimazon"] = "Enable Id Or Passport Number Field For Zimazon",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableIdOrPassportNumberZimazon.Hint"] = "EnableIdOrPassportNumberField is use for enable or disable 'Id Or Passport Number' in product details page",

                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableIdOrPassportNumberElectrosales"] = "Enable Id Or Passport Number Field For Electrosales",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableIdOrPassportNumberElectrosales.Hint"] = "EnableIdOrPassportNumberField is use for enable or disable 'Id Or Passport Number' in product details page",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireIdOrPassportNumberZimazon"] = "Required Id Or Passport Number for Zimazon",

                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireIdOrPassportNumberZimazon.Hint"] = "Id Or Passport Number will mandatory for Zimazon if it's required",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireIdOrPassportNumberElectrosales"] = "Required Id Or Passport Number for Electrosales",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireIdOrPassportNumberElectrosales.Hint"] = "Id Or Passport Number will mandatory for Electrosales if it's required",

                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ZimazonGiftProductSku"] = "Zimazon Gift Card Product SKU",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ZimazonGiftProductSku.Hint"] = "Selected product link will appear in the home page menu",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ElectrosalesGiftProductSku"] = "Electrosales Credit Voucher Product SKU",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ElectrosalesGiftProductSku.Hint"] = "Selected product link will appear in the home page menu",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.FirstName"] = "Recipient First Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.LastName"] = "Recipient Last Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.RecipientEmail"] = "Recipient Email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.SenderName"] = "Sender Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.SenderEmail"] = "Sender Email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Message"] = "Message",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.PhysicalAddress"] = "Physical Address",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.CellPhoneNumber"] = "Recipient Cell Phone Number",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.IdOrPassportNumber"] = "ID or Passport Number",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.FirstNameError"] = "Enter valid first name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.LastNameError"] = "Enter valid last name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.RecipientEmailError"] = "Enter valid recipient email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.SenderNameError"] = "Enter valid sender name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.SenderEmailError"] = "Enter valid sender email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.PhysicalAddressError"] = "Enter valid address",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.CellPhoneNumberError"] = "Enter valid phone number",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.IdOrPassportNumberError"] = "Enter valid id or passport number",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Zimazon.DefaultMessage"] = "Enjoy shopping on Zimazon.co.uk!",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Electrosales.DefaultMessage"] = "Enjoy shopping in all Electrosales shops!",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ZimazonGiftCardAvaiableAmounts"] = "Available Amount For Zimazon Gift Card",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ZimazonGiftCardAvaiableAmounts.Hint"] = "Put comma(,) separated value for availble option to choose amount in add to cart section",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ElectrosalesGiftVoucherAvaiableAmounts"] = "Available Amount For Electrosales Credit Voucher",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ElectrosalesGiftVoucherAvaiableAmounts.Hint"] = "Put comma(,) separated value for availble option to choose amount in add to cart section",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Zimazon.DeliveryDateMessage"] = "Up to a year from today!",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Electrosales.DeliveryDateMessage"] = "Up to 90 days from today!",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Electrosales.ChangeCurrencyToUsdMessage"] = "Electrosales Credit Vouchers can be purchased in US Dollars only. Please change to US Dollars",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.ShoppingCart.CurrencyError"] = "Electrosales Credit Vouchers only available in US Dollars",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Electrosales.ChangeCurrencyButtonText"] = "Change to USD",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Electrosales.ElectrosalesCurrencyInformation"] = "While you have Electrosales Credit Voucher in your cart all your transactions will be in USD only!",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Account.Fields.GiftCardNumber"] = "Gift Card Number",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Account.Fields.CouponCode"] = "Coupon Code",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Account.Fields.TotalAmount"] = "Total Amount",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Account.Fields.RemainingAmount"] = "Remaining Amount",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Account.NoGiftCards"] = "No Gift Cards",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Account.GiftCards"] = "Gift Cards",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Name"] = "Zimzone Feature",
                ["Plugins.Misc.ZimzoneAdditionalFeature.GiftVoucher.Configure"] = "Gift Voucher Configure",
                ["Plugins.Misc.ZimzoneAdditionalFeature.ZimzoneServiceList"] = "Zimzone Service List",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.SearchIsActive"] = "Search Active",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.SearchServiceName"] = "Service Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.SearchServiceName.Hint"] = "Search by service name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.ZimzoneServicePaymentProductName"] = "Payment Product",
                ["Plugins.Misc.ZimzoneAdditionalFeature.ZimzoneServiceProductName"] = "Service Product",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Create"] = "Create",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Services.Requests"] = "Service Requests",

                ["Plugins.Misc.ZimzoneAdditionalFeature.ServiceProduct.NotSelectedError"] = "Please choose a service product",
                ["Plugins.Misc.ZimzoneAdditionalFeature.PaymentProduct.NotSelectedError"] = "Please choose a service payment product",
                ["Plugins.Misc.ZimzoneAdditionalFeature.ZimzoneServiceRequestList"] = "Service Request List",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.IsAgreed"] = "Is Agreed",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.AgreedAmount"] = "Agreed Amount",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CreatedOn"] = "Submitted On",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CreatedOn.Hint"] = "request submitted on date time",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.AgreedAmount.Hint"] = "amount to provide service",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.ServiceName"] = "Service Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.ServiceName.Hint"] = "requested service Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.OrderId"] = "Service Payment",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.OrderId.Hint"] = "Payd by order reference",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomerName"] = "Customer Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomerName.Hint"] = "requested customer Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.Description"] = "Service Description",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.Description.Hint"] = "service request description",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.DownloadGuid"] = "Uploaded Files",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.DownloadGuid.Hint"] = "Submitted file",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.Status"] = "Status",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomerEmail"] = "Request From",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomerEmail.Hint"] = "customer email",

                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.CustomerEmail"] = "Customer Email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.CustomerEmail.Hint"] = "Search by customer email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.CustomerName"] = "Customer Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.CustomerName.Hint"] = "Search by customer name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.ServiceId"] = "Service",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.ServiceId.Hint"] = "Search by service",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.StatusId"] = "Status",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ServiceRequest.Search.StatusId.Hint"] = "Search by status",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin..ViewOrder"] = "View Order",

                ["Plugins.Misc.ZimzoneAdditionalFeature.Request.Sumbmit.Text"] = "Submit Request",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Request.Submitted"] = "Your service request submitted",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Name"] = "Name Attribute",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Name.Hint"] = "Select attribute for name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Email"] = "Email Attribute",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Email.Hint"] = "Select attribute for email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Address"] = "Address Attribute",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Address.Hint"] = "Select attribute for address",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Description"] = "Description Attribute",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.Description.Hint"] = "Select attribute for description",

                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomerAddress"] = "Address",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomerAddress.Hint"] = "Address provided by the customer for service",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.File"] = "File Attribute",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Attribute.File.Hint"] = "Select attribute for file",
                ["Plugins.Misc.ZimzoneAdditionalFeature.ServiceAttribute.Configure"] = "Service Attribute Configuration",
                ["Plugins.Misc.ZimzoneAdditionalFeature.EditService"] = "Edit Service",
                ["Plugins.Misc.ZimzoneAdditionalFeature.EditRequest"] = "Edit Service Request",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Request.BackToList"] = "Back To Service Request List",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Pending"] = "Pending",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Accepted"] = "Accepted",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Status.Paid"] = "Paid",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.SaveAndAccept"] = "Save and accept request",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Account.ServiceRequests"] = "Service Requests",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Account.ViewYourRequestButton.Text"] = "View Requests",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Account.ContinueShopping.Text"] = "Continue Shopping",
                ["Plugins.Misc.ZimzoneAdditionalFeature.PayNow"] = "Pay Now",
                ["Plugins.Misc.ZimzoneAdditionalFeature.RequestDetails"] = "Request Details",
                ["Plugins.Misc.ZimzoneAdditionalFeature.PaymentPending"] = "Pending",
                ["Plugins.Misc.ZimzoneAdditionalFeature.NoRequests"] = "No service request",
                ["Plugins.Misc.ZimzoneAdditionalFeature.IsActive"] = "Is Active",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Service.Requests"] = "Service Requests",
                ["Plugins.Misc.ZimzoneAdditionalFeature.ServiceRequestInformation"] = "Service Request Information",
                ["Plugins.Misc.ZimzoneAdditionalFeature.ServiceName"] = "Service Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.RequestId"] = "Request Number",
                ["Plugins.Misc.ZimzoneAdditionalFeature.CreatedOn"] = "Submitted On",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Status"] = "Request Status",
                ["Plugins.Misc.ZimzoneAdditionalFeature.AgreedAmount"] = "Accepted Amount",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Account.GoToLogIn.Text"] = "Log in to view requests",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.QuoteDownloadId"] = "Quote File",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.QuoteDownloadId.Hint"] = "Last quote file",
                ["Plugins.Misc.ZimzoneAdditionalFeature.QuoteFileMeesage"] = "See the attched document.",


                //Query
                ["Plugins.Misc.ZimzoneAdditionalFeature.QureryNode"] = "Customer Queries",
                ["Plugins.Misc.ZimzoneAdditionalFeature.QueryList"] = "Query List",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Field.FullName"] = "Customer Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Field.Email"] = "Customer Email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Field.MarkedAsRead"] = "Reviewed",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Field.ProductName"] = "Product Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.QueryNotFoundError"] = "Query not found",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.FirstName"] = "First Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.LastName"] = "Last Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.Email"] = "Email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.Email.Hint"] = "customer email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.PhoneNumber"] = "Phone Number",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.PhoneNumber.Hint"] = "customer phone number",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.ProductName"] = "Product/Service Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.ProductName.Hint"] = "customer looking for",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.ProductDescription"] = "Product/Service Description",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.ProductDescription.hint"] = "Product Description",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.Message"] = "Message",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.Message.Hint"] = "Message from customer",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.AdditionalLink"] = "Reference link if you have any",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.AdditionalLink.Hint"] = "provided additional link",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.DownloadGuid"] = "Attachment",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.DownloadGuid.Hint"] = "provided attchment",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.AdminComment"] = "Admin Comment",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.AdminComment.Hint"] = "admin comment for internal reference",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.MarkedAsRead"] = "Mark as read",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.MarkedAsRead.Hint"] = "mark as reviewed",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.MarkedBy"] = "Reviewed By",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.MarkedBy.Hint"] = "Reviewd by admin name",

                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Search.CustomerEmail"] = "Customer Email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Search.CustomerEmail.Hint"] = "search by customer email",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Search.CustomerName"] = "Customer Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Search.CustomerName.Hint"] = "search by customer name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Search.StatusId"] = "Status",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.Search.StatusId.Hint"] = "search by status",

                ["Plugins.Misc.ZimzoneAdditionalFeature.PageTitle.SubmitQuestion"] = "Can't Find",
                ["Plugins.Misc.ZimzoneAdditionalFeature.SubmitQuery.Button"] = "Submit",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.YourEnquiryHasBeenSent"] = "Your query has been received",
                ["Plugins.Misc.ZimzoneAdditionalFeature.EditQuery"] = "Edit Query",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Question.BackToList"] = "Back To List",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.FullName"] = "Customer Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.FullName.Hint"] = "submitted by",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.FirstName.Required"] = "First name is required",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.LastName.Required"] = "Last name is required",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.Email.Required"] = "Email is required",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.PhoneNumber.Required"] = "Phone number is required",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.Message.Required"] = "Message is required",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.UploadFile.Text"] = "Upload screenshot or additional file",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVouchers.PageTitle"] = "Electrosales Credit Vouchers",


                ["Plugins.Misc.ZimzoneAdditionalFeature.ServiceRequest.LoginMessage"] = "Log in to submit service request",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Tax.TaxMessage"] = "Zimbabwe VAT included",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Vat"] = "VAT",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Tax.NoVatOnElectrosales"] = "Electrosales Credit Voucher has no VAT obligation ",
                ["Plugins.Misc.ZimzoneAdditionalFeature.RemoveGiftCardButtonMessage"] = "Remove Gift Card",
                ["Plugins.Misc.ZimzoneAdditionalFeature.ShipmentPackageSlipSize.A5"] = "A5",
                ["Plugins.Misc.ZimzoneAdditionalFeature.ShipmentPackageSlipSize.A5Landscape"] = "A5 Landscape",
                ["Plugins.Misc.ZimzoneAdditionalFeature.ShipmentPackageSlipSize.A6"] = "A6",
                ["Plugins.Misc.ZimzoneAdditionalFeature.ShipmentPackageSlipSize.A6Landscape"] = "A6 Landscape",
                ["Plugins.Misc.ZimzoneAdditionalFeature.QuoteFileDownload.DownloadText"] = "Click here to download/review your final quotation",
                ["Plugins.Misc.ZimzoneAdditionalFeature.CustomOrderStatusNode"] = "Custom Order Status Configuration",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.CustomOrderStatusName"] = "Custom Order Status",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.CustomOrderStatusName.Hint"] = "Custom Order Status to be added",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.DisplayOrder"] = "Display Order",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.DisplayOrder.Hint"] = "Display Order",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ParentOrderStatusId"] = "Parent Order Status",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ParentOrderStatusId.Hint"] = "Parent Order Status of the custom one",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ParentOrderStatus"] = "Parent Order Status",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.IsMarkedToSendEmail"] = "Notify Customer",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.IsMarkedToSendEmail.Hint"] = "Notify customer on changing custom status",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.CustomOrderStatus.PageTitle"] = "Custom Order Status Configuration",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.CustomOrderStatusInsert.PageTitle"] = "Insert Custom Order Status",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.CustomOrderStatusInsert.Errors"] = "Use Unique custom order status name under this parent order status",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.CustomOrderStatusId"] = "Custom Order Status",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.CustomOrderStatusId.Hint"] = "Custom Order Status",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.CustomOrderStatusId.NotSelectError"] = "Select Valid One",
                ["Plugins.Misc.ZimzoneAdditionalFeature.OrderWithCustomStatusNode"] = "Orders",
                ["Plugins.Misc.ZimzoneAdditionalFeature.OrderItemPdfDownload.DownloadButton"] = "Download Order Info",
                ["Plugins.Misc.ZimzoneAdditionalFeature.OrderItemPdfDownload.Admin"] = "Electrosales",
                ["Plugins.Misc.ZimzoneAdditionalFeature.OrderItemPdfDownload.ProductCost"] = "Product Cost(Per unit)",



                ["Plugins.Misc.ZimzoneAdditionalFeature.UpdatePriceAndStock.Button.Text"] = "Update Price and Stock From Excel",
                ["Plugins.Misc.ZimzoneAdditionalFeature.UpdatePriceAndStock.Warning"] = "Excel should contains only SKU, Stock and Price column",
                ["Plugins.Misc.ZimzoneAdditionalFeature.UpdatePriceAndStock.Success"] = "Price and stock updated",
                ["Plugins.Misc.ZimzoneAdditionalFeature.UpdtatePriceAndStock.NodeTitle"] = "Update Price & Stock",
                ["Plugins.Misc.ZimzoneAdditionalFeature.UpdtatePriceAndStock.Sku"] = "Sku",
                ["Plugins.Misc.ZimzoneAdditionalFeature.UpdtatePriceAndStock.Price"] = "Price",
                ["Plugins.Misc.ZimzoneAdditionalFeature.UpdtatePriceAndStock.Stock"] = "Stock",
                ["Plugins.Misc.ZimzoneAdditionalFeature.UpdtatePriceAndStock.ErrorMessage"] = "Error Message",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVoucher.Fields.PhoneNumber"] = "Phone Number",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVoucher.Fields.PhoneNumber.Hint"] = "Cell Phone Number",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVoucher.Delete.Success"] = "The electrosales voucher has been deleted successfully.",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVoucher.Update.Success"] = "The electrosales voucher has been updated successfully.",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVoucher.Edit"] = "Edit electrosales voucher details",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVoucher.BackToVoucherList"] = "back to electrosales voucher list",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.ElectrosalesVoucher.Info.TabName"] = "Electrosales Voucher Info",


                ["Plugins.Misc.ZimzoneAdditionalFeature.ShipmentItemPdfDownload.DownloadButton"] = "Download Shipment Item Info",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.CreatedOn"] = "Created On",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Question.CreatedOn.Hint"] = "Created On",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomName"] = "Custom Name",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Field.CustomName.Hint"] = "Custom Name for the service request",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Cart.Column.QuoteFile"] = "Quotation",
                ["Plugins.Misc.ErpSync.ManufacturerSyncUrl"] = "Manufacturer Sync Url",
                ["Plugins.Misc.ErpSync.ManufacturerSyncUrl.Hint"] = "Manufacturer Sync Url",


                ["Plugins.Misc.ZimzoneAdditionalFeature.Product.Search.NoResult"] = "PRODUCT NOT FOUND!",

                //full erp sync schedule task
                ["Plugins.Misc.ErpSync.ManualSyncFrom"] = "Manual Sync From",
                ["Plugins.Misc.ErpSync.ManualSyncFrom.Hint"] = "Set Date From when Manual Sync should be run",
                
                // search field for out of stock products - 17-10-22
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.SearchStockOptionId"] = "Stock Option",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.SearchStockOptionId.Hint"] = "Choose stock type - in stock/out of stock",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.StockOptions.InStock"] = "In Stock",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Admin.StockOptions.OutOfStock"] = "Out of Stock",

                // sync time in erp sync configure - 18-10-2022
                ["Plugins.Misc.ErpSync.SyncTime"] = "Synchronization Time",
                ["Plugins.Misc.ErpSync.SyncTime.Hint"] = "Synchronization Time in format HH:MM. Example: 23:05, 02:32, 00:15",

                // request time out for erp sync - 19-10-2022
                ["Plugins.Misc.ErpSync.RequestTimeOutInSeconds"] = "Request Timeout",
                ["Plugins.Misc.ErpSync.RequestTimeOutInSeconds.Hint"] = "Request Timeout for erp product sync in seconds",

                // pickup point selection validation - 01-11-2022
                ["Plugins.Misc.ZimzoneAdditionalFeature.Checkout.PickupPoint.NotSelected"] = "You must select a pickup point",
                // shipment create admin notification table column - 11-11-22
                ["Messages.Order.Product(s).Vendor"] = "Vendor",
                ["Plugins.Misc.ZimzoneAdditionalFeature.Messages.Order.Product(s).VendorAdmin"] = "Electrosales",
            });

            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Misc.ErpSync.Configure"] = "ERP Sync Configure",
                ["Plugins.Misc.ErpSync.ProductSyncUrl"] = "Product Sync Url",
                ["Plugins.Misc.ErpSync.ProductSyncUrl.Hint"] = "API endpoint to get products",
                ["Plugins.Misc.ErpSync.CategorySyncUrl"] = "Category Sync Url",
                ["Plugins.Misc.ErpSync.CategorySyncUrl.Hint"] = "API endpoint to get categories",
                ["Plugins.Misc.ErpSync.StockSyncUrl"] = "Stock Sync Url",
                ["Plugins.Misc.ErpSync.StockSyncUrl.Hint"] = "API endpoint to get products stock",
                ["Plugins.Misc.ErpSync.BufferTime"] = "Buffer Time",
                ["Plugins.Misc.ErpSync.BufferTime.Hint"] = "",
                ["Plugins.Misc.ErpSync.ImageUrlEndpoint"] = "Image Url Endpoint",
                ["Plugins.Misc.ErpSync.ImageUrlEndpoint.Hint"] = "if the image path is 'https://ik.imagekit.io/demo/medium_cafe_B1iTdD0C.jpg' then the endpoint is 'https://ik.imagekit.io/demo/'. To know more visit 'https://docs.imagekit.io/features/image-transformations'",
            });
        }

        async Task AddTemplatesAsync()
        {
            //product template
            var template = new ProductTemplate()
            {
                Name = "Simple Service",
                DisplayOrder = 10,
                IgnoredProductTypes = "10",
                ViewPath = "ServiceTemplate.Simple"
            };
            await _productTemplateService.InsertProductTemplateAsync(template);

            //message template
            var requestSubmittedEmailTemplate = new MessageTemplate
            {
                Name = ServiceRequestTemplateSystemNames.ServiceRequestSubmitted,

                Subject = "%Store.Name%. %ServiceRequest.Name% request submitted",
                Body = $"{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"Hello %ServiceRequest.CustomerName%,{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"We have received your %ServiceRequest.Name% request. We will get back to you after reviewing your service request." +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"Below is the summary of the service request.{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"Request Number: %ServiceRequest.ReqestNumber%{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"Date submited: %ServiceRequest.CreatedOn%{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"Request Description: {Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"%ServiceRequest.Description%" +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"Address: {Environment.NewLine}" +
                $"%ServiceRequest.CustomerAddress%" +
                $"<br />{Environment.NewLine}",
                IsActive = true,
                //EmailAccountId = eaGeneral.Id
            };
            await _messageTemplateService.InsertMessageTemplateAsync(requestSubmittedEmailTemplate);
            var requestAcceptedEmailTemplate = new MessageTemplate
            {
                Name = ServiceRequestTemplateSystemNames.ServiceRequestAccepted,
                Subject = "%Store.Name%. %ServiceRequest.Name% request accepted",
                Body = $"{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"Hello %ServiceRequest.CustomerName%,{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"We have accepted your %ServiceRequest.Name% request. We have added the service in your <a href=\"%Store.URL%service-cart/\"target=\"_blank\">cart</a>. Please complete the payment." +
                $"Below is the summary of the service request.{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"Request Number: %ServiceRequest.ReqestNumber%{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"Date submited: %ServiceRequest.CreatedOn%{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"Request Description: {Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"%ServiceRequest.Description%" +
                $"<br />{Environment.NewLine}" +
                $"<br />{Environment.NewLine}" +
                $"Address: {Environment.NewLine}" +
                $"%ServiceRequest.CustomerAddress%" +
                $"<br />{Environment.NewLine}" +
                $"%if(!string.IsNullOrEmpty(%ServiceRequest.QuoteDownLoadInfo%))<a href=\"%ServiceRequest.QuoteDownLoadInfo%/\"target = \"_blank\" >Download Quote File</a>endif% ",
                IsActive = true,
                //EmailAccountId = eaGeneral.Id
            };
            await _messageTemplateService.InsertMessageTemplateAsync(requestAcceptedEmailTemplate);


            var querySubmittedCustomerEmailTemplate = new MessageTemplate
            {
                Name = ServiceRequestTemplateSystemNames.QuerySubmittedCustomerNotification,

                Subject = "Could not find it on %Store.Name% query received",
                Body = $"{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}" +
                    $"<br />{Environment.NewLine}" +
                    $"<br />{Environment.NewLine}" +
                    $"Hello %Query.FirstName%,{Environment.NewLine}" +
                    $"<br />{Environment.NewLine}" +
                    $"We have received your query. We will get back to you after reviewing your query.{Environment.NewLine}" +
                    $"<br />{Environment.NewLine}",

                IsActive = true,
                //EmailAccountId = eaGeneral.Id
            };
            await _messageTemplateService.InsertMessageTemplateAsync(querySubmittedCustomerEmailTemplate);

            var querySubmittedAdminEmailTemplate = new MessageTemplate
            {
                Name = ServiceRequestTemplateSystemNames.QuerySubmittedAdminNotification,

                Subject = "Could not find it on %Store.Name% query received",
                Body = $"{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}" +
                    $"<br />{Environment.NewLine}" +
                    $"<br />{Environment.NewLine}" +
                    $"Hello, %Query.FirstName%,{Environment.NewLine}" +
                    $"<br />{Environment.NewLine}" +
                    $"We have received your query. We will get back to you after reviewing your query.",

                IsActive = true,
                //EmailAccountId = eaGeneral.Id
            };
            await _messageTemplateService.InsertMessageTemplateAsync(querySubmittedAdminEmailTemplate);

            var customOrderStatusEmailTemplate = new MessageTemplate()
            {
                Name = CustomOrderStatusDefaults.MessageTemplateName,
                Subject = "Order receipt from %Store.Name%.",
                Body = $"{Environment.NewLine}Order ID: %Order.OrderNumber%{Environment.NewLine}" +
                    $"Date Ordered: %Order.CreatedOn%<br />{Environment.NewLine}" +
                    $"<br />{Environment.NewLine}" +
                    $"Your Order has been updated to the following status:<br />{Environment.NewLine}" +
                    $"%Order.CustomOrderStatusMessage%<br />{Environment.NewLine}" +
                    $"To view your order click on the link below:,{Environment.NewLine}" +
                    $"<a href=\"%Order.OrderURLForCustomer%\"><br />{Environment.NewLine}" +
                    $"<br />{Environment.NewLine}" +
                    $"Please reply to this email if you have any questions.",
                IsActive = true
            };

            await _messageTemplateService.InsertMessageTemplateAsync(customOrderStatusEmailTemplate);

            var serviceSettings = new ServiceSettings
            {
                ProductTemplateId = template.Id,
                RequestSubmittedEmailTemplateId = requestSubmittedEmailTemplate.Id,
                RequestAcceptedEmailTemplateId = requestAcceptedEmailTemplate.Id
            };
            await _settingService.SaveSettingAsync(serviceSettings);
        }
        #endregion

        public async Task InstallPermissionsRecordsAndMappingsAsync()
        {
            var permissionService = EngineContext.Current.Resolve<IPermissionService>();

            try
            {
                await permissionService.InstallPermissionsAsync(new ZimZonePermissionProvider());
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Zimzone Additional Feature: Could not install permissions. ", ex);
            }
        }

        public async Task UninstallPermissionsRecordsAndMappingsAsync()
        {
            var permissionService = EngineContext.Current.Resolve<IPermissionService>();

            try
            {
                //No uninstall method found
                //await permissionService.un(new B2BPermissionProvider());
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Zimzone Additional Feature: Could not uninstall permissions. ", ex);
            }
        }

        public async Task RunInstallationScriptsAsync()
        {
            try
            {
                var sqlScrpts = new List<string>();
                var sqlFile = _nopFileProvider.MapPath(ScriptPaths.ImportTablesCreateSqlFilePath);
                var sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
                sqlScrpts.Add(sqlScript);

                sqlFile = _nopFileProvider.MapPath(ScriptPaths.ImportStoredProceduresCreateSqlFilePath);
                sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
                sqlScrpts.Add(sqlScript);

                sqlFile = _nopFileProvider.MapPath(ScriptPaths.CustomOrderStatusCreateSqlFilePath);
                sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
                sqlScrpts.Add(sqlScript);

                sqlFile = _nopFileProvider.MapPath(ScriptPaths.OrderWithCustomStatusCreateSqlFilePath);
                sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
                sqlScrpts.Add(sqlScript);

                //manufacturer sync table
                sqlFile = _nopFileProvider.MapPath(ScriptPaths.ErpManufacturerCreateSqlFilePath);
                sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
                sqlScrpts.Add(sqlScript);

                await RunSqlsAsync(sqlScrpts);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error while parsing script from file", ex);
            }
        }

        public async Task RunUninstallationScriptsAsync()
        {
            try
            {
                var sqlScrpts = new List<string>();
                var sqlFile = _nopFileProvider.MapPath(ScriptPaths.ImportTablesDropSqlFilePath);
                var sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
                sqlScrpts.Add(sqlScript);

                sqlFile = _nopFileProvider.MapPath(ScriptPaths.ImportStoredProceduresDropSqlFilePath);
                sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
                sqlScrpts.Add(sqlScript);

                sqlFile = _nopFileProvider.MapPath(ScriptPaths.CustomOrderStatusDropSqlFilePath);
                sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
                sqlScrpts.Add(sqlScript);

                sqlFile = _nopFileProvider.MapPath(ScriptPaths.OrderWithCustomStatusDropSqlFilePath);
                sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
                sqlScrpts.Add(sqlScript);

                //manufacturer sync table
                sqlFile = _nopFileProvider.MapPath(ScriptPaths.ErpManufacturerDropSqlFilePath);
                sqlScript = await _nopFileProvider.ReadAllTextAsync(sqlFile, Encoding.UTF8);
                sqlScrpts.Add(sqlScript);

                await RunSqlsAsync(sqlScrpts);
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "Error while parsing script from file", ex.Message);
            }
        }


        public async Task RunSqlsAsync(IList<string> sqlScripts)
        {
            var connString = Nop.Data.DataSettingsManager.LoadSettings().ConnectionString;
            using (var connection = new SqlConnection(connString))
            {
                connection.Open();
                try
                {
                    foreach (var script in sqlScripts)
                    {
                        var query = script;
                        var cmd = new SqlCommand(query, connection);
                        var rowChanged = cmd.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, "Error while runnig sql script", ex.Message);
                    connection.Close();
                }
            }

        }

    }
}
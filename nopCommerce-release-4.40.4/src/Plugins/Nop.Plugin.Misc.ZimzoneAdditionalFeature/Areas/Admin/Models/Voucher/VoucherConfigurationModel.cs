using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models
{
    public record VoucherConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableGiftCardAndVoucherLinkOnMegaMenu")]
        public bool EnableGiftCardAndVoucherLinkOnMegaMenu { get; set; }
        public bool EnableGiftCardAndVoucherLinkOnMegaMenu_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ZimazonGiftProductSku")]
        public string ZimazonGiftProductSku { get; set; }
        public bool ZimazonGiftProductSku_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ElectrosalesGiftProductSku")]
        public string ElectrosalesGiftProductSku { get; set; }
        public bool ElectrosalesGiftProductSku_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ZimazonGiftCardAvaiableAmounts")]
        public string ZimazonGiftCardAvaiableAmounts { get; set; }
        public bool ZimazonGiftCardAvaiableAmounts_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.ElectrosalesGiftVoucherAvaiableAmounts")]
        public string ElectrosalesGiftVoucherAvaiableAmounts { get; set; }
        public bool ElectrosalesGiftVoucherAvaiableAmounts_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnablePhysicalAddressZimazon")]
        public bool EnablePhysicalAddressZimazon { get; set; }
        public bool EnablePhysicalAddressZimazon_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnablePhysicalAddressElectrosales")]
        public bool EnablePhysicalAddressElectrosales { get; set; }
        public bool EnablePhysicalAddressElectrosales_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableCellPhoneNumberZimazon")]
        public bool EnableCellPhoneNumberZimazon { get; set; }
        public bool EnableCellPhoneNumberZimazon_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableCellPhoneNumberElectrosales")]
        public bool EnableCellPhoneNumberElectrosales { get; set; }
        public bool EnableCellPhoneNumberElectrosales_OverrideForStore { get; set; }



        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableIdOrPassportNumberZimazon")]
        public bool EnableIdOrPassportNumberZimazon { get; set; }
        public bool EnableIdOrPassportNumberZimazon_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.EnableIdOrPassportNumberElectrosales")]
        public bool EnableIdOrPassportNumberElectrosales { get; set; }
        public bool EnableIdOrPassportNumberElectrosales_OverrideForStore { get; set; }



        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequirePhysicalAddressZimazon")]
        public bool RequirePhysicalAddressZimazon { get; set; }
        public bool RequirePhysicalAddressZimazon_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequirePhysicalAddressElectrosales")]
        public bool RequirePhysicalAddressElectrosales { get; set; }
        public bool RequirePhysicalAddressElectrosales_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireCellPhoneNumberZimazon")]
        public bool RequireCellPhoneNumberZimazon { get; set; }
        public bool RequireCellPhoneNumberZimazon_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireCellPhoneNumberElectrosales")]
        public bool RequireCellPhoneNumberElectrosales { get; set; }
        public bool RequireCellPhoneNumberElectrosales_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireIdOrPassportNumberZimazon")]
        public bool RequireIdOrPassportNumberZimazon { get; set; }
        public bool RequireIdOrPassportNumberZimazon_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireIdOrPassportNumberElectrosales")]
        public bool RequireIdOrPassportNumberElectrosales { get; set; }
        public bool RequireIdOrPassportNumberElectrosales_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireRecipientEmailZimazon")]
        public bool RequireRecipientEmailZimazon { get; set; }
        public bool RequireRecipientEmailZimazon_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Admin.Fields.RequireRecipientEmailElectrosales")]
        public bool RequireRecipientEmailElectrosales { get; set; }
        public bool RequireRecipientEmailElectrosales_OverrideForStore { get; set; }

    }
}

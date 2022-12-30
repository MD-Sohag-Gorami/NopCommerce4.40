using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature
{
    public class GiftVoucherSettings : ISettings
    {
        public bool EnableGiftCardAndVoucherLinkOnMegaMenu { get; set; }
        public bool EnablePhysicalAddressZimazon { get; set; }
        public bool EnablePhysicalAddressElectrosales { get; set; }
        public bool RequirePhysicalAddressZimazon { get; set; }
        public bool RequirePhysicalAddressElectrosales { get; set; }

        public bool EnableCellPhoneNumberZimazon { get; set; }
        public bool EnableCellPhoneNumberElectrosales { get; set; }
        public bool RequireCellPhoneNumberZimazon { get; set; }
        public bool RequireCellPhoneNumberElectrosales { get; set; }

        public bool EnableIdOrPassportNumberZimazon { get; set; }
        public bool EnableIdOrPassportNumberElectrosales { get; set; }
        public bool RequireIdOrPassportNumberZimazon { get; set; }
        public bool RequireIdOrPassportNumberElectrosales { get; set; }

        public bool RequireRecipientEmailZimazon { get; set; }
        public bool RequireRecipientEmailElectrosales { get; set; }

        public string ZimazonGiftProductSku { get; set; }
        public string ElectrosalesGiftProductSku { get; set; }

        public string ZimazonGiftCardAvaiableAmounts { get; set; }
        public string ElectrosalesGiftVoucherAvaiableAmounts { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Catalog
{
    public partial record GiftCardAdditionalDataModel
    {
        public bool IsGiftCard { get; set; }

        public int ProductId { get; set; }

        public string ProductSku { get; set; }

        public GiftCardType GiftCardType { get; set; }
        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.FirstName")]
        public string FirstName { get; set; }
        public bool IsRequiredFirstName { get; set; }
        public bool IsEnabledFirstName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.LastName")]
        public string LastName { get; set; }
        public bool IsRequiredLastName { get; set; }
        public bool IsEnabledLastName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.RecipientEmail")]
        [DataType(DataType.EmailAddress)]
        public string RecipientEmail { get; set; }
        public bool IsRequiredRecipientEmail { get; set; }
        public bool IsEnabledRecipientEmail { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.SenderName")]
        public string SenderName { get; set; }
        public bool IsRequiredSenderName { get; set; }
        public bool IsEnabledSenderName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.SenderEmail")]
        [DataType(DataType.EmailAddress)]
        public string SenderEmail { get; set; }
        public bool IsRequiredSenderEmail { get; set; }
        public bool IsEnabledSenderEmail { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.Message")]
        public string Message { get; set; }
        public bool IsRequiredMessage { get; set; }
        public bool IsEnabledMessage { get; set; }

        public DateTime? GiftCardDeliveryDate { get; set; }
        public bool IsRequiredGiftCardDeliveryDate { get; set; }
        public bool IsEnabledGiftCardDeliveryDate { get; set; }
        public string DeliveryDateMessage { get; set; }
        public int ValidUptoInDays { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.PhysicalAddress")]
        public string PhysicalAddress { get; set; }
        public bool IsRequiredPhysicalAddress { get; set; }
        public bool IsEnabledPhysicalAddress { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.CellPhoneNumber")]
        public string CellPhoneNumber { get; set; }
        public bool IsRequiredCellPhoneNumber { get; set; }
        public bool IsEnabledCellPhoneNumber { get; set; }

        [NopResourceDisplayName("Plugins.Misc.ZimzoneAdditionalFeature.Products.GiftCard.IdOrPassportNumber")]
        public string IdOrPassportNumber { get; set; }
        public bool IsRequiredIdOrPassportNumber { get; set; }
        public bool IsEnabledIdOrPassportNumber { get; set; }

        public Dictionary<string, int> ButtonsValue { get; set; }

        public bool HasValidCurrency { get; set; }
        public string ElectrosalesVoucherCurrencyErrorMessage { get; set; }
    }
}

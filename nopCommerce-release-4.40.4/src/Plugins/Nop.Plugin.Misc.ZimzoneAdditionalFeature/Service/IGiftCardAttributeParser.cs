using System;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public interface IGiftCardAttributeParser
    {
        public void GetGiftCardAttribute(string attributesXml, out string firstName, out string lastName,
            out string recipientEmail, out string senderName, out string senderEmail, out string giftCardMessage,
            out string physicalAddress, out string cellPhoneNumber, out string idOrPassportNumber, out DateTime? giftcardDeliveryDate);
    }
}

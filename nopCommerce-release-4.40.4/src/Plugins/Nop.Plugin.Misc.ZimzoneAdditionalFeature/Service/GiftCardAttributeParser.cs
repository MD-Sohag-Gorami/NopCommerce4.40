using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public class GiftCardAttributeParser : IGiftCardAttributeParser
    {
        public void GetGiftCardAttribute(string attributesXml, out string firstName, out string lastName,
            out string recipientEmail, out string senderName, out string senderEmail, out string giftCardMessage,
            out string physicalAddress, out string cellPhoneNumber, out string idOrPassportNumber, out DateTime? giftcardDeliveryDate)
        {
            firstName = string.Empty;
            lastName = string.Empty;
            recipientEmail = string.Empty;
            senderName = string.Empty;
            senderEmail = string.Empty;
            giftCardMessage = string.Empty;
            physicalAddress = string.Empty;
            cellPhoneNumber = string.Empty;
            idOrPassportNumber = string.Empty;
            giftcardDeliveryDate = (DateTime?)null;
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var firstNameElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/FirstName");
                var lastNameElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/LastName");
                var recipientEmailElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/RecipientEmail");
                var senderNameElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/SenderName");
                var senderEmailElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/SenderEmail");
                var messageElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/Message");
                var physicalAddressElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/PhysicalAddress");
                var cellPhoneNumberElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/CellPhoneNumber");
                var idOrPassportNumberElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/IdOrPassportNumber");
                var giftCardDeliveryDateElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/GiftCardDeliveryDate");

                if (firstNameElement != null)
                    firstName = firstNameElement.InnerText;
                if (lastNameElement != null)
                    lastName = lastNameElement.InnerText;
                if (recipientEmailElement != null)
                    recipientEmail = recipientEmailElement.InnerText;
                if (senderNameElement != null)
                    senderName = senderNameElement.InnerText;
                if (senderEmailElement != null)
                    senderEmail = senderEmailElement.InnerText;
                if (messageElement != null)
                    giftCardMessage = messageElement.InnerText;
                if (physicalAddressElement != null)
                    physicalAddress = physicalAddressElement.InnerText;
                if (cellPhoneNumberElement != null)
                    cellPhoneNumber = cellPhoneNumberElement.InnerText;
                if (idOrPassportNumberElement != null)
                    idOrPassportNumber = idOrPassportNumberElement.InnerText;
                if (giftCardDeliveryDateElement != null)
                {
                    var date = DateTime.UtcNow;
                    if (DateTime.TryParseExact(giftCardDeliveryDateElement.InnerText, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    {
                        giftcardDeliveryDate = date;
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
        }
    }
}

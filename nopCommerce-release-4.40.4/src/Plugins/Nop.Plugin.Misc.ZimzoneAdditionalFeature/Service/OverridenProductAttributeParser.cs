using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public class OverridenProductAttributeParser : ProductAttributeParser
    {
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        public OverridenProductAttributeParser(ICurrencyService currencyService,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IProductAttributeService productAttributeService,
            IRepository<ProductAttributeValue> productAttributeValueRepository,
            IWorkContext workContext,
            ICustomerService customerService) : base(currencyService,
                downloadService,
                localizationService,
                productAttributeService,
                productAttributeValueRepository,
                workContext)
        {
            _workContext = workContext;
            _customerService = customerService;
        }
        public override async Task<string> ParseProductAttributesAsync(Product product, IFormCollection form, List<string> errors)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (form == null)
                throw new ArgumentNullException(nameof(form));

            //product attributes
            var attributesXml = await GetProductAttributesXmlAsync(product, form, errors);

            //gift cards
            OverridenAddGiftCardsAttributesXml(product, form, ref attributesXml);

            return attributesXml;
        }

        protected void OverridenAddGiftCardsAttributesXml(Product product, IFormCollection form, ref string attributesXml)
        {
            if (!product.IsGiftCard)
                return;

            var firstName = "";
            var lastName = "";
            var recipientEmail = "";
            var senderName = "";
            var senderEmail = "";
            var giftCardMessage = "";
            var physicalAddress = "";
            var cellPhoneNumber = "";
            var idOrPassportNumber = "";
            var deliveryDate = "";
            var customer = _workContext.GetCurrentCustomerAsync().Result;
            var isGuest = _customerService.IsGuestAsync(customer).Result;
            if (!isGuest)
            {
                senderEmail = customer.Email;
                senderName = _customerService.GetCustomerFullNameAsync(customer).Result;
            }
            foreach (var formKey in form.Keys)
            {
                if (formKey.Equals($"giftcard_{product.Id}.FirstName", StringComparison.InvariantCultureIgnoreCase))
                {
                    firstName = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.LastName", StringComparison.InvariantCultureIgnoreCase))
                {
                    lastName = form[formKey];
                    continue;
                }
                if (formKey.Equals($"giftcard_{product.Id}.RecipientEmail", StringComparison.InvariantCultureIgnoreCase))
                {
                    recipientEmail = form[formKey];
                    continue;
                }
                if (isGuest)
                {
                    if (formKey.Equals($"giftcard_{product.Id}.SenderName", StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals($"giftcard_{product.Id}.SenderEmail", StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderEmail = form[formKey];
                        continue;
                    }
                }

                if (formKey.Equals($"giftcard_{product.Id}.Message", StringComparison.InvariantCultureIgnoreCase))
                {
                    giftCardMessage = form[formKey];
                }
                if (formKey.Equals($"giftcard_{product.Id}.PhysicalAddress", StringComparison.InvariantCultureIgnoreCase))
                {
                    physicalAddress = form[formKey];
                }
                if (formKey.Equals($"giftcard_{product.Id}.CellPhoneNumber", StringComparison.InvariantCultureIgnoreCase))
                {
                    cellPhoneNumber = form[formKey];
                }
                if (formKey.Equals($"giftcard_{product.Id}.IdOrPassportNumber", StringComparison.InvariantCultureIgnoreCase))
                {
                    idOrPassportNumber = form[formKey];
                }
                if (formKey.Equals($"giftcard_{product.Id}.GiftCardDeliveryDate", StringComparison.InvariantCultureIgnoreCase))
                {
                    deliveryDate = form[formKey];
                }
            }

            attributesXml = AddGiftCardAttribute(attributesXml, firstName, lastName, recipientEmail,
                senderName, senderEmail, giftCardMessage, physicalAddress, cellPhoneNumber, idOrPassportNumber, deliveryDate);
        }

        public string AddGiftCardAttribute(string attributesXml, string firstName, string lastName,
            string recipientEmail, string senderName, string senderEmail, string giftCardMessage,
            string physicalAddress, string cellPhoneNumber, string idOrPassportNumber, string deliveryDate)
        {
            var result = string.Empty;
            try
            {
                firstName = firstName.Trim();
                lastName = lastName.Trim();
                recipientEmail = recipientEmail.Trim();
                senderName = senderName.Trim();
                senderEmail = senderEmail.Trim();
                physicalAddress = physicalAddress.Trim();
                cellPhoneNumber = cellPhoneNumber.Trim();
                idOrPassportNumber = idOrPassportNumber.Trim();

                var xmlDoc = new XmlDocument();
                if (string.IsNullOrEmpty(attributesXml))
                {
                    var element1 = xmlDoc.CreateElement("Attributes");
                    xmlDoc.AppendChild(element1);
                }
                else
                    xmlDoc.LoadXml(attributesXml);

                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

                var giftCardElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo");
                if (giftCardElement == null)
                {
                    giftCardElement = xmlDoc.CreateElement("GiftCardInfo");
                    rootElement.AppendChild(giftCardElement);
                }

                var firstNameElement = xmlDoc.CreateElement("FirstName");
                firstNameElement.InnerText = firstName;
                giftCardElement.AppendChild(firstNameElement);

                var lastNameElement = xmlDoc.CreateElement("LastName");
                lastNameElement.InnerText = lastName;
                giftCardElement.AppendChild(lastNameElement);

                var recipientEmailElement = xmlDoc.CreateElement("RecipientEmail");
                recipientEmailElement.InnerText = recipientEmail;
                giftCardElement.AppendChild(recipientEmailElement);

                var senderNameElement = xmlDoc.CreateElement("SenderName");
                senderNameElement.InnerText = senderName;
                giftCardElement.AppendChild(senderNameElement);

                var senderEmailElement = xmlDoc.CreateElement("SenderEmail");
                senderEmailElement.InnerText = senderEmail;
                giftCardElement.AppendChild(senderEmailElement);

                var messageElement = xmlDoc.CreateElement("Message");
                messageElement.InnerText = giftCardMessage;
                giftCardElement.AppendChild(messageElement);

                var physicalAddressElement = xmlDoc.CreateElement("PhysicalAddress");
                physicalAddressElement.InnerText = physicalAddress;
                giftCardElement.AppendChild(physicalAddressElement);

                var cellPhoneNumberElement = xmlDoc.CreateElement("CellPhoneNumber");
                cellPhoneNumberElement.InnerText = cellPhoneNumber;
                giftCardElement.AppendChild(cellPhoneNumberElement);

                var idOrPassportNumberElement = xmlDoc.CreateElement("IdOrPassportNumber");
                idOrPassportNumberElement.InnerText = idOrPassportNumber;
                giftCardElement.AppendChild(idOrPassportNumberElement);

                var deliveryDateElement = xmlDoc.CreateElement("GiftCardDeliveryDate");
                deliveryDateElement.InnerText = deliveryDate;
                giftCardElement.AppendChild(deliveryDateElement);
                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return result;
        }

    }
}

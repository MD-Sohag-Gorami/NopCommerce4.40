
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Infrastructure
{
    public class RouteProvider : BaseRouteProvider, IRouteProvider
    {

        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var lang = GetLanguageRoutePattern();

            endpointRouteBuilder.MapControllerRoute("Customer.GiftCards", "customer-gift-cards",
                new { controller = "CustomGiftCard", action = "List" });
            endpointRouteBuilder.MapControllerRoute("Customer.ServiceRequests", "service-requests",
                new { controller = "ServiceRequest", action = "ServiceRequestList" });
            endpointRouteBuilder.MapControllerRoute("Customer.ServiceRequestPayment", $"service-request-payment-{{requestId}}",
                new { controller = "ServiceRequest", action = "ServiceRequestPayment" });
            endpointRouteBuilder.MapControllerRoute("Customer.ServiceRequestDetails", $"service-request-details-{{requestId}}",
                new { controller = "ServiceRequest", action = "ServiceRequestDetails" });
            endpointRouteBuilder.MapControllerRoute("Customer.ServiceRequestQuoteFile", $"service-request-quote-file-{{quoteDownloadId}}",
                new { controller = "ServiceRequest", action = "GetFile" });
            endpointRouteBuilder.MapControllerRoute("Customer.ServiceShoppingCart", $"service-cart",
                new { controller = "ServiceRequest", action = "Cart" });

            endpointRouteBuilder.MapControllerRoute("Customer.AdditionalFeature", $"submit-a-query",
                new { controller = "Question", action = "SubmitQuestion" });

            endpointRouteBuilder.MapControllerRoute("Customer.UploadFileSubmitQuery", $"upload-query-submit-file",
                new { controller = "Question", action = "UploadFileSubmitQuery" });

            endpointRouteBuilder.MapControllerRoute(name: "Customer.PasswordRecovery",
                pattern: $"{lang}/passwordrecoverybyemail/",
                defaults: new { controller = "PasswordRecovery", action = "RecoverPassword" });
        }

        public int Priority => int.MaxValue;
    }
}

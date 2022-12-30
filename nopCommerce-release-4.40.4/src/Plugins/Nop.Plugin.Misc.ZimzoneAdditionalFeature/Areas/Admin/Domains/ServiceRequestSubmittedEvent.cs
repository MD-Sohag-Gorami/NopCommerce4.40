using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains
{
    public class ServiceRequestSubmittedEvent
    {
        public ServiceRequestSubmittedEvent(ZimzoneServiceRequestEntity request)
        {
            Request = request;
        }
        public ZimzoneServiceRequestEntity Request
        {
            get;
        }
    }
}

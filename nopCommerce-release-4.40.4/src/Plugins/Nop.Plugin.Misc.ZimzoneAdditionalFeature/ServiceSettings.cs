using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature
{
    public class ServiceSettings : ISettings
    {
        public int ProductTemplateId { get; set; }
        public int RequestSubmittedEmailTemplateId { get; set; }
        public int RequestAcceptedEmailTemplateId { get; set; }
    }
}

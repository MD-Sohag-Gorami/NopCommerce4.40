using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature
{
    public class ServiceAttributeSettings : ISettings
    {
        public int NameAttributeId { get; set; }
        public int EmailAttributeId { get; set; }
        public int AddressAttributeId { get; set; }
        public int DescriptionAttributeId { get; set; }
        public int FileAttributeId { get; set; }
    }
}

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains
{
    public class CustomOrderStatusChangedEvent
    {
        public CustomOrderStatusChangedEvent(OrderWithCustomStatus orderWithCustomStatus)
        {
            OrderWithCustomStatus = orderWithCustomStatus;
        }
        public OrderWithCustomStatus OrderWithCustomStatus
        {
            get;
        }
    }
}

using Nop.Core;

namespace Nop.Plugin.NopStation.OCarousels.Domains
{
    public partial class OCarouselItem : BaseEntity
    {
        public int ProductId { get; set; }

        public int OCarouselId { get; set; }

        public int DisplayOrder { get; set; }

        public virtual OCarousel OCarousel { get; set; }
    }
}

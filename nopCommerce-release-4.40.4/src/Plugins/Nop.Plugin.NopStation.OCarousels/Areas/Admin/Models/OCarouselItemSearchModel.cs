using System;
using System.Collections.Generic;
using System.Text;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.NopStation.OCarousels.Areas.Admin.Models
{
    public record OCarouselItemSearchModel : BaseSearchModel
    {
        public int OCarouselId { get; set; }
    }
}

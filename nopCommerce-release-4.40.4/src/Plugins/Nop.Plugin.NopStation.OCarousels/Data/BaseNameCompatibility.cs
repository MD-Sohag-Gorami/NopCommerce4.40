using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.NopStation.OCarousels.Domains;

namespace Nop.Plugin.NopStation.OCarousels.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(OCarousel), "NS_OCarousel" },
            { typeof(OCarouselItem), "NS_OCarouselItem" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}

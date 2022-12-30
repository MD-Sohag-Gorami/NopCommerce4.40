using System;
using System.Collections.Generic;
using System.Text;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.NopStation.OCarousels.Domains;

namespace Nop.Plugin.NopStation.OCarousels.Data
{
    public class OCarouselItemBuilder : NopEntityBuilder<OCarouselItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(OCarouselItem.OCarouselId)).AsInt32().ForeignKey<OCarousel>();
        }
    }
}

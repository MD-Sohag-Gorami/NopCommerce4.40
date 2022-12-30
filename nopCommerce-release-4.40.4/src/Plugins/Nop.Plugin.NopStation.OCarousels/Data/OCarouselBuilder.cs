using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.NopStation.OCarousels.Domains;

namespace Nop.Plugin.NopStation.OCarousels.Data
{
    public class OCarouselBuilder : NopEntityBuilder<OCarousel>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(OCarousel.Name))
                .AsString(400);
        }
    }
}
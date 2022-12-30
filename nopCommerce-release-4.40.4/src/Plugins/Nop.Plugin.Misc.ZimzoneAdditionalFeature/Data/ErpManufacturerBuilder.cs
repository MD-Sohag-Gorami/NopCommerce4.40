using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Erp;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Data
{
    public class ErpManufacturerBuilder : NopEntityBuilder<ErpManufacturer>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ErpManufacturer.NopManufacturerId))
                .AsInt32()
                .NotNullable();
        }
    }
}

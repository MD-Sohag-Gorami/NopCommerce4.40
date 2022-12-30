using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ErpSync.Domains;

namespace Nop.Plugin.Misc.ErpSync.Data
{
    public class ErpCategoryBuilder : NopEntityBuilder<ErpCategory>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ErpCategory.NopCategoryId))
                .AsInt32()
                .NotNullable();
        }
    }
}

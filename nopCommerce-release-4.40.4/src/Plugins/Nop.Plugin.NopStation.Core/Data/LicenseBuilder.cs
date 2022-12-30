using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.NopStation.Core.Domains;

namespace Nop.Plugin.NopStation.Core.Data
{
    public class LicenseBuilder : NopEntityBuilder<License>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.
                WithColumn(nameof(License.Key)).AsString(2000);
        }
    }
}

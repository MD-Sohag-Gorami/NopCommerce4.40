using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.ErpSync.Domains;

namespace Nop.Plugin.Misc.ErpSync.Data
{
    public class ErpProductPictureBuilder : NopEntityBuilder<ErpProductPicture>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ErpProductPicture.NopProductId))
                .AsInt32()
                .NotNullable()
                .WithColumn(nameof(ErpProductPicture.CreatedDate))
                .AsDateTime()
                .Nullable()
                .WithColumn(nameof(ErpProductPicture.ModifiedDate))
                .AsDateTime()
                .Nullable()
                .WithColumn(nameof(ErpProductPicture.ImageLink))
                .AsString()
                .NotNullable();
        }
    }
}

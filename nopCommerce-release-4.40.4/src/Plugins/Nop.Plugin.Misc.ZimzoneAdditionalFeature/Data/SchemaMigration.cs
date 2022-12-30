using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.ErpSync.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Erp;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Query;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Data
{
    [SkipMigrationOnUpdate]
    [SkipMigrationOnInstall]
    [NopMigration("2022/03/14 09:44:10", "Zimzone Additional Feature Schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        #region Fields

        protected IMigrationManager _migrationManager;

        #endregion

        #region Ctor

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            _migrationManager.BuildTable<ZimzoneServiceRequestEntity>(Create);
            _migrationManager.BuildTable<ZimzoneServiceEntity>(Create);
            _migrationManager.BuildTable<ErpProductPicture>(Create);
            _migrationManager.BuildTable<ErpCategory>(Create);
            _migrationManager.BuildTable<Question>(Create);
            _migrationManager.BuildTable<CustomOrderStatus>(Create);
            _migrationManager.BuildTable<OrderWithCustomStatus>(Create);
            _migrationManager.BuildTable<ErpManufacturer>(Create);
        }

        #endregion
    }
}

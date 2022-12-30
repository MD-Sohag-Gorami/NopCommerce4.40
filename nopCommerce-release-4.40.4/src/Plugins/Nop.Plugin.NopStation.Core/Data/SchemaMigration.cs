using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.NopStation.Core.Domains;

namespace Nop.Plugin.NopStation.Core.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2021/07/25 09:55:55:1687542", "NopStation.Core base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;
        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }
        public override void Up()
        {
            _migrationManager.BuildTable<License>(Create);
        }
    }
}

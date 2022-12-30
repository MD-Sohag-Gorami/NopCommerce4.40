using System;
using System.Collections.Generic;
using System.Text;
using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.NopStation.OCarousels.Domains;

namespace Nop.Plugin.NopStation.OCarousels.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2020/07/08 08:41:55:1687543", "NopStation.OCarousels base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;
        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }
        public override void Up()
        {
            _migrationManager.BuildTable<OCarousel>(Create);
            _migrationManager.BuildTable<OCarouselItem>(Create);
        }
    }
}

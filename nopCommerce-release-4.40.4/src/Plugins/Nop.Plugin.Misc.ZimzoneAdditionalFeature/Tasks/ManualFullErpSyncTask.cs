using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Misc.ErpSync.Tasks
{
    public class ManualFullErpSyncTask : IScheduleTask
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IZimZoneErpSyncService _zimZoneErpSyncService;
        private readonly ILogger _logger;

        public ManualFullErpSyncTask(IStoreContext storeContext,
                                     ISettingService settingService,
                                     IZimZoneErpSyncService zimZoneErpSyncService,
                                     ILogger logger)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _zimZoneErpSyncService = zimZoneErpSyncService;
            _logger = logger;
        }
        public async Task ExecuteAsync()
        {
            await _logger.InsertLogAsync(LogLevel.Information, $"Manual Full Erp Sync Started at local time - {DateTime.Now}");
            //load settings for a chosen store scope
            var storeScope = (await _storeContext.GetCurrentStoreAsync()).Id ;
            var erpSyncSettings = await _settingService.LoadSettingAsync<ErpSyncSettings>(storeScope);
            await _zimZoneErpSyncService.SyncAsync(erpSyncSettings.ManualSyncFrom);

            await _logger.InsertLogAsync(LogLevel.Information, $"Manual Full Erp Sync Finished at local time - {DateTime.Now}");
        }
    }
}

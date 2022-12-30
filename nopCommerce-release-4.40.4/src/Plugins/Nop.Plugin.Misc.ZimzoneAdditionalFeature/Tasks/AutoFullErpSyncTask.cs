using System;
using System.Linq;
using System.Text;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Misc.ErpSync.Tasks
{
    public class AutoFullErpSyncTask : IScheduleTask
    {
        private readonly IZimZoneErpSyncService _zimZoneErpSyncService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly IDateTimeHelper _dateTimeHelper;

        public AutoFullErpSyncTask(IZimZoneErpSyncService zimZoneErpSyncService,
                               IStoreContext storeContext,
                               ISettingService settingService,
                               ILogger logger,
                               IDateTimeHelper dateTimeHelper)
        {
            _zimZoneErpSyncService = zimZoneErpSyncService;
            _storeContext = storeContext;
            _settingService = settingService;
            _logger = logger;
            _dateTimeHelper = dateTimeHelper;
        }
        public async Task ExecuteAsync()
        {
            //load settings for a chosen store scope
            var storeScope = (await _storeContext.GetCurrentStoreAsync()).Id;
            var erpSyncSettings = await _settingService.LoadSettingAsync<ErpSyncSettings>(storeScope);

            try
            {
                if (erpSyncSettings != null && (!string.IsNullOrWhiteSpace(erpSyncSettings.SyncTime) && erpSyncSettings.SyncTime.Contains(':')) && !erpSyncSettings.IsRunningSync)
                {
                    //execution
                    var syncTime = erpSyncSettings.SyncTime.Split(':').ToList();
                    var currentTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.UtcNow, DateTimeKind.Utc);
                    var nextExecutionTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, int.Parse(syncTime[0]), int.Parse(syncTime[1]), 0);

                    if (erpSyncSettings.NextSync == null)
                    {
                        erpSyncSettings.NextSync = nextExecutionTime;
                        await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.NextSync, true, storeScope, true);
                    }

                    if (erpSyncSettings.NextSync != null && DateTime.Compare(erpSyncSettings.NextSync.Value, currentTime) <= 0)
                    {
                        await _logger.InsertLogAsync(LogLevel.Information, $"Auto Full Erp Sync Started at local time - {currentTime}");

                        erpSyncSettings.IsRunningSync = true;
                        await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.IsRunningSync, true, storeScope, true);

                        // start sync
                        await _zimZoneErpSyncService.SyncAsync(erpSyncSettings.ManualSyncFrom);

                        // update settings
                        erpSyncSettings.LastSync = currentTime;
                        erpSyncSettings.NextSync = nextExecutionTime.AddDays(1);
                        erpSyncSettings.IsRunningSync = false;

                        await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.LastSync, true, storeScope, true);
                        await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.NextSync, true, storeScope, true);
                        await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.IsRunningSync, true, storeScope, true);

                        // add log that auto full erp sycn finished successfully
                        var fullMessage = new StringBuilder();
                        fullMessage.AppendLine("Auto Full Erp Sync Finished successfully.");
                        fullMessage.AppendLine($"NextSync value is set to -> {erpSyncSettings.NextSync}");
                        fullMessage.AppendLine($"LastSync value is set to -> {erpSyncSettings.LastSync}");
                        fullMessage.AppendLine($"IsRunning value is set to -> {erpSyncSettings.IsRunningSync}");


                        await _logger.InsertLogAsync(LogLevel.Information, $"Auto Full Erp Sync Finished at local time - {await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.UtcNow, DateTimeKind.Utc)}", fullMessage.ToString());
                    }
                }
                else if (erpSyncSettings.IsRunningSync)
                {
                    await _logger.InsertLogAsync(LogLevel.Information, "Erp Synchronization is already running.", "There is a synchronization running. You can't run again untill it is finished.");
                }
                else
                {
                    await _logger.InsertLogAsync(LogLevel.Information, "Erp Synchronization couldn't start.", "There is a problem in processing the Sync Time. Check in Erp Sync Configure page.");
                }
            }
            catch(Exception e)
            {
                erpSyncSettings.IsRunningSync = false;
                await _settingService.SaveSettingOverridablePerStoreAsync(erpSyncSettings, x => x.IsRunningSync, true, storeScope, true);
            }
        }
    }
}

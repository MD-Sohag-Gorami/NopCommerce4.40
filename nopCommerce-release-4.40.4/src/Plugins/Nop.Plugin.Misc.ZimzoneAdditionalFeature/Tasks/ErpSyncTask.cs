using System;
using Nop.Plugin.Misc.ErpSync.Services;
using Nop.Services.Helpers;
using Nop.Services.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Nop.Plugin.Misc.ErpSync.Tasks
{
    public class ErpSyncTask : IScheduleTask
    {

        #region Fields

        private readonly IZimZoneErpSyncService _zimZoneErpSyncService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public ErpSyncTask(IZimZoneErpSyncService zimZoneErpSyncService,
                           IScheduleTaskService scheduleTaskService,
                           IDateTimeHelper dateTimeHelper)
        {
            _zimZoneErpSyncService = zimZoneErpSyncService;
            _scheduleTaskService = scheduleTaskService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute task
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ExecuteAsync()
        {
            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(ErpSyncDefaults.SynchronizationTaskType);

            var lastUpdatedTime = await _dateTimeHelper.ConvertToUserTimeAsync(scheduleTask.LastSuccessUtc.Value, DateTimeKind.Utc);

            await _zimZoneErpSyncService.SyncAsync(lastUpdatedTime);
        }

        #endregion
    }
}

INSERT INTO [dbo].[ScheduleTask](Name, Seconds, Type, Enabled, StopOnError)
VALUES('Full ERP Sync', '2592000', 'Nop.Plugin.Misc.ErpSync.Tasks.FullErpSyncTask', '0', '0')

GO

/* 19-10-2022 */

INSERT INTO [dbo].[ScheduleTask](Name, Seconds, Type, Enabled, StopOnError)
VALUES('Manual Full ERP Sync', '2592000', 'Nop.Plugin.Misc.ErpSync.Tasks.ManualFullErpSyncTask', '0', '0')
GO

UPDATE [dbo].[ScheduleTask]
SET Name = 'Auto Full ERP Sync', Type = 'Nop.Plugin.Misc.ErpSync.Tasks.AutoFullErpSyncTask', Seconds = '300'
WHERE Name = 'Full ERP Sync' AND Type = 'Nop.Plugin.Misc.ErpSync.Tasks.FullErpSyncTask'
GO


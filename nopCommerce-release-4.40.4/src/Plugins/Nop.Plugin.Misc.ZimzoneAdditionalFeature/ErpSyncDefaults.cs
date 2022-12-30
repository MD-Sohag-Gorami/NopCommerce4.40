namespace Nop.Plugin.Misc.ErpSync
{
    public static class ErpSyncDefaults
    {
        public static string SynchronizationTaskType => "Nop.Plugin.Misc.ErpSync.Tasks.ErpSyncTask";
        public static string AutoFullErpSyncTaskType => "Nop.Plugin.Misc.ErpSync.Tasks.AutoFullErpSyncTask";
        public static string ManualFullErpSyncTaskType => "Nop.Plugin.Misc.ErpSync.Tasks.ManualFullErpSyncTask";
        public static string SynchronizationTaskName => "ERP Sync";
        public static string AutoFullErpSyncTaskName => "Auto Full ERP Sync";
        public static string ManualFullErpSyncTaskName => "Manual Full ERP Sync";
        public static string DefaultRootCategoryName => "ECOMMERCE";
    }
}

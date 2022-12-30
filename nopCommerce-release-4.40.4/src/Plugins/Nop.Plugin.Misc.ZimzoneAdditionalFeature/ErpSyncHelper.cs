namespace Nop.Plugin.Misc.ErpSync
{
    public static class ErpSyncHelper
    {
        public static string PrepareImageUrlWithSize(string imagePath, int pictureSize, ErpSyncSettings erpSyncSettings)
        {
            if (imagePath.Contains(erpSyncSettings.ImageUrlEndpoint))
            {
                imagePath = imagePath.Replace(erpSyncSettings.ImageUrlEndpoint, "");
                imagePath = $"{erpSyncSettings.ImageUrlEndpoint}tr:w-{pictureSize},h-{pictureSize}/{imagePath}";
            }
            return imagePath;
        }
    }
}

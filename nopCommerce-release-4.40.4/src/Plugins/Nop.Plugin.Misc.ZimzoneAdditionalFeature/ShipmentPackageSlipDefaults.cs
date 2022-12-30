using iTextSharp.text;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature
{
    public static class ShipmentPackageSlipDefaults
    {
        public static Rectangle A5Portrait => PageSize.A5;
        public static Rectangle A5Landscape => new Rectangle(594f, 420f);
        public static Rectangle A6Portrait => PageSize.A6;
        public static Rectangle A6Landscape => new Rectangle(420f, 297f);
    }
}

using Nop.Core;

namespace Nop.Plugin.Misc.ErpSync.Domains
{
    public class ErpCategory : BaseEntity
    {
        public string ErpId { get; set; }

        public string Name { get; set; }

        public bool LowestLevelCategory { get; set; }

        public string RootCategory { get; set; }

        public float V { get; set; }

        public string ParentId { get; set; }

        public int NopCategoryId { get; set; }
    }
}

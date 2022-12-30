using System.Threading.Tasks;
using Nop.Plugin.NopStation.Core.Domains;
using Nop.Plugin.NopStation.Core.Caching;
using Nop.Services.Caching;

namespace Nop.Plugin.NopStation.Core.Services.Cache
{
    public partial class LicenseCacheEventConsumer : CacheEventConsumer<License>
    {
        protected override async Task ClearCacheAsync(License entity)
        {
            await RemoveByPrefixAsync(NopStationEntityCacheDefaults<License>.Prefix);
            await RemoveByPrefixAsync(CoreCacheDefaults.LicensePrefix);
        }
    }
}
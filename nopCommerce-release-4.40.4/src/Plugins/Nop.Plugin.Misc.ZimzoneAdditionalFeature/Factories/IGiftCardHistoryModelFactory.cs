using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Factories
{
    public interface IGiftCardHistoryModelFactory
    {
        Task<IList<GiftCardHistoryModel>> PrepareGiftCardHistoryModelsAsync();
    }
}
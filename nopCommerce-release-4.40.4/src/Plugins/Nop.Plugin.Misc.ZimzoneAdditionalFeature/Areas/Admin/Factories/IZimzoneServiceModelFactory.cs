using System.Threading.Tasks;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public interface IZimzoneServiceModelFactory
    {
        Task<ServiceListModel> PrepareServiceListModelAsync(ServiceSearchModel searchModel);
    }
}
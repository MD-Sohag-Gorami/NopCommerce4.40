using System.Threading.Tasks;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Query;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public interface IQueryModelFactory
    {
        Task<QuestionListModel> PrepareQueryListModelAsync(QuestionSearchModel searchModel);
        Task<QuestionSearchModel> PrepareQuerySearchModelAsync();
    }
}
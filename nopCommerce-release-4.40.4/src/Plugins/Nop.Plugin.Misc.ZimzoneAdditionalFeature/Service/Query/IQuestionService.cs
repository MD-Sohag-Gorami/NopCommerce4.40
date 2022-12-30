using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Query;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Query;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.Query
{
    public interface IQuestionService
    {
        Task<IList<Question>> GetAllQuestionsAsync(QuestionSearchModel searchModel);
        Task<Question> GetQuestionByIdAsync(int id);
        Task InsertQuestionAsync(Question question);
        Task UpdateQuestionAsync(Question question);
        Task DeleteQuestionAsync(Question question);
    }
}
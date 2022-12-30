using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Events;
using Nop.Data;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Query;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Query;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.Query
{
    public class QuestionService : IQuestionService
    {
        private readonly IRepository<Question> _repository;
        private readonly IEventPublisher _eventPublisher;

        public QuestionService(IRepository<Question> repository,
            IEventPublisher eventPublisher)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
        }
        public async Task DeleteQuestionAsync(Question question)
        {
            await _repository.DeleteAsync(question);
        }

        public async Task<IList<Question>> GetAllQuestionsAsync(QuestionSearchModel searchModel)
        {
            var query = _repository.Table.Where(x => !x.Deleted);
            if (!string.IsNullOrEmpty(searchModel.CustomerEmail))
            {
                query = query.Where(x => x.Email == searchModel.CustomerEmail);
            }
            if (!string.IsNullOrEmpty(searchModel.CustomerName))
            {
                query = query.Where(x => x.FirstName.Contains(searchModel.CustomerName) || x.LastName.Contains(searchModel.CustomerName) || x.FirstName + " " + x.LastName == searchModel.CustomerName);
            }
            if (searchModel.StatusId != 0)
            {
                query = searchModel.StatusId == 1 ? query.Where(x => !x.MarkedAsRead) : query.Where(x => x.MarkedAsRead);
            }
            query = query.OrderByDescending(x => x.Id);
            var questions = query.ToList();
            return await Task.FromResult(questions);
        }

        public async Task<Question> GetQuestionByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task InsertQuestionAsync(Question question)
        {
            await _repository.InsertAsync(question);
            await _eventPublisher.PublishAsync(new QuestionSubmittedEvent(question));
        }

        public async Task UpdateQuestionAsync(Question question)
        {
            await _repository.UpdateAsync(question);
        }
    }
}

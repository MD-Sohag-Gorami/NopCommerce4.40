using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Models.Query;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Query;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service.Query;
using Nop.Services.Helpers;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Areas.Admin.Factories
{
    public class QueryModelFactory : IQueryModelFactory
    {
        private readonly IQuestionService _questionService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public QueryModelFactory(IQuestionService questionService,
                                 IDateTimeHelper dateTimeHelper)
        {
            _questionService = questionService;
            _dateTimeHelper = dateTimeHelper;
        }
        public async Task<QuestionListModel> PrepareQueryListModelAsync(QuestionSearchModel searchModel)
        {
            var queries = await _questionService.GetAllQuestionsAsync(searchModel);
            var queryList = new PagedList<Question>(queries, 0, searchModel.PageSize);
            var model = await new QuestionListModel().PrepareToGridAsync(searchModel, queryList, () =>
            {
                return queryList.SelectAwait(async q =>
                {
                    var query = new QuestionModel
                    {
                        Id = q.Id,
                        Email = q.Email,
                        FullName = $"{q.FirstName} {q.LastName}",
                        MarkedAsRead = q.MarkedAsRead,
                        ProductName = q.ProductName,
                        CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(q.CreatedOnUtc, DateTimeKind.Utc)
                    };
                    return await Task.FromResult(query);
                });
            });

            return model;
        }

        public async Task<QuestionSearchModel> PrepareQuerySearchModelAsync()
        {
            return await Task.FromResult(new QuestionSearchModel
            {
                AvailablePageSizes = "10,20,50,100",
                AvailableStatus = new List<SelectListItem>()
                {
                    new SelectListItem
                    {
                        Text="All",
                        Value="0"
                    },
                    new SelectListItem
                    {
                        Text="Pending",
                        Value="1"
                    },
                    new SelectListItem
                    {
                        Text="Reviewed",
                        Value="2"
                    },
                }
            });
        }
    }
}

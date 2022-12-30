using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains;
using Nop.Plugin.Misc.ZimzoneAdditionalFeature.Domains.Query;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.ZimzoneAdditionalFeature.Service
{
    public interface IServiceRequestNotificationService
    {
        Task<IList<int>> SendServiceRequestSubmittedNotificationAsync(ZimzoneServiceRequestEntity serviceRequest);
        Task<IList<int>> SendServiceRequestAcceptedNotificationAsync(ZimzoneServiceRequestEntity serviceRequest);
        Task AddServiceRequestTokensAsync(IList<Token> tokens, ZimzoneServiceRequestEntity serviceRequest);

        Task<IList<int>> SendQuerySubmittedNotificationAsync(Question question);
        Task<IList<int>> SendQuerySubmittedAdminNotificationAsync(Question question);
    }
}
